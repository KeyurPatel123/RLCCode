/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * KeepAliveManager.cs: Keep Alive Manager
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using Abiomed.DotNetCore.Models;
using Abiomed.DotNetCore.Configuration;
using Abiomed.DotNetCore.Repository;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Timers;

namespace Abiomed.DotNetCore.Business
{
    public class KeepAliveManager : IKeepAliveManager
    {
        private ConcurrentDictionary<string, KeepAliveTimer> _rlmConnections = new ConcurrentDictionary<string, KeepAliveTimer>();
        private ConcurrentDictionary<string, KeepAliveTimer> _rlmImageCountdown = new ConcurrentDictionary<string, KeepAliveTimer>();
        private IRedisDbRepository<RLMDevice> _redisDbRepository;
        private ILogger<IKeepAliveManager> _logger;
        private int _keepAliveTimer;
        private int _imageCountDownTimer;
        private IConfigurationCache _configurationCache;

        public KeepAliveManager(IRedisDbRepository<RLMDevice> redisDbRepository, ILogger<IKeepAliveManager> logger, IConfigurationCache configurationCache)
        {
            _redisDbRepository = redisDbRepository;
            _logger = logger;
            _configurationCache = configurationCache;

            _keepAliveTimer = _configurationCache.GetNumericConfigurationItem("optionsmanager", "keepalivetimer");
            _imageCountDownTimer = _configurationCache.GetNumericConfigurationItem("optionsmanager", "imagecountdowntimer");
        }

        public void Add(string deviceIpAddress)
        {
            KeepAliveTimer keepAliveTimer = new KeepAliveTimer(deviceIpAddress, _keepAliveTimer, TimerExpiredCallback);
            _rlmConnections.TryAdd(deviceIpAddress, keepAliveTimer);
        }

        public void Remove(string deviceIpAddress)
        {
            KeepAliveTimer keepAliveTimer;
            _rlmConnections.TryRemove(deviceIpAddress, out keepAliveTimer);
        }

        public void Ping(string deviceIpAddress)
        {
            KeepAliveTimer keepAliveTimer;
            _rlmConnections.TryGetValue(deviceIpAddress, out keepAliveTimer);
            
            if(keepAliveTimer != null)
            {
                keepAliveTimer.PingTimer();
            }            
        }
        
        private void TimerExpiredCallback(object sender, ElapsedEventArgs e, string deviceIpAddress)
        {
            // Destroy Timer, Remove from list, and broadcast message
            KeepAliveTimer keepAliveTimer;
            _rlmConnections.TryGetValue(deviceIpAddress, out keepAliveTimer);

            _logger.LogInformation("Keep Alive Timer Expired IP Address {1}", deviceIpAddress);

            if (keepAliveTimer != null)
            {
                keepAliveTimer.DestroyTimer();
            }

            Remove(deviceIpAddress);

            _redisDbRepository.Publish(Definitions.RemoveRLMDeviceRLR, deviceIpAddress);
        }

        private void ImageCounterTimerExpiredCallback(object sender, ElapsedEventArgs e, string deviceIpAddress)
        {
            KeepAliveTimer keepAliveTimer;
            _rlmConnections.TryGetValue(deviceIpAddress, out keepAliveTimer);

            // Request New Image
            //todo _redisDbRepository.Publish(Definitions.ScreenCaptureIndicationEvent, deviceIpAddress);
        }

        public void ImageTimerAdd(string deviceIpAddress)
        {            
            KeepAliveTimer keepAliveTimer = new KeepAliveTimer(deviceIpAddress, _imageCountDownTimer, ImageCounterTimerExpiredCallback);
            _rlmImageCountdown.TryAdd(deviceIpAddress, keepAliveTimer);
        }

        public void ImageTimerDelete(string deviceIpAddress)
        {
            KeepAliveTimer keepAliveTimer;
            _rlmImageCountdown.TryRemove(deviceIpAddress, out keepAliveTimer);
        }
    }    
}
