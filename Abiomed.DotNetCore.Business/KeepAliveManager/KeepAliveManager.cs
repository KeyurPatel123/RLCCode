/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * KeepAliveManager.cs: Keep Alive Manager
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using Abiomed.DotNetCore.Models;
using Abiomed.DotNetCore.Repository;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Timers;

namespace Abiomed.DotNetCore.Business
{
    public class KeepAliveManager : IKeepAliveManager
    {
        private Models.Configuration _configuration;
        private ConcurrentDictionary<string, KeepAliveTimer> _rlmConnections = new ConcurrentDictionary<string, KeepAliveTimer>();
        private ConcurrentDictionary<string, KeepAliveTimer> _rlmImageCountdown = new ConcurrentDictionary<string, KeepAliveTimer>();
        private IRedisDbRepository<RLMDevice> _redisDbRepository;
        private ILogger<IKeepAliveManager> _logger;

        public KeepAliveManager(Models.Configuration configuration, IRedisDbRepository<RLMDevice> redisDbRepository, ILogger<IKeepAliveManager> logger)
        {
            _configuration = configuration;
            _redisDbRepository = redisDbRepository;
            _logger = logger;
        }

        public void Add(string deviceIpAddress)
        {
            KeepAliveTimer keepAliveTimer = new KeepAliveTimer(deviceIpAddress, _configuration.KeepAliveTimer, TimerExpiredCallback);
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

            //todo _redisDbRepository.Publish(Definitions.RemoveRLMDeviceRLR, deviceIpAddress);
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
            KeepAliveTimer keepAliveTimer = new KeepAliveTimer(deviceIpAddress, _configuration.ImageCountDownTimer, ImageCounterTimerExpiredCallback);
            _rlmImageCountdown.TryAdd(deviceIpAddress, keepAliveTimer);
        }

        public void ImageTimerDelete(string deviceIpAddress)
        {
            KeepAliveTimer keepAliveTimer;
            _rlmImageCountdown.TryRemove(deviceIpAddress, out keepAliveTimer);
        }
    }    
}
