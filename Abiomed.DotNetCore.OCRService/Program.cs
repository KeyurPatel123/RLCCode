using Abiomed.DotNetCore.Business;
using Abiomed.DotNetCore.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;
using Abiomed.DotNetCore.Storage;
using System.Collections.Generic;

namespace Abiomed.DotNetCore.OCRService
{
    class Program
    {
        static int _pollingInterval = 0;
        static IMediaManager _mediaManager;

        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            Console.WriteLine("Starting Image Stream Reader");
            await Initialize();

            while (true)
            {
                Task.Run(async () =>
                {
                    await Listen();
                }).GetAwaiter().GetResult();
                Thread.Sleep(_pollingInterval);
            }
        }

        public static async Task Listen()
        {
            List<string> imageStreams = await _mediaManager.GetLiveStreamsAsync();
            foreach (string serialNumber in imageStreams)
            {
                var thumbnail = await _mediaManager.GetImageTextAsync(serialNumber);
            }
        }

        static private async Task Initialize()
        {
            ITableStorage tableStorage = new TableStorage();
            ConfigurationManager configurationManager = new ConfigurationManager(tableStorage);
            IConfigurationCache configurationCache = new ConfigurationCache(configurationManager);
            await configurationCache.LoadCache();

            _mediaManager = new MediaManager(configurationCache);
            _pollingInterval = configurationCache.GetNumericConfigurationItem("mediamanager", "pollinginterval");
        }
    }
}