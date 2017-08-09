/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * DigitiserCommunication.cs: Digitiser Communication
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/
using System;
using Abiomed.Models;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Abiomed.Repository;

namespace Abiomed.Business
{
    public class DigitiserCommunication : IDigitiserCommunication
    {
        private IKeepAliveManager _keepAliveManager;
        private ILogManager _logManager;
        private RLMDeviceList _rlmDeviceList;
        private Abiomed.Models.Configuration _configuration;
        private IRedisDbRepository<RLMDevice> _redisDbRepository;

        public DigitiserCommunication(IKeepAliveManager keepAliveManager, ILogManager logManager, RLMDeviceList rlmDeviceList, Abiomed.Models.Configuration configuration, IRedisDbRepository<RLMDevice> redisDbRepository)
        {            
            _keepAliveManager = keepAliveManager;
            _logManager = logManager;
            _rlmDeviceList = rlmDeviceList;
            _configuration = configuration;
            _redisDbRepository = redisDbRepository;
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
                Definitions.LogType traceLogType = Definitions.LogType.NoTrace;
                // Error checking
                if (streamingVideoControlResponse.Status != Definitions.SuccessStats || streamingVideoControlResponse.UserRef != Definitions.UserRef)
                {
                    status.Status = RLMStatus.StatusEnum.Failure;
                    failureText = @"Failure ";
                    traceLogType = Definitions.LogType.Error;
                }
                else
                {
                    traceLogType = Definitions.LogType.Information;
                }

                traceMessage = string.Format(@"Streaming Video Control Response {0}{1}", failureText, rlmDevice.SerialNo);
                _logManager.Log(deviceIpAddress, deviceSerialNumber, streamingVideoControlResponse, Definitions.LogMessageType.StreamingVideoControlResponse, traceLogType, traceMessage);
            }
            catch (Exception e)
            {
                string errorMessage = string.Format(@"Streaming Video Control Response Failure {0} Exception {1}", deviceIpAddress, e.ToString());
                _logManager.Log(deviceIpAddress, deviceSerialNumber, e, Definitions.LogMessageType.StreamingVideoControlResponse, Definitions.LogType.Exception, errorMessage);

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

                // Error checking
                if (bufferStatusRequest.Status != Definitions.SuccessStats)
                {
                    // Temo???
                    //status.Status = RLMStatus.StatusEnum.Failure;
                    returnMessage = VideoStop(deviceIpAddress);

                    // Wait 10 Seconds and send video start
                    TimerCallback tmCallback = KickStartVideo;
                    Timer timer = new Timer(tmCallback, deviceIpAddress, 10000, -1);
                }

                RLMDevice rlmDevice;
                _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);
                _keepAliveManager.Ping(deviceIpAddress);
                deviceSerialNumber = rlmDevice.SerialNo;

                // Put back with Config Verbose flag
                //_logManager.Log(deviceIpAddress, rlmDevice.SerialNo, bufferStatusRequest, Definitions.LogMessageType.BufferStatusRequest, Definitions.LogType.Information, string.Format(@"Buffer Status Request {0}", deviceIpAddress));
            }
            catch (Exception e)
            {
                string errorMessage = string.Format(@"Buffer Status Request Failure {0} Exception {1}", deviceIpAddress, e.ToString());
                _logManager.Log(deviceIpAddress, deviceSerialNumber, e, Definitions.LogMessageType.BufferStatusRequest, Definitions.LogType.Exception, errorMessage);

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

                // todo check for what type
                // Create Open File Ind for screen 0
                returnMessage = General.GenerateRequest(Definitions.OpenScreenFileIndication, rlmDevice);

                string traceMessage = string.Format(@"Screen Capture Response {0}", deviceIpAddress);
                _logManager.Log(deviceIpAddress, rlmDevice.SerialNo, screenCaptureResponse, Definitions.LogMessageType.ScreenCaptureResponse, Definitions.LogType.Information, traceMessage);
            }
            catch (Exception e)
            {
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Failure };

                string traceMessage = string.Format(@"Screen Capture Response Failure {0} Exception {1}", deviceIpAddress, e.ToString());
                _logManager.Log(deviceIpAddress, deviceSerialNumber, e, Definitions.LogMessageType.ScreenCaptureResponse, Definitions.LogType.Exception, traceMessage);
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
            if (_configuration.Security)
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

            _logManager.TraceIt(Definitions.LogType.Information, string.Format(@"Sending Screen Capture {0}", rlmDevice.SerialNo));

            return returnMessage;
        }

        public byte[] VideoStop(string deviceIpAddress)
        {
            RLMDevice rlmDevice;
            _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);
            rlmDevice.Streaming = false;

            byte[] returnMessage = General.GenerateRequest(Definitions.VideoStopIndicator, rlmDevice);
        
            _logManager.TraceIt(Definitions.LogType.Information, string.Format(@"Sending Video Stop {0}", rlmDevice.SerialNo));
            return returnMessage;
        }

        public byte[] ImageStop(string deviceIpAddress)
        {
            RLMDevice rlmDevice;
            _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);

            // Shut off Request Image Timer
            _keepAliveManager.ImageTimerDelete(deviceIpAddress);

            _logManager.TraceIt(Definitions.LogType.Information, string.Format(@"Stop Screen Capture {0}", rlmDevice.SerialNo));
            return new byte[0];
        }

        // Temp Function????
        private void KickStartVideo(object obj)
        {
            // convert obj to string
            string deviceIpAddress = (string)obj;

            _logManager.TraceIt(Definitions.LogType.Information, string.Format(@"Starting Video after 10 seconds {0}", deviceIpAddress));

            // Send message to start 
            _redisDbRepository.Publish(Definitions.StreamingVideoControlIndicationEvent, deviceIpAddress);
        }
        #endregion
    }
}
