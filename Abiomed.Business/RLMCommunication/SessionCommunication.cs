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
        private Configuration _configuration;
        private IKeepAliveManager _keepAliveManager;
        private RLMDeviceList _rlmDeviceList;

        public SessionCommunication(IRedisDbRepository<RLMDevice> redisDbRepository, ILogManager logManager, Configuration configuration, IKeepAliveManager keepAliveManager, RLMDeviceList rlmDeviceList)
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

                    var videoControl = General.VideoControlGeneration(true, rlmDevice.SerialNo, secureStream);
                    streamIndicator = General.GenerateRequest(videoControl, rlmDevice);
                    rlmDevice.Streaming = true;
                }

                // Append to current Byte[]
                returnMessage = new byte[sessionMessage.Length + streamIndicator.Length];
                sessionMessage.CopyTo(returnMessage, 0);
                streamIndicator.CopyTo(returnMessage, sessionMessage.Length);

                Trace.TraceInformation(@"Session Request Session {0}", rlmDevice.SerialNo);
                _logManager.Create(deviceIpAddress, rlmDevice.SerialNo, sessionRequest, Definitions.LogMessageType.SessionRequest);

            }
            catch (Exception e)
            {
                Trace.TraceError(@"Session Request Failure {0} Exception {1}", deviceIpAddress, e.ToString());
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Failure };
            }
            return returnMessage;
        }

        public byte[] BearerRequest(string deviceIpAddress, byte[] message, out RLMStatus status)
        {
            byte[] returnMessage = new byte[0];
            try
            {
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };
                BearerRequest bearerRequest = new BearerRequest();
                bearerRequest.SerialNo =  Encoding.ASCII.GetString(message.Skip(6).Take(7).ToArray());
                bearerRequest.Bearer = (Definitions.Bearer)BitConverter.ToUInt16(message.Skip(13).Take(2).Reverse().ToArray(), 0);

                RLMDevice rlmDevice;
                _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);
                returnMessage = General.GenerateRequest(Definitions.BearerConfirm, rlmDevice);

                Trace.TraceInformation(@"Bearer Request {0}", rlmDevice.SerialNo);
                _logManager.Create(deviceIpAddress, rlmDevice.SerialNo, bearerRequest, Definitions.LogMessageType.BearerRequest);                
            }
            catch (Exception e)
            {
                Trace.TraceInformation(@"Bearer Request Failure {0} Exception {1}", deviceIpAddress, e.ToString());
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Failure };
            }

            return returnMessage;
        }

        public byte[] BearerChangeResponse(string deviceIpAddress, byte[] message, out RLMStatus status)
        {
            byte[] returnMessage = new byte[0];
            try
            {
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };
                BearerChangeResponse bearerChangeResponse = new BearerChangeResponse();
                bearerChangeResponse.Status = (Definitions.Status)BitConverter.ToUInt16(message.Skip(6).Take(2).Reverse().ToArray(), 0);

                RLMDevice rlmDevice;
                _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);

                // Todo look at status and verify okay, look at REDIS for message queue!

                Trace.TraceInformation(@"Bearer Change Response {0}", rlmDevice.SerialNo);
                _logManager.Create(deviceIpAddress, rlmDevice.SerialNo, bearerChangeResponse, Definitions.LogMessageType.BearerChangeResponse);
            }
            catch (Exception e)
            {
                Trace.TraceError(@"Bearer Change Response Failure {0} Exception {1}", deviceIpAddress, e.ToString());
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Failure };
            }

            return returnMessage;
        }

        public byte[] CloseBearerRequest(string deviceIpAddress, byte[] message, out RLMStatus status)
        {
            byte[] returnMessage = new byte[0];
            try
            {
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };
                CloseBearerRequest closeBearerRequest = new CloseBearerRequest();
                closeBearerRequest.BearerStatistics.Bytes = BitConverter.ToUInt64(message.Skip(6).Take(8).Reverse().ToArray(), 0);
                closeBearerRequest.BearerStatistics.Frames = BitConverter.ToInt32(message.Skip(14).Take(4).Reverse().ToArray(), 0);
                closeBearerRequest.BearerStatistics.Seq = BitConverter.ToInt32(message.Skip(18).Take(2).Reverse().ToArray(), 0);
                closeBearerRequest.BearerStatistics.Count = BitConverter.ToInt32(message.Skip(20).Take(2).Reverse().ToArray(), 0);

                RLMDevice rlmDevice;
                _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);
                returnMessage = General.GenerateRequest(Definitions.CloseBearerConfirm, rlmDevice);

                Trace.TraceInformation(@"Close Bearer Request {0}", rlmDevice.SerialNo);
                _logManager.Create(deviceIpAddress, rlmDevice.SerialNo, closeBearerRequest, Definitions.LogMessageType.CloseBearerRequest);
            }
            catch (Exception e)
            {
                Trace.TraceError(@"Close Bearer Request Failure {0} Exception {1}", deviceIpAddress, e.ToString());
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Failure };
            }

            return returnMessage;
        }

        public byte[] KeepAliveRequest(string deviceIpAddress, byte[] message, out RLMStatus status)
        {
            _keepAliveManager.Ping(deviceIpAddress);

            RLMDevice rlmDevice;
            _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);
            Trace.TraceInformation(@"Keep Alive Request {0}", rlmDevice.SerialNo);

            status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };
            return new byte[0];
        }

        public byte[] CloseSessionRequest(string deviceIpAddress, byte[] message, out RLMStatus status)
        {
            byte[] returnMessage = new byte[0];
            try
            {
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };
                CloseSessionRequest closeSessionRequest = new CloseSessionRequest();

                #region Ethernet 
                closeSessionRequest.Ethernet.Bytes = BitConverter.ToUInt64(message.Skip(6).Take(8).Reverse().ToArray(), 0);                
                closeSessionRequest.Ethernet.Frames = BitConverter.ToInt32(message.Skip(14).Take(4).Reverse().ToArray(), 0);
                closeSessionRequest.Ethernet.Seq = BitConverter.ToInt32(message.Skip(18).Take(2).Reverse().ToArray(), 0);
                closeSessionRequest.Ethernet.Count = BitConverter.ToInt32(message.Skip(20).Take(2).Reverse().ToArray(), 0);
                #endregion

                #region Wifi 2.4 
                closeSessionRequest.Wifi24.Bytes = BitConverter.ToUInt64(message.Skip(22).Take(8).Reverse().ToArray(), 0);
                closeSessionRequest.Wifi24.Frames = BitConverter.ToInt32(message.Skip(30).Take(4).Reverse().ToArray(), 0);
                closeSessionRequest.Wifi24.Seq = BitConverter.ToInt32(message.Skip(34).Take(2).Reverse().ToArray(), 0);
                closeSessionRequest.Wifi24.Count = BitConverter.ToInt32(message.Skip(36).Take(2).Reverse().ToArray(), 0);
                #endregion

                #region Wifi 5
                closeSessionRequest.Wifi5.Bytes = BitConverter.ToUInt64(message.Skip(38).Take(8).Reverse().ToArray(), 0);
                closeSessionRequest.Wifi5.Frames = BitConverter.ToInt32(message.Skip(46).Take(4).Reverse().ToArray(), 0);
                closeSessionRequest.Wifi5.Seq = BitConverter.ToInt32(message.Skip(50).Take(2).Reverse().ToArray(), 0);
                closeSessionRequest.Wifi5.Count = BitConverter.ToInt32(message.Skip(52).Take(2).Reverse().ToArray(), 0);
                #endregion

                #region LTE 
                closeSessionRequest.LTE.Bytes = BitConverter.ToUInt64(message.Skip(54).Take(8).Reverse().ToArray(), 0);
                closeSessionRequest.LTE.Frames = BitConverter.ToInt32(message.Skip(62).Take(4).Reverse().ToArray(), 0);
                closeSessionRequest.LTE.Seq = BitConverter.ToInt32(message.Skip(66).Take(2).Reverse().ToArray(), 0);
                closeSessionRequest.LTE.Count = BitConverter.ToInt32(message.Skip(68).Take(2).Reverse().ToArray(), 0);
                #endregion

                RLMDevice rlmDevice;
                _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);

                Trace.TraceInformation(@"Close Session Request {0}", rlmDevice.SerialNo);
                _logManager.Create(deviceIpAddress, rlmDevice.SerialNo, closeSessionRequest, Definitions.LogMessageType.CloseSessionRequest);

                // todo close session afterwards????
            }
            catch (Exception e)
            {
                Trace.TraceInformation(@"Close Session Request Failure {0} Exception {1}", deviceIpAddress, e.ToString());
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Failure };
            }

            return returnMessage;
        }
        #endregion

        #region Sending
        public byte[] BearerChangeIndication(string deviceIpAddress, Definitions.Bearer bearer)
        {
            byte[] returnMessage = new byte[0];
            try
            {              
                RLMDevice rlmDevice;
                _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);

                returnMessage = General.GenerateRequest(Definitions.BearerChangeIndication, rlmDevice);

                // Replace with correct bearer
                returnMessage[Definitions.BearerChangeIndication.Length - 1] = Convert.ToByte(bearer);

                Trace.TraceInformation(@"Updated bearer to {0}, RLM Serial: {1}", bearer, rlmDevice.SerialNo);
                _logManager.Create(deviceIpAddress, rlmDevice.SerialNo, string.Format("Updated bearer to {0}", bearer), Definitions.LogMessageType.BearerChangeIndication);
            }
            catch (Exception e)
            {
                Trace.TraceError(@"Bearer Change Indication Failure {0} Exception {1}", deviceIpAddress, e.ToString());
            }

            return returnMessage;
        }

        public byte[] KeepAliveIndication(string deviceIpAddress)
        {
            byte[] returnMessage = new byte[0];

            RLMDevice rlmDevice;
            _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);
            returnMessage = General.GenerateRequest(Definitions.KeepAliveIndication, rlmDevice);

            Trace.TraceInformation(@"Keep Alive Indication {0}", rlmDevice.SerialNo);
            return returnMessage;
        }

        public byte[] CloseSessionIndication(string deviceIpAddress)
        {
            byte[] returnMessage = new byte[0];

            RLMDevice rlmDevice;
            _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);
            if (rlmDevice != null)
            {
                returnMessage = General.GenerateRequest(Definitions.CloseSessionIndication, rlmDevice);
                Trace.TraceInformation(@"Close Session Indication {0}", rlmDevice.SerialNo);
            }
            else
            {
                Trace.TraceInformation(@"Close Session Indication - device does not exist {0}", deviceIpAddress);
            }
            return returnMessage;
        }
        #endregion                    
    }
}
