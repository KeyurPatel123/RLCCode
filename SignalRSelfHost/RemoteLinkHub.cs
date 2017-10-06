using Abiomed.Business;
using Abiomed.Models;
using Abiomed.Repository;
using Autofac;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abiomed.SignalRSelfHost
{
    public class RemoteLinkHub : Hub
    {
        private readonly ILifetimeScope _hubLifetimeScope;
        private IRedisDbRepository<RLMDevice> _redisDbRepository;

        public RemoteLinkHub(ILifetimeScope lifetimeScope)
        {
            _hubLifetimeScope = lifetimeScope.BeginLifetimeScope();

            // Resolve dependencies from the hub lifetime scope.
            //_logManager = _hubLifetimeScope.Resolve<ILogManager>();
            _redisDbRepository = _hubLifetimeScope.Resolve<IRedisDbRepository<RLMDevice>>();
            RegisterEvents();
        }

        #region Server calls to Client
        public void AddedRemoteLink(DeviceStatus RLMDevice)
        {            
            //// Message that will be broadcasted to client
            Clients.All.AddedRemoteLink(RLMDevice);
        }

        public void UpdatedRemoteLink(DeviceStatus RLMDevice)
        {            
            // Message that will be broadcasted to client
            Clients.All.UpdatedRemoteLink(RLMDevice);
        }

        public void DeletedRemoteLink(DeviceStatus RLMDevice)
        {        
            // Message that will be broadcasted to client
            Clients.All.DeletedRemoteLink(RLMDevice);
        }
        public void BearerCommunicationsStatus()
        {
            // Message that will be broadcasted to client
            //Clients.All.BearerCommunicationsStatus();
        }

        public void BearerSettings(string deviceSerialNo, List<BearerAuthenticationReadResponse> bearerInfoList)
        {
            string output = JsonConvert.SerializeObject(bearerInfoList);

            Clients.All.BearerSettings(deviceSerialNo, bearerInfoList);
        }
        #endregion

        #region Calls From Client
        public void AddToGroup()
        {
            //Clients.Groups.AddToGroup(Context.ConnectionId);
        }
        #endregion

        #region REDIS Events
        private void RegisterEvents()
        {
            #region RLM AUD Events
            _redisDbRepository.Subscribe(Definitions.AddRLMDevice, (channel, message) =>
            {                
                RLMDevice device = _redisDbRepository.RLMModelGet(message);                
                var deviceStatus = ConvertRLMDevice(device);
                Console.WriteLine("Added device");
                AddedRemoteLink(deviceStatus);
            });

            _redisDbRepository.Subscribe(Definitions.UpdateRLMDevice, (channel, message) =>
            {
                RLMDevice device = _redisDbRepository.RLMModelGet(message);
                var deviceStatus = ConvertRLMDevice(device);
                Console.WriteLine("Updated device");
                UpdatedRemoteLink(deviceStatus);
            });

            _redisDbRepository.Subscribe(Definitions.DeleteRLMDevice, (channel, message) =>
            {
                RLMDevice device = _redisDbRepository.RLMModelGet(message);
                var deviceStatus = ConvertRLMDevice(device);
                Console.WriteLine("Deleted device");
                DeletedRemoteLink(deviceStatus);
            });
            #endregion

            _redisDbRepository.Subscribe(Definitions.BearerInfoRLMDevice, (channel, message) =>
            {
                Console.WriteLine("Bearer Info");
                RLMDevice device = _redisDbRepository.RLMModelGet(message);
                BearerSettings(device.SerialNo, device.BearerAuthInformationList);
            });
        }
        #endregion

        #region Helper 
        public DeviceStatus ConvertRLMDevice(RLMDevice device)
        {
            DeviceStatus ds = new DeviceStatus { Bearer = device.Bearer.ToString(), ConnectionTime = device.ConnectionTime, SerialNumber = device.SerialNo, DeviceIpAddress = device.DeviceIpAddress };
            return ds;
        }        
        #endregion
    }
}
