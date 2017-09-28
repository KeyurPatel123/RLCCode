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

namespace Abiomed.DotNetCore.OCRService
{
    class Program
    {
        static int _pollingInterval = 0;
        static DateTime _batchStartTimeUtc;
        static IMediaManager _mediaManager;
        static IAzureCosmosDB _azureCosmosDB;

        static void Main(string[] args)
        {
            Console.WriteLine("Starting Image Stream Reader");
            List<string> imageStreams = new List<string>();
            InitializeAsync().GetAwaiter().GetResult();

            while (true)
            {
                imageStreams = _mediaManager.GetLiveStreamsAsync().GetAwaiter().GetResult();
                ListenInParallel(imageStreams);
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

        private static async Task<OcrResponse> ProcessStreams(string serialNumber)
        {
            var ocrRetrievedText = await _mediaManager.GetImageTextAsync(serialNumber, _batchStartTimeUtc);
            await _azureCosmosDB.AddAsync(ocrRetrievedText);
            return ocrRetrievedText;
        }

        private static async Task InitializeAsync()
        {
            ITableStorage tableStorage = new TableStorage();
            ConfigurationManager configurationManager = new ConfigurationManager(tableStorage);
            IConfigurationCache configurationCache = new ConfigurationCache(configurationManager);
            await configurationCache.LoadCacheAsync();

            _mediaManager = new MediaManager(configurationCache);
            _pollingInterval = configurationCache.GetNumericConfigurationItem("mediamanager", "pollinginterval");

            _azureCosmosDB = new AzureCosmosDB(configurationCache);
            _azureCosmosDB.SetContext(configurationCache.GetConfigurationItem("mediamanager", "ocrdatabasename"), 
                configurationCache.GetConfigurationItem("mediamanager", "ocrcollectionname"));
        }
    }
}