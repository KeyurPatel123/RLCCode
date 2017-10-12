using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abiomed.DotNetCore.Configuration;
using Abiomed.DotNetCore.Repository;
using Abiomed.DotNetCore.Models;
using StackExchange.Redis;
using System.Collections.Concurrent;
using Newtonsoft.Json;

namespace Abiomed.WirelessRemoteLink
{
    public class DeviceManager : IDeviceManager
    {
        private ConfigurationCache _configurationCache;
        private IRedisDbRepository<OcrResponse> _redisDbRepository;
        private List<string> _activeDevices;
        private RLMDevices _rlmDevices = new RLMDevices();

        public DeviceManager(ConfigurationCache configurationCache, IRedisDbRepository<OcrResponse> redisDbRepository)
        {
            _configurationCache = configurationCache;
            _redisDbRepository = redisDbRepository;
            InitAsync().Wait();
        }
    
        private Task GetDevices()
        {
            var tasks = _activeDevices.Select(i =>
            {
                return GetRLMData(i);
            });
            return Task.WhenAll(tasks);
        }

        private async Task GetRLMData(string serialNumber)
        {
            var rlmData = await _redisDbRepository.StringGetAsync(serialNumber);
            _rlmDevices.Devices[serialNumber] = rlmData;
        }

        private async Task UpdatedDevicesAsync(List<string> devices)
        {
            // Ensure list is current. todo look at RedisValue
            _activeDevices = devices;

            // Update devices
            await GetDevices();

            // Todo - verify what devices are still in the list, if they have expired
        }

        private async Task InitAsync()
        {
            await _redisDbRepository.SubscribeAsync(Definitions.UpdatedRLMDevices, async (channel, message) =>
            {
                var devices = (List<string>)JsonConvert.DeserializeObject<List<string>>(message);                
                await UpdatedDevicesAsync(devices);
            });
        }

        public RLMDevices GetRlmDevices()
        {
            return _rlmDevices;
        }
    }
}
