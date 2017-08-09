/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * SessionCommunication.cs: Business Logic for Session
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/
using System;
using Abiomed.Models;
using Abiomed.Repository;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace Abiomed.Business
{
    public class SessionCommunication : ISessionCommunication
    {
        private IRedisDbRepository<RLMDevice> _redisDbRepository;
        private ILogManager _logManager;
        private Abiomed.Models.Configuration _configuration;
        private IKeepAliveManager _keepAliveManager;
        private RLMDeviceList _rlmDeviceList;

        public SessionCommunication(IRedisDbRepository<RLMDevice> redisDbRepository, ILogManager logManager, Abiomed.Models.Configuration configuration, IKeepAliveManager keepAliveManager, RLMDeviceList rlmDeviceList)
        {            
            _redisDbRepository = redisDbRepository;
            _logManager = logManager;
            _configuration = configuration;
            _keepAliveManager = keepAliveManager;
            _rlmDeviceList = rlmDeviceList;
        }

        #region Receiving

        public byte[] SessionRequest(string deviceIpAddress, byte[] message, out RLMStatus status)
        {
            byte[] returnMessage = new byte[0];
            string deviceSerialNumber = string.Empty;
            try
            {
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };
                SessionRequest sessionRequest = new SessionRequest();

                // the Skip, Take & Reverse is used to reverse BigEndian Data...
                sessionRequest.MsgSeq = BitConverter.ToUInt16(message.Skip(4).Take(2).Reverse().ToArray(), 0);
                sessionRequest.IfaceVer = BitConverter.ToUInt16(message.Skip(6).Take(2).Reverse().ToArray(), 0);
                sessionRequest.SerialNo = Encoding.ASCII.GetString(message.Skip(8).Take(7).ToArray());
                sessionRequest.Bearer = (Definitions.Bearer)BitConverter.ToUInt16(message.Skip(15).Take(2).Reverse().ToArray(), 0);
                sessionRequest.Text = Encoding.ASCII.GetString(message.Skip(17).Take(message.Length - 18).ToArray());

                RLMDevice rlmDevice = new RLMDevice()
                {
                    DeviceIpAddress = deviceIpAddress,
                    ConnectionTime = DateTime.UtcNow,
                    Bearer = sessionRequest.Bearer,
                    IfaceVer = sessionRequest.IfaceVer,
                    SerialNo = sessionRequest.SerialNo,
                    ClientSequence = sessionRequest.MsgSeq,
                };

                deviceSerialNumber = rlmDevice.SerialNo;

                // Check if already online, 
                bool deviceOnline = _redisDbRepository.StringKeyExist(rlmDevice.SerialNo);

                // If device online, update device
                string addedOrUpdatedDevice = deviceOnline ? Definitions.UpdateRLMDevice : Definitions.AddRLMDevice;
                
                // Add/Update set and publish message
                _redisDbRepository.StringSet(rlmDevice.SerialNo, rlmDevice);
                _redisDbRepository.AddToSet(Definitions.RLMDeviceSet, rlmDevice.SerialNo);
                _redisDbRepository.Publish(addedOrUpdatedDevice, rlmDevice.SerialNo);

                // Remove old entry
                if (deviceOnline)
                {
                    // Find old entry based upon serial number and delete all instances
                    var oldRLMDevice = _rlmDeviceList.RLMDevices.FirstOrDefault(x => x.Value.SerialNo == rlmDevice.SerialNo);

                    if (oldRLMDevice.Key != null)
                    {
                        var oldDeviceIpAddress = oldRLMDevice.Key;
                        RLMDevice deleteRLMDevice;
                        _rlmDeviceList.RLMDevices.TryRemove(oldDeviceIpAddress, out deleteRLMDevice);
                        _keepAliveManager.Remove(oldDeviceIpAddress);
                        _keepAliveManager.ImageTimerDelete(oldDeviceIpAddress);
                    }
                }

                _rlmDeviceList.RLMDevices.TryAdd(deviceIpAddress, rlmDevice);
                _keepAliveManager.Add(deviceIpAddress);
                               
                // Check if we will video stream or screen grab
                byte[] streamIndicator = new byte[0];

                var sessionMessage = General.GenerateRequest(Definitions.SessionConfirm, rlmDevice);

                streamIndicator = General.GenerateRequest(Definitions.ScreenCaptureIndicator, rlmDevice);
                rlmDevice.FileTransferType = Definitions.RLMFileTransfer.ScreenCapture0;
                              
                if (rlmDevice.Bearer != Definitions.Bearer.LTE)
                {
                    
                    List<byte> secureStream = Definitions.StreamVideoControlIndicationRTMP;
                    if (_configuration.Security)
                    {
                        secureStream = Definitions.StreamVideoControlIndicationRTMPS;
                    }

                    // Remove
                    //secureStream = Definitions.StreamVideoControlIndication;

                    var videoControl = General.VideoControlGeneration(true, rlmDevice.SerialNo, secureStream);
                    streamIndicator = General.GenerateRequest(videoControl, rlmDevice);
                    rlmDevice.Streaming = true;
                }

                // Append to current Byte[]
                returnMessage = new byte[sessionMessage.Length + streamIndicator.Length];
                sessionMessage.CopyTo(returnMessage, 0);
                streamIndicator.CopyTo(returnMessage, sessionMessage.Length);

                string traceMessage = string.Format(@"Session Request Session {0}", rlmDevice.SerialNo);
                _logManager.Log(deviceIpAddress, rlmDevice.SerialNo, sessionRequest, Definitions.LogMessageType.SessionRequest, Definitions.LogType.Information, traceMessage);

            }
            catch (Exception e)
            {
                string traceMessage  = string.Format(@"Session Request Failure {0} Exception {1}", deviceIpAddress, e.ToString());
                _logManager.Log(deviceIpAddress, deviceSerialNumber, e, Definitions.LogMessageType.SessionRequest, Definitions.LogType.Information, traceMessage);
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Failure };
            }
            return returnMessage;
        }

        public byte[] BearerRequest(string deviceIpAddress, byte[] message, out RLMStatus status)
        {
            byte[] returnMessage = new byte[0];
            string deviceSerialNumber = string.Empty;

            try
            {
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };
                BearerRequest bearerRequest = new BearerRequest();
                bearerRequest.SerialNo =  Encoding.ASCII.GetString(message.Skip(6).Take(7).ToArray());
                bearerRequest.Bearer = (Definitions.Bearer)BitConverter.ToUInt16(message.Skip(13).Take(2).Reverse().ToArray(), 0);

                RLMDevice rlmDevice;
                _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);
                returnMessage = General.GenerateRequest(Definitions.BearerConfirm, rlmDevice);
                deviceSerialNumber = rlmDevice.SerialNo;

                string traceMessage = string.Format(@"Bearer Request {0}", rlmDevice.SerialNo);
                _logManager.Log(deviceIpAddress, rlmDevice.SerialNo, bearerRequest, Definitions.LogMessageType.BearerRequest, Definitions.LogType.Information, traceMessage);                
            }
            catch (Exception e)
            {
                string traceMessage = string.Format(@"Bearer Request Failure {0} Exception {1}", deviceIpAddress, e.ToString());
                _logManager.Log(deviceIpAddress, deviceSerialNumber, e, Definitions.LogMessageType.BearerRequest, Definitions.LogType.Exception, traceMessage);

                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Failure };
            }

            return returnMessage;
        }

        public byte[] SessionCloseResponse(string deviceIpAddress, byte[] message, out RLMStatus status)
        {
            byte[] returnMessage = new byte[0];
            string deviceSerialNumber = string.Empty;
            try
            {
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };
                SessionCloseResponse sessionCloseResponse = new SessionCloseResponse();
                sessionCloseResponse.Status = (Definitions.Status)BitConverter.ToUInt16(message.Skip(6).Take(2).Reverse().ToArray(), 0);

                RLMDevice rlmDevice;
                _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);
                deviceSerialNumber = rlmDevice.SerialNo;

                // The status will indicate rather to keep the session going. For this, we should not do any error checking.

                string traceMessage = string.Format(@"Session Close Response {0}", rlmDevice.SerialNo);
                _logManager.Log(deviceIpAddress, rlmDevice.SerialNo, sessionCloseResponse, Definitions.LogMessageType.SessionCloseResponse, Definitions.LogType.Information, traceMessage);
            }
            catch (Exception e)
            {
                string traceMessage = string.Format(@"Session Close Response Failure {0} Exception {1}", deviceIpAddress, e.ToString());
                _logManager.Log(deviceIpAddress, deviceSerialNumber, e, Definitions.LogMessageType.SessionCloseResponse, Definitions.LogType.Exception, traceMessage);
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Failure };
            }

            return returnMessage;
        }

        public byte[] CloseBearerRequest(string deviceIpAddress, byte[] message, out RLMStatus status)
        {
            byte[] returnMessage = new byte[0];
            string deviceSerialNumber = string.Empty;
            try
            {
                // temp
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };
                /*
                CloseBearerRequest closeBearerRequest = new CloseBearerRequest();
                closeBearerRequest.BearerStatistics.Bytes = BitConverter.ToUInt64(message.Skip(6).Take(8).Reverse().ToArray(), 0);
                closeBearerRequest.BearerStatistics.Frames = BitConverter.ToInt32(message.Skip(14).Take(4).Reverse().ToArray(), 0);
                closeBearerRequest.BearerStatistics.Seq = BitConverter.ToInt32(message.Skip(18).Take(2).Reverse().ToArray(), 0);
                closeBearerRequest.BearerStatistics.Count = BitConverter.ToInt32(message.Skip(20).Take(2).Reverse().ToArray(), 0);

                RLMDevice rlmDevice;
                _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);
                returnMessage = General.GenerateRequest(Definitions.CloseBearerConfirm, rlmDevice);

                deviceSerialNumber = rlmDevice.SerialNo;
                string traceMessage = string.Format(@"Close Bearer Request {0}", rlmDevice.SerialNo);
                _logManager.Log(deviceIpAddress, rlmDevice.SerialNo, closeBearerRequest, Definitions.LogMessageType.CloseBearerRequest, Definitions.LogType.Information);
                */
            }
            catch (Exception e)
            {
                string traceMessage = string.Format(@"Close Bearer Request Failure {0} Exception {1}", deviceIpAddress, e.ToString());
                _logManager.Log(deviceIpAddress, deviceSerialNumber, e, Definitions.LogMessageType.CloseBearerRequest, Definitions.LogType.Exception, traceMessage);

                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Failure };
            }

            return returnMessage;
        }

        public byte[] KeepAliveRequest(string deviceIpAddress, byte[] message, out RLMStatus status)
        {
            byte[] returnMessage = new byte[0];

            _keepAliveManager.Ping(deviceIpAddress);

            RLMDevice rlmDevice;
            _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);

            returnMessage = General.GenerateRequest(Definitions.KeepAliveIndication, rlmDevice);
            
            status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };
            return returnMessage;
        }

        public byte[] SessionCloseRequest(string deviceIpAddress, byte[] message, out RLMStatus status)
        {
            byte[] returnMessage = new byte[0];
            string deviceSerialNumber = string.Empty;
            try
            {
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };
                SessionCloseRequest sessionCloseRequest = new SessionCloseRequest();
                sessionCloseRequest.Bearer = (Definitions.Bearer)BitConverter.ToUInt16(message.Skip(6).Take(2).Reverse().ToArray(), 0);
                sessionCloseRequest.Status = (Definitions.Status)BitConverter.ToUInt16(message.Skip(8).Take(2).Reverse().ToArray(), 0);

                // No need to check status as RLM is going to shutdown either upon Session Close Confirm or destory it's own TCP session.

                RLMDevice rlmDevice;
                _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);
                deviceSerialNumber = rlmDevice.SerialNo;

                returnMessage = General.GenerateRequest(Definitions.SessionCloseConfirm, rlmDevice);
                string traceMessage = string.Format(@"Session Close Request {0}", rlmDevice.SerialNo);
                _logManager.Log(deviceIpAddress, rlmDevice.SerialNo, sessionCloseRequest, Definitions.LogMessageType.SessionCloseRequest, Definitions.LogType.Information, traceMessage);                
            }
            catch (Exception e)
            {
                string traceMessage = string.Format(@"Session Close Request Failure {0} Exception {1}", deviceIpAddress, e.ToString());
                _logManager.Log(deviceIpAddress, deviceSerialNumber, e, Definitions.LogMessageType.SessionCloseRequest, Definitions.LogType.Exception, traceMessage);

                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Failure };
            }

            return returnMessage;
        }
        #endregion

        #region Sending
        public byte[] BearerChangeIndication(string deviceIpAddress, Definitions.Bearer bearer)
        {
            byte[] returnMessage = new byte[0];
            string deviceSerialNumber = string.Empty;
            try
            {              
                RLMDevice rlmDevice;
                _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);
                deviceSerialNumber = rlmDevice.SerialNo;

                returnMessage = General.GenerateRequest(Definitions.BearerChangeIndication, rlmDevice);
                 
                // Replace with correct bearer
                returnMessage[7] = Convert.ToByte(bearer);

                string traceMessage = string.Format(@"Updated bearer to {0}, RLM Serial: {1}", bearer, rlmDevice.SerialNo);
                _logManager.Log(deviceIpAddress, rlmDevice.SerialNo, string.Format("Updated bearer to {0}", bearer), Definitions.LogMessageType.BearerChangeIndication, Definitions.LogType.Information, traceMessage);
            }
            catch (Exception e)
            {
                string traceMessage = string.Format(@"Bearer Change Indication Failure {0} Exception {1}", deviceIpAddress, e.ToString());
                _logManager.Log(deviceIpAddress, deviceSerialNumber, e, Definitions.LogMessageType.SessionCloseRequest, Definitions.LogType.Exception, traceMessage);
            }

            return returnMessage;
        }

        public byte[] KeepAliveIndication(string deviceIpAddress)
        {
            byte[] returnMessage = new byte[0];

            RLMDevice rlmDevice;
            _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);
            returnMessage = General.GenerateRequest(Definitions.KeepAliveIndication, rlmDevice);

            _logManager.TraceIt(Definitions.LogType.Information, string.Format(@"Keep Alive Indication {0}", rlmDevice.SerialNo));
           
            return returnMessage;
        }

        public byte[] SessionCloseIndication(string deviceIpAddress)
        {
            byte[] returnMessage = new byte[0];

            RLMDevice rlmDevice;
            _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);
            if (rlmDevice != null)
            {
                returnMessage = General.GenerateRequest(Definitions.CloseSessionIndication, rlmDevice);
                _logManager.TraceIt(Definitions.LogType.Information, string.Format(@"Close Session Indication {0}", rlmDevice.SerialNo));
            }
            else
            {
                _logManager.TraceIt(Definitions.LogType.Information, string.Format(@"Close Session Indication - device does not exist {0}", deviceIpAddress));
            }
            return returnMessage;
        }
        #endregion                    
    }
}
