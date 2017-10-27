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
        static List<string> _validCases = new List<string>();

        static void Main(string[] args)
        {
            Console.WriteLine("Starting Image Stream Reader");
            List<string> imageStreams = new List<string>();
            InitializeAsync().GetAwaiter().GetResult();
            var pollingTime = _configurationCache.GetNumericConfigurationItem("ocrmanager", "polloinginterval");

            while (true)
            {
                imageStreams = _mediaManager.GetLiveStreamsAsync().GetAwaiter().GetResult();
                ListenInParallel(imageStreams).Wait();
                PublishResults();
                Thread.Sleep(pollingTime);
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
            Console.WriteLine("Serial {0} is Demo {1}", serialNumber, ocrRetrievedText.IsDemo);

            if (ocrRetrievedText.ScreenName != ScreenName.Unknown.ToString() && ocrRetrievedText.IsDemo == "false")
            {
                _validCases.Add(serialNumber);
                var jsonOcr = JsonConvert.SerializeObject(ocrRetrievedText);
                await _redisDbRepositoryOcrResponse.StringSetAsync(serialNumber + ":OCR", jsonOcr, true);                
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
        }

        private static void PublishResults()
        {
            string activeStreams = JsonConvert.SerializeObject(_validCases);
            _validCases.Clear();
            _redisDbRepositoryOcrResponse.PublishAsync(Definitions.UpdatedRemoteLinkCases, activeStreams);
            _azureCosmosDB = new AzureCosmosDB(_configurationCache);
            _azureCosmosDB.SetContext(_configurationCache.GetConfigurationItem("mediamanager", "ocrdatabasename"), 
                _configurationCache.GetConfigurationItem("mediamanager", "ocrcollectionname"));
        }
    }
}