using Abiomed.DotNetCore.Business;
using Abiomed.DotNetCore.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;
using Abiomed.DotNetCore.Storage;
using System.Collections.Generic;
using System.Linq;
using Abiomed.DotNetCore.Repository;
using Abiomed.DotNetCore.Models;
using StackExchange.Redis;
using Newtonsoft.Json;

namespace Abiomed.DotNetCore.OCRService
{
    class Program
    {
        static int _pollingInterval = 0;
        static DateTime _batchStartTimeUtc;
        static IMediaManager _mediaManager;
        static IAzureCosmosDB _azureCosmosDB;
        static IConfigurationCache _configurationCache;
        static IRedisDbRepository<OcrResponse> _redisDbRepositoryOcrResponse;
        static IRedisDbRepository<Case> _redisDbRepositoryCase;

        static void Main(string[] args)
        {
            Console.WriteLine("Starting Image Stream Reader");
            List<string> imageStreams = new List<string>();
            InitializeAsync().GetAwaiter().GetResult();

            //  TODO Refine and make configurable - leaving this way to to test...
            //  3600000 is equal to 1-hour in ms - so the check is for every hour
            //  the hours to go back (the -4) needs to be configurable as well.
            //  TODO POC Type Code, to prove out the removal.
            var iterationsUntilClean = 3600000/_pollingInterval;
            var iterations = 0;

            while (true)
            {
                if (iterations>iterationsUntilClean)
                {
                    CaseManager.CleanupExpiredCases(_redisDbRepositoryCase, DateTime.UtcNow.AddHours(-4));
                    iterations = 0;
                }
                iterations++;
                imageStreams = _mediaManager.GetLiveStreamsAsync().GetAwaiter().GetResult();
                ListenInParallel(imageStreams);
                PublishResults(imageStreams);  

                Thread.Sleep(int.MaxValue);
            }
        }

        public static Task ListenInParallel(List<string> imageStreams)
        {
            _batchStartTimeUtc = DateTime.UtcNow;
            List<string> results = new List<string>();
            var tasks = imageStreams.Select(i =>
            {
                return ProcessStreams(i);
            });

            return Task.WhenAll(tasks);
        }

        private static async Task ProcessStreams(string serialNumber)
        {
            var ocrRetrievedText = await _mediaManager.GetImageTextAsync(serialNumber, _batchStartTimeUtc);
            _redisDbRepositoryOcrResponse.StringSet(serialNumber, ocrRetrievedText);

            if (ocrRetrievedText.ScreenName != ScreenName.Unknown.ToString())
            {
                CaseManager.AddOrUpdate(_redisDbRepositoryCase, ocrRetrievedText);
                await _azureCosmosDB.AddAsync(ocrRetrievedText);
            }
        }

        private static async Task InitializeAsync()
        {
            ITableStorage tableStorage = new TableStorage();
            ConfigurationManager configurationManager = new ConfigurationManager(tableStorage);
            _configurationCache = new ConfigurationCache(configurationManager);
            await _configurationCache.LoadCacheAsync();

            _mediaManager = new MediaManager(_configurationCache);
            _pollingInterval = _configurationCache.GetNumericConfigurationItem("mediamanager", "pollinginterval");

            _azureCosmosDB = new AzureCosmosDB(_configurationCache);
            _azureCosmosDB.SetContext(_configurationCache.GetConfigurationItem("mediamanager", "ocrdatabasename"), 
                _configurationCache.GetConfigurationItem("mediamanager", "ocrcollectionname"));

            _redisDbRepositoryOcrResponse = new RedisDbRepository<OcrResponse>(_configurationCache);
            _redisDbRepositoryCase = new RedisDbRepository<Case>(_configurationCache);
        }

        private static void PublishResults(List<string> imageStreams)
        {
            string activeStreams = JsonConvert.SerializeObject(imageStreams);            
            _redisDbRepositoryOcrResponse.PublishAsync(Definitions.UpdatedRLMDevices, activeStreams);
            _azureCosmosDB = new AzureCosmosDB(_configurationCache);
            _azureCosmosDB.SetContext(_configurationCache.GetConfigurationItem("mediamanager", "ocrdatabasename"), 
                _configurationCache.GetConfigurationItem("mediamanager", "ocrcollectionname"));
        }
    }
}