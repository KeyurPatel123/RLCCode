using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abiomed.DotNetCore.Configuration;
using Abiomed.DotNetCore.Repository;
using Abiomed.DotNetCore.Models;
using StackExchange.Redis;
using System.Collections.Concurrent;

namespace Abiomed.WirelessRemoteLink
{
    public class DeviceManager : IDeviceManager
    {
        private ConfigurationCache _configurationCache;
        private IRedisDbRepository<OcrResponse> _redisDbRepository;
        private string[] _activeDevices;
        private RLMDevices _rlmDevices = new RLMDevices();

        public DeviceManager(ConfigurationCache configurationCache, IRedisDbRepository<OcrResponse> redisDbRepository)
        {
            _configurationCache = configurationCache;
            _redisDbRepository = redisDbRepository;
            Init();
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

        private void UpdatedDevices(RedisValue message)
        {
            // Ensure list is current. todo look at RedisValue
            

            // Updated devices
            GetDevices();
        }

        private async void Init()
        {
            // Get Current Active List of RLM's, Get all devices in store into local storage and subscribe to updates
            //_activeDevices = await _redisDbRepository.GetSetAsync(Definitions.RLMDeviceSetWOWZA);

            //await GetDevices();

            await _redisDbRepository.SubscribeAsync(Definitions.UpdatedRLMDevices, (channel, message) =>
            {
                UpdatedDevices(message);
            });
        }

        public RLMDevices GetRlmDevices()
        {
            return _rlmDevices;
        }
    }
}
