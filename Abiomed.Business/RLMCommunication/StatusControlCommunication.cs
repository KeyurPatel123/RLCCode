/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * StatusControl.cs: Status Control Communication
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/
using System;
using Abiomed.Models;
using System.Diagnostics;
using System.Linq;
using Abiomed.Repository;
using System.Text;

namespace Abiomed.Business
{
    public class StatusControlCommunication : IStatusControlCommunication
    {
        private ILogManager _logManager;
        private RLMDeviceList _rlmDeviceList;
        private IRedisDbRepository<RLMDevice> _redisDbRepository;

        public StatusControlCommunication(ILogManager logManager, RLMDeviceList rlmDeviceList, IRedisDbRepository<RLMDevice> redisDbRepository)
        {
            _logManager = logManager;
            _rlmDeviceList = rlmDeviceList;
            _redisDbRepository = redisDbRepository;
    }

    #region Receiving
    public byte[] StatusResponse(string deviceIpAddress, byte[] message, out RLMStatus status)
        {
            byte[] returnMessage = new byte[0];
            try
            {
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };
                StatusResponse statusResponse = new StatusResponse();

                #region Ethernet 
                statusResponse.Ethernet.Result = BitConverter.ToUInt16(message.Skip(6).Take(2).Reverse().ToArray(), 0);
                statusResponse.Ethernet.Available = BitConverter.ToUInt16(message.Skip(8).Take(2).Reverse().ToArray(), 0);
                statusResponse.Ethernet.RSSI = BitConverter.ToUInt16(message.Skip(10).Take(2).Reverse().ToArray(), 0);
                statusResponse.Ethernet.Attempts = BitConverter.ToUInt16(message.Skip(12).Take(2).Reverse().ToArray(), 0);
                statusResponse.Ethernet.Failures = BitConverter.ToUInt16(message.Skip(14).Take(2).Reverse().ToArray(), 0);
                #endregion

                #region Wifi 2.4 
                statusResponse.Wifi24.Result = BitConverter.ToUInt16(message.Skip(16).Take(2).Reverse().ToArray(), 0);
                statusResponse.Wifi24.Available = BitConverter.ToUInt16(message.Skip(18).Take(2).Reverse().ToArray(), 0);
                statusResponse.Wifi24.RSSI = BitConverter.ToUInt16(message.Skip(20).Take(2).Reverse().ToArray(), 0);
                statusResponse.Wifi24.Attempts = BitConverter.ToUInt16(message.Skip(22).Take(2).Reverse().ToArray(), 0);
                statusResponse.Wifi24.Failures = BitConverter.ToUInt16(message.Skip(24).Take(2).Reverse().ToArray(), 0);
                #endregion

                #region Wifi 5
                statusResponse.Wifi5.Result = BitConverter.ToUInt16(message.Skip(26).Take(2).Reverse().ToArray(), 0);
                statusResponse.Wifi5.Available = BitConverter.ToUInt16(message.Skip(28).Take(2).Reverse().ToArray(), 0);
                statusResponse.Wifi5.RSSI = BitConverter.ToUInt16(message.Skip(30).Take(2).Reverse().ToArray(), 0);
                statusResponse.Wifi5.Attempts = BitConverter.ToUInt16(message.Skip(32).Take(2).Reverse().ToArray(), 0);
                statusResponse.Wifi5.Failures = BitConverter.ToUInt16(message.Skip(34).Take(2).Reverse().ToArray(), 0);
                #endregion

                #region LTE
                statusResponse.LTE.Result = BitConverter.ToUInt16(message.Skip(36).Take(2).Reverse().ToArray(), 0);
                statusResponse.LTE.Available = BitConverter.ToUInt16(message.Skip(38).Take(2).Reverse().ToArray(), 0);
                statusResponse.LTE.RSSI = BitConverter.ToUInt16(message.Skip(40).Take(2).Reverse().ToArray(), 0);
                statusResponse.LTE.Attempts = BitConverter.ToUInt16(message.Skip(42).Take(2).Reverse().ToArray(), 0);
                statusResponse.LTE.Failures = BitConverter.ToUInt16(message.Skip(44).Take(2).Reverse().ToArray(), 0);
                #endregion


                RLMDevice rlmDevice;
                _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);

                Trace.TraceInformation(@"Status Response {0}", rlmDevice.SerialNo);
                _logManager.Create(deviceIpAddress, rlmDevice.SerialNo, statusResponse, Definitions.LogMessageType.StatusResponse);
            }
            catch (Exception e)
            {
                Trace.TraceInformation(@"Status Response Failure {0} Exception {1}", deviceIpAddress, e.ToString());
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Failure };
            }

            return returnMessage;
        }

        public byte[] BearerAuthenticationUpdateResponse(string deviceIpAddress, byte[] message, out RLMStatus status)
        {
            byte[] returnMessage = new byte[0];
            try
            {
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };
                BearerAuthenticationUpdateResponse bearerAuthenticationUpdateResponse = new BearerAuthenticationUpdateResponse();
                bearerAuthenticationUpdateResponse.Status = BitConverter.ToUInt16(message.Skip(6).Take(2).Reverse().ToArray(), 0);
                bearerAuthenticationUpdateResponse.UserRef = BitConverter.ToUInt16(message.Skip(8).Take(2).Reverse().ToArray(), 0);
                bearerAuthenticationUpdateResponse.Slot = BitConverter.ToUInt16(message.Skip(10).Take(2).Reverse().ToArray(), 0);

                RLMDevice rlmDevice;
                _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);

                if (bearerAuthenticationUpdateResponse.Status != Definitions.SuccessStats || bearerAuthenticationUpdateResponse.UserRef != Definitions.UserRef)
                {
                    status.Status = RLMStatus.StatusEnum.Failure;
                    Trace.TraceInformation(@"Bearer Authentication Update Response Failure {0}", rlmDevice.SerialNo);
                }
                else
                {
                    Trace.TraceInformation(@"Bearer Authentication Update Response {0}", rlmDevice.SerialNo);
                }

                _logManager.Create(deviceIpAddress, rlmDevice.SerialNo, bearerAuthenticationUpdateResponse, Definitions.LogMessageType.BearerAuthenticationUpdateResponse);
            }
            catch (Exception e)
            {
                Trace.TraceInformation(@"Bearer Authentication Update Response Failure {0} Exception {1}", deviceIpAddress, e.ToString());
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Failure };
            }

            return returnMessage;
        }

        // Awaiting Response from CC before continuing
        public byte[] BearerAuthenticationReadResponse(string deviceIpAddress, byte[] message, out RLMStatus status)
        {
            byte[] returnMessage = new byte[0];

            try
            {
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };
                RLMDevice rlmDevice;
                _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);

                // Todo get information!
                BearerAuthenticationReadResponse bearerAuthenticationReadResponse = new BearerAuthenticationReadResponse();
                bearerAuthenticationReadResponse.Status = (Definitions.Status)BitConverter.ToUInt16(message.Skip(6).Take(2).Reverse().ToArray(), 0);
                bearerAuthenticationReadResponse.UserRef = BitConverter.ToUInt16(message.Skip(8).Take(2).Reverse().ToArray(), 0);
                bearerAuthenticationReadResponse.Slot = BitConverter.ToUInt16(message.Skip(10).Take(2).Reverse().ToArray(), 0);

                // Add bearer information to list
                bearerAuthenticationReadResponse.BearerAuthInformation.AuthType = (Definitions.AuthenicationType)BitConverter.ToUInt16(message.Skip(12).Take(2).Reverse().ToArray(), 0);
                var SSIDLength = BitConverter.ToUInt16(message.Skip(14).Take(1).ToArray(), 0);
                bearerAuthenticationReadResponse.BearerAuthInformation.SSID = BitConverter.ToString(message.Skip(15).Take(SSIDLength).Reverse().ToArray(), 0);
                rlmDevice.BearerAuthInformationList.Add(bearerAuthenticationReadResponse.BearerAuthInformation);

                // Determine if you need another to request another slot
                if (rlmDevice.BearerSlotNumber >= Definitions.MaxBearerSlot)
                {
                    // Add bearer info into REDIS, clean up RLMDevice, and PUB Message 
                    _redisDbRepository.StringSet(rlmDevice.SerialNo, rlmDevice);
                    rlmDevice.BearerSlotNumber = 0;
                    _redisDbRepository.Publish(Definitions.BearerInfoRLMDevice, rlmDevice.SerialNo);
                }
                else
                {
                    // Build up for next message
                    returnMessage = BearerAuthenticationReadIndication(deviceIpAddress);
                }

                Trace.TraceInformation(@"Bearer Authentication Read Response {0}", rlmDevice.SerialNo);
                _logManager.Create(deviceIpAddress, rlmDevice.SerialNo, bearerAuthenticationReadResponse, Definitions.LogMessageType.BearerAuthenticationReadResponse);
            }
            catch (Exception e)
            {
                Trace.TraceInformation(@"Bearer Authentication Read Response Failure {0} Exception {1}", deviceIpAddress, e.ToString());
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Failure };
            }
            return returnMessage;
        }

        public byte[] LimitWarningRequest(string deviceIpAddress, byte[] message, out RLMStatus status)
        {
            byte[] returnMessage = new byte[0];
            try
            {
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };
                LimitWarningRequest limitWarningRequest = new LimitWarningRequest();

                var bearer = BitConverter.ToUInt16(message.Skip(6).Take(2).Reverse().ToArray(), 0);
                limitWarningRequest.LimitRequest.Bearer = (Definitions.Bearer)bearer;

                //limitWarningRequest.LimitRequest.Bearer = (Definitions.Bearer)BitConverter.ToUInt16(message.Skip(6).Take(2).Reverse().ToArray(), 0);

                limitWarningRequest.LimitRequest.Bytes = BitConverter.ToInt32(message.Skip(8).Take(4).Reverse().ToArray(), 0);
                limitWarningRequest.LimitRequest.Percent = BitConverter.ToInt16(message.Skip(12).Take(2).Reverse().ToArray(), 0);

                _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out RLMDevice rlmDevice);

                // put back when confirmed warning
                returnMessage = General.GenerateRequest(Definitions.LimitWarningConfirm, rlmDevice);

                // Update Bearer to be same as above
                returnMessage[returnMessage.Length - 1] = Convert.ToByte(limitWarningRequest.LimitRequest.Bearer);

                Trace.TraceInformation(@"Limit Warning Request {0}", rlmDevice.SerialNo);
                _logManager.Create(deviceIpAddress, rlmDevice.SerialNo, limitWarningRequest, Definitions.LogMessageType.LimitWarningRequest);
            }
            catch (Exception e)
            {
                Trace.TraceInformation(@"Limit Warning Request Failure {0} Exception {1}", deviceIpAddress, e.ToString());
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Failure };
            }

            return returnMessage;
        }

        public byte[] LimitCriticalRequest(string deviceIpAddress, byte[] message, out RLMStatus status)
        {
            byte[] returnMessage = new byte[0];
            try
            {
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };
                LimitCriticalRequest limitCriticalRequest = new LimitCriticalRequest();
                var bearer = BitConverter.ToUInt16(message.Skip(6).Take(2).Reverse().ToArray(), 0);
                limitCriticalRequest.LimitRequest.Bearer = (Definitions.Bearer)bearer;

                //limitCriticalRequest.LimitRequest.Bearer = (Definitions.Bearer)BitConverter.ToUInt16(message.Skip(6).Take(2).Reverse().ToArray(), 0);

                limitCriticalRequest.LimitRequest.Bytes = BitConverter.ToInt32(message.Skip(8).Take(4).Reverse().ToArray(), 0);
                limitCriticalRequest.LimitRequest.Percent = BitConverter.ToInt16(message.Skip(12).Take(2).Reverse().ToArray(), 0);

                RLMDevice rlmDevice;
                _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);

                returnMessage = General.GenerateRequest(Definitions.LimitWarningConfirm, rlmDevice);

                // Update Bearer to be same as above
                returnMessage[returnMessage.Length - 1] = Convert.ToByte(limitCriticalRequest.LimitRequest.Bearer);

                Trace.TraceInformation(@"Limit Critical Request {0}", rlmDevice.SerialNo);
                _logManager.Create(deviceIpAddress, rlmDevice.SerialNo, limitCriticalRequest, Definitions.LogMessageType.LimitCriticalRequest);
            }
            catch (Exception e)
            {
                Trace.TraceInformation(@"Limit Warning Request Failure {0} Exception {1}", deviceIpAddress, e.ToString());
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Failure };
            }

            return returnMessage;
        }
        #endregion

        #region Sending
        public byte[] StatusIndication(string deviceIpAddress)
        {
            byte[] returnMessage = new byte[0];
            try
            {
                RLMDevice rlmDevice;
                _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);

                returnMessage = General.GenerateRequest(Definitions.StatusIndication, rlmDevice);               

                Trace.TraceInformation(@"Status Indication {0}", rlmDevice.SerialNo);
                _logManager.Create(deviceIpAddress, rlmDevice.SerialNo, string.Format("Status Indication {0}", rlmDevice.SerialNo), Definitions.LogMessageType.StatusIndication);
            }
            catch (Exception e)
            {
                Trace.TraceError(@"Bearer Change Indication Failure {0} Exception {1}", deviceIpAddress, e.ToString());
            }

            return returnMessage;
        }

        public byte[] BearerAuthenticationUpdateIndication(string deviceIpAddress, Authorization authorization)
        {
            byte[] returnMessage = new byte[0];
            try
            {
                var returnList = Definitions.BearerAuthenticationUpdateIndication;

                // Update Slot
                returnList[11] = Convert.ToByte(authorization.AuthorizationInfo.Slot);

                // If note deleting add messages
                if (!authorization.AuthorizationInfo.DeleteCredential)
                {
                    // Build Up Message
                    returnList[9] = Convert.ToByte(authorization.AuthorizationInfo.BearerType);
                    returnList[13] = Convert.ToByte(authorization.AuthorizationInfo.AuthType);

                    // Get string length and convert ASCII to byte 
                    byte SSIDLength = Convert.ToByte(authorization.AuthorizationInfo.SSID.Length);
                    byte[] SSID = Encoding.ASCII.GetBytes(authorization.AuthorizationInfo.SSID);

                    byte PSKLength = Convert.ToByte(authorization.AuthorizationInfo.PSK.Length);
                    byte[] PSK = Encoding.ASCII.GetBytes(authorization.AuthorizationInfo.PSK);

                    // Add all messages
                    returnList.Add(SSIDLength);
                    returnList.AddRange(SSID);
                    returnList.Add(PSKLength);
                    returnList.AddRange(PSK);
                }
                else
                {
                    // Add empty SSID and PSK for Deleting
                    returnList.AddRange(Definitions.EmptySSIDPSK);
                }

                RLMDevice rlmDevice;
                _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);
                returnMessage = General.GenerateRequest(returnList.ToArray(), rlmDevice);
                                
                Trace.TraceInformation(@"Bearer Authentication Update Indication {0}", rlmDevice.SerialNo);
                _logManager.Create(deviceIpAddress, rlmDevice.SerialNo, string.Format("Bearer Authentication Update Indication {0}", rlmDevice.SerialNo), Definitions.LogMessageType.BearerAuthenticationUpdateIndication);
            }
            catch (Exception e)
            {
                Trace.TraceError(@"Bearer Authentication Update Indication Failure {0} Exception {1}", deviceIpAddress, e.ToString());
            }

            return returnMessage;
        }
        
        public byte[] BearerAuthenticationReadIndication(string deviceIpAddress)
        {
            byte[] returnMessage = new byte[0];
            try
            {
                var returnList = Definitions.BearerAuthenticationReadIndication;
                
                RLMDevice rlmDevice;
                _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);

                // Build Up Message
                byte[] bearerSlotNumberBytes = BitConverter.GetBytes(rlmDevice.BearerSlotNumber++);

                // todo check
                returnList[9] = bearerSlotNumberBytes[1];
                returnMessage = General.GenerateRequest(returnList.ToArray(), rlmDevice);

                Trace.TraceInformation(@"Bearer Authentication Update Indication {0}", rlmDevice.SerialNo);
                _logManager.Create(deviceIpAddress, rlmDevice.SerialNo, string.Format("Bearer Authentication Update Indication {0}", rlmDevice.SerialNo), Definitions.LogMessageType.BearerAuthenticationUpdateIndication);
            }
            catch (Exception e)
            {
                Trace.TraceError(@"Bearer Authentication Update Indication Failure {0} Exception {1}", deviceIpAddress, e.ToString());
            }

            return returnMessage;
        }       
        #endregion        
    }
}
