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
        static IRedisDbRepository<OcrResponse> _redisDbRepository;

        static void Main(string[] args)
        {
            Console.WriteLine("Starting Image Stream Reader");
            List<string> imageStreams = new List<string>();
            InitializeAsync().GetAwaiter().GetResult();

            while (true)
            {
                imageStreams = _mediaManager.GetLiveStreamsAsync().GetAwaiter().GetResult();
                ListenInParallel(imageStreams);
                PublishResults(imageStreams);                
                Thread.Sleep(_pollingInterval);
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
            _redisDbRepository.StringSet(serialNumber, ocrRetrievedText);

            if (ocrRetrievedText.ScreenName != ScreenName.Unknown.ToString())
            {
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

            _redisDbRepository = new RedisDbRepository<OcrResponse>(_configurationCache);
        }

        private static void PublishResults(List<string> imageStreams)
        {
            string activeStreams = JsonConvert.SerializeObject(imageStreams);            
            _redisDbRepository.PublishAsync(Definitions.UpdatedRLMDevices, activeStreams);
            _azureCosmosDB = new AzureCosmosDB(_configurationCache);
            _azureCosmosDB.SetContext(_configurationCache.GetConfigurationItem("mediamanager", "ocrdatabasename"), 
                _configurationCache.GetConfigurationItem("mediamanager", "ocrcollectionname"));
        }
    }
}