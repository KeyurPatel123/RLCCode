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
using System.Diagnostics;
using System.Collections.Generic;

namespace Abiomed.Business
{
    public class DigitiserCommunication : IDigitiserCommunication
    {
        private IKeepAliveManager _keepAliveManager;
        private ILogManager _logManager;
        private RLMDeviceList _rlmDeviceList;
        private Configuration _configuration;

        public DigitiserCommunication(IKeepAliveManager keepAliveManager, ILogManager logManager, RLMDeviceList rlmDeviceList, Configuration configuration)
        {            
            _keepAliveManager = keepAliveManager;
            _logManager = logManager;
            _rlmDeviceList = rlmDeviceList;
            _configuration = configuration;
        }

        #region Receiving
        public byte[] StreamingVideoControlResponse(string deviceIpAddress, byte[] message, out RLMStatus status)
        {
            byte[] returnMessage = new byte[0];
            try
            {
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };
                StreamingVideoControlResponse streamingVideoControlResponse = new StreamingVideoControlResponse();
                streamingVideoControlResponse.Status = BitConverter.ToUInt16(message.Skip(6).Take(2).Reverse().ToArray(), 0);
                streamingVideoControlResponse.UserRef = BitConverter.ToUInt16(message.Skip(8).Take(2).Reverse().ToArray(), 0);

                RLMDevice rlmDevice;
                _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);

                // Error checking
                if (streamingVideoControlResponse.Status != Definitions.SuccessStats || streamingVideoControlResponse.UserRef != Definitions.UserRef)
                {
                    status.Status = RLMStatus.StatusEnum.Failure;
                    Trace.TraceInformation(@"Streaming Video Control Response Failure {0}", rlmDevice.SerialNo);
                }
                else
                {
                    Trace.TraceInformation(@"Streaming Video Control Response {0}", rlmDevice.SerialNo);
                }
                _logManager.Create(deviceIpAddress, rlmDevice.SerialNo, streamingVideoControlResponse, Definitions.LogMessageType.StreamingVideoControlResponse);

            }
            catch (Exception e)
            {
                Trace.TraceInformation(@"Streaming Video Control Response Failure {0} Exception {1}", deviceIpAddress, e.ToString());
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Failure };
            }

            return returnMessage;
        }

        public byte[] BufferStatusRequest(string deviceIpAddress, byte[] message, out RLMStatus status)
         {        
            byte[] returnMessage = new byte[0];

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
                    status.Status = RLMStatus.StatusEnum.Failure;
                }

                RLMDevice rlmDevice;
                _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);
                _keepAliveManager.Ping(deviceIpAddress);

                Trace.TraceInformation(@"Buffer Status Request {0}", deviceIpAddress);
                _logManager.Create(deviceIpAddress, rlmDevice.SerialNo, bufferStatusRequest, Definitions.LogMessageType.BufferStatusRequest);
            }
            catch (Exception e)
            {
                Trace.TraceInformation(@"Buffer Status Request Failure {0} Exception {1}", deviceIpAddress, e.ToString());
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Failure };

            }
            return returnMessage;
        }

        public byte[] ScreenCaptureResponse(string deviceIpAddress, byte[] message, out RLMStatus status)
        {
            byte[] returnMessage = new byte[0];
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

                // todo check for what type
                // Create Open File Ind for screen 0
                returnMessage = General.GenerateRequest(Definitions.OpenScreenFileIndication, rlmDevice);

                Trace.TraceInformation(@"Screen Capture Response {0}", deviceIpAddress);
                _logManager.Create(deviceIpAddress, rlmDevice.SerialNo, screenCaptureResponse, Definitions.LogMessageType.ScreenCaptureResponse);

            }
            catch (Exception e)
            {
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Failure };
                Trace.TraceError(@"Screen Capture Response Failure {0} Exception {1}", deviceIpAddress, e.ToString());
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

            Trace.TraceInformation(@"Sending Screen Capture {0}", rlmDevice.SerialNo);

            return returnMessage;
        }

        public byte[] VideoStop(string deviceIpAddress)
        {
            RLMDevice rlmDevice;
            _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);

            rlmDevice.Streaming = false;

            byte[] returnMessage = General.GenerateRequest(Definitions.VideoStopIndicator, rlmDevice);

            Trace.TraceInformation(@"Sending Video Stop {0}", rlmDevice.SerialNo);

            return returnMessage;
        }

        public byte[] ImageStop(string deviceIpAddress)
        {
            RLMDevice rlmDevice;
            _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);

            // Shut off Request Image Timer
            _keepAliveManager.ImageTimerDelete(deviceIpAddress);

            Trace.TraceInformation(@"Stop Screen Capture {0}", rlmDevice.SerialNo);
            return new byte[0];
        }
        #endregion
    }
}
