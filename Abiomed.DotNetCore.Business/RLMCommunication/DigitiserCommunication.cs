/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * DigitiserCommunication.cs: Digitiser Communication
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/
using System;
using Abiomed.DotNetCore.Models;
using System.Linq;
using System.Collections.Generic;
using Abiomed.DotNetCore.Repository;
using Abiomed.DotNetCore.Configuration;
using Microsoft.Extensions.Logging;

namespace Abiomed.DotNetCore.Business
{
    public class DigitiserCommunication : IDigitiserCommunication
    {
        private IKeepAliveManager _keepAliveManager;
        private ILogger<IDigitiserCommunication> _logger;
        private RLMDeviceList _rlmDeviceList;
        private IRedisDbRepository<RLMDevice> _redisDbRepository;
        private IConfigurationCache _configurationCache;
        private bool _isSecurity;

        public DigitiserCommunication(IKeepAliveManager keepAliveManager, ILogger<IDigitiserCommunication> logger, RLMDeviceList rlmDeviceList, IRedisDbRepository<RLMDevice> redisDbRepository, IConfigurationCache configurationCache)
        {            
            _keepAliveManager = keepAliveManager;
            _logger = logger;
            _rlmDeviceList = rlmDeviceList;
            _configurationCache = configurationCache;
            _redisDbRepository = redisDbRepository;

            _isSecurity = _configurationCache.GetBooleanConfigurationItem("connectionmanager", "security");
        }

        #region Receiving
        public byte[] StreamingVideoControlResponse(string deviceIpAddress, byte[] message, out RLMStatus status)
        {
            byte[] returnMessage = new byte[0];
            string deviceSerialNumber = string.Empty;
            try
            {
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };
                StreamingVideoControlResponse streamingVideoControlResponse = new StreamingVideoControlResponse();
                streamingVideoControlResponse.Status = BitConverter.ToUInt16(message.Skip(6).Take(2).Reverse().ToArray(), 0);
                streamingVideoControlResponse.UserRef = BitConverter.ToUInt16(message.Skip(8).Take(2).Reverse().ToArray(), 0);

                RLMDevice rlmDevice;
                _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);
                deviceSerialNumber = rlmDevice.SerialNo;

                string traceMessage = string.Empty;
                string failureText = string.Empty;

                // Error checking
                if (streamingVideoControlResponse.Status != Definitions.SuccessStats || streamingVideoControlResponse.UserRef != Definitions.UserRef)
                {
                    status.Status = RLMStatus.StatusEnum.Failure;
                    _logger.LogInformation("Streaming Video Control Response Error {0}", rlmDevice.SerialNo);
                }
                else
                {
                    _logger.LogInformation("Streaming Video Control Response {0}", rlmDevice.SerialNo);
                }

            }
            catch (Exception e)
            {
                _logger.LogError("Streaming Video Control Response Failure {0} Exception {1}", deviceIpAddress, e.ToString());                
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Failure };
            }

            return returnMessage;
        }

        public byte[] BufferStatusRequest(string deviceIpAddress, byte[] message, out RLMStatus status)
         {        
            byte[] returnMessage = new byte[0];
            string deviceSerialNumber = string.Empty;

            try
            {
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };

                BufferStatusRequest bufferStatusRequest = new BufferStatusRequest();

                bufferStatusRequest.Status = BitConverter.ToUInt16(message.Skip(6).Take(2).Reverse().ToArray(), 0);
                bufferStatusRequest.Bytes = BitConverter.ToUInt16(message.Skip(8).Take(4).Reverse().ToArray(), 0);
                bufferStatusRequest.Dropped = BitConverter.ToUInt16(message.Skip(12).Take(4).Reverse().ToArray(), 0);
                bufferStatusRequest.Sent = BitConverter.ToUInt16(message.Skip(16).Take(4).Reverse().ToArray(), 0);             

                RLMDevice rlmDevice;
                _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);
                _keepAliveManager.Ping(deviceIpAddress);
                deviceSerialNumber = rlmDevice.SerialNo;

                _logger.LogDebug("Buffer Status Request {0}", deviceIpAddress);
            }
            catch (Exception e)
            {                
                _logger.LogError("Buffer Status Request Failure {0}Exception { 1}", deviceIpAddress, e.ToString());

                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Failure };
            }
            return returnMessage;
        }

        public byte[] ScreenCaptureResponse(string deviceIpAddress, byte[] message, out RLMStatus status)
        {
            byte[] returnMessage = new byte[0];
            string deviceSerialNumber = string.Empty;
            try
            {
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };

                ScreenCaptureResponse screenCaptureResponse = new ScreenCaptureResponse();
                screenCaptureResponse.Status = BitConverter.ToUInt16(message.Skip(6).Take(2).Reverse().ToArray(), 0);
                screenCaptureResponse.UserRef = BitConverter.ToUInt16(message.Skip(8).Take(2).Reverse().ToArray(), 0);

                // todo; Check the Status in more detail
                if (screenCaptureResponse.Status != Definitions.SuccessStats || screenCaptureResponse.UserRef != Definitions.UserRef)
                {
                    status.Status = RLMStatus.StatusEnum.Failure;
                }

                RLMDevice rlmDevice;
                _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);
                _keepAliveManager.Ping(deviceIpAddress);
                deviceSerialNumber = rlmDevice.SerialNo;

                // Create Open File Ind for screen 0
                returnMessage = General.GenerateRequest(Definitions.OpenScreenFileIndication, rlmDevice);
                _logger.LogInformation("Screen Capture Response {0}", deviceIpAddress);
            }
            catch (Exception e)
            {
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Failure };
                _logger.LogError("Screen Capture Response Failure {0} Exception {1}", deviceIpAddress, e.ToString());
            }
            return returnMessage;
        }
        #endregion

        #region Sending
        public byte[] StreamingVideoControlIndication(string deviceIpAddress)
        {
            RLMDevice rlmDevice;
            _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);
            List<byte> secureStream = Definitions.StreamVideoControlIndicationRTMP;            
            if (_isSecurity)
            {
                secureStream = Definitions.StreamVideoControlIndicationRTMPS;
            }

            // Temp
            secureStream = Definitions.StreamVideoControlIndication;

            // Remove Image Capture Timer
            _keepAliveManager.ImageTimerDelete(deviceIpAddress);

            byte[] videoControl = new byte[0];

            // Check if already streaming, if so, do not send message.
            if (!rlmDevice.Streaming)
            {
                videoControl = General.VideoControlGeneration(true, rlmDevice.SerialNo, secureStream);
                videoControl = General.GenerateRequest(videoControl, rlmDevice);
                rlmDevice.Streaming = true;
            }

            return videoControl;
        }

        public byte[] ScreenCaptureIndication(string deviceIpAddress)
        {
            RLMDevice rlmDevice;
            _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);

            // Shut off Request Image Timer
            _keepAliveManager.ImageTimerDelete(deviceIpAddress);

            byte[] returnMessage = General.GenerateRequest(Definitions.ScreenCaptureIndicator, rlmDevice);            
            rlmDevice.FileTransferType = Definitions.RLMFileTransfer.ScreenCapture0;

            _logger.LogInformation("Sending Screen Capture {0}", rlmDevice.SerialNo);

            return returnMessage;
        }

        public byte[] VideoStop(string deviceIpAddress)
        {
            RLMDevice rlmDevice;
            _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);
            rlmDevice.Streaming = false;

            byte[] returnMessage = General.GenerateRequest(Definitions.VideoStopIndicator, rlmDevice);
        
            _logger.LogInformation("Sending Video Stop {0}", rlmDevice.SerialNo);
            return returnMessage;
        }

        public byte[] ImageStop(string deviceIpAddress)
        {
            RLMDevice rlmDevice;
            _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);

            // Shut off Request Image Timer
            _keepAliveManager.ImageTimerDelete(deviceIpAddress);

            _logger.LogInformation("Stop Screen Capture {0}", rlmDevice.SerialNo);
            return new byte[0];
        }
        #endregion
    }
}
