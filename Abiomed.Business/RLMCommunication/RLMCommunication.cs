/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * RLMCommunication.cs: Main Business Logic between RLM and RLR
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using Abiomed.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using Abiomed.Repository;
using System.Diagnostics;

namespace Abiomed.Business
{
    public class RLMCommunication : IRLMCommunication
    {
        private Configuration _configuration;
        private IDigitiserCommunication _digitiserCommunication;
        private IFileTransferCommunication _fileTransferCommunication;
        private ISessionCommunication _sessionCommunication;
        private IStatusControlCommunication _statusControlCommunication;
        private IRedisDbRepository<RLMDevice> _redisDbRepository;
        private IKeepAliveManager _keepAliveManager;
        private RLMDeviceList _rlmDeviceList;
        private byte[] returnMessage;
        private Dictionary<string, PartialPayload> _partialPayloadDictionary = new Dictionary<string, PartialPayload>();

        public RLMCommunication(Configuration Configuration, IDigitiserCommunication DigitiserCommunication, IFileTransferCommunication FileTransferCommunication, ISessionCommunication SessionCommunication, IStatusControlCommunication StatusControlCommunication, IRedisDbRepository<RLMDevice> redisDbRepository, IKeepAliveManager keepAliveManager, RLMDeviceList rlmDeviceList)
        {
            _configuration = Configuration;
            _digitiserCommunication = DigitiserCommunication;
            _fileTransferCommunication = FileTransferCommunication;
            _sessionCommunication = SessionCommunication;
            _statusControlCommunication = StatusControlCommunication;
            _redisDbRepository = redisDbRepository;
            _keepAliveManager = keepAliveManager;
            _rlmDeviceList = rlmDeviceList;
        }

        #region Process Message Dictionary
        private delegate TV processMessageFunc<in V, T, TU, out TV>(V id, T input, out TU output);

        private Dictionary<UInt16, processMessageFunc<string, byte[], RLMStatus, byte[]>> processMessageDictionary = new Dictionary<UInt16, processMessageFunc<string, byte[], RLMStatus, byte[]>>();
        private Dictionary<UInt16, processMessageFunc<string, byte[], RLMStatus, byte[]>> processMessage
        {
            get
            {
                if (processMessageDictionary.Count == 0)
                {
                    processMessageDictionary = new Dictionary<UInt16, processMessageFunc<string, byte[], RLMStatus, byte[]>>()
                    {
                        #region Session
                        {Definitions.SessionRequest, _sessionCommunication.SessionRequest},
                        {Definitions.BearerRequest, _sessionCommunication.BearerRequest},
                        {Definitions.BearerChangeResponse, _sessionCommunication.BearerChangeResponse},
                        {Definitions.CloseBearerRequest, _sessionCommunication.CloseBearerRequest},
                        {Definitions.KeepAliveRequest, _sessionCommunication.KeepAliveRequest},
                        {Definitions.CloseSessionRequest, _sessionCommunication.CloseSessionRequest},
                        #endregion

                        #region Status Control
                        {Definitions.StatusResponse, _statusControlCommunication.StatusResponse},
                        {Definitions.BearerAuthenticationUpdateResponse, _statusControlCommunication.BearerAuthenticationUpdateResponse},
                        {Definitions.BearerAuthenticationReadResponse, _statusControlCommunication.BearerAuthenticationReadResponse},
                        {Definitions.LimitWarningRequest, _statusControlCommunication.LimitWarningRequest},
                        {Definitions.LimitCriticalRequest, _statusControlCommunication.LimitCriticalRequest},
                        #endregion

                        #region Digitiser
                        {Definitions.StreamVideoControlResponse, _digitiserCommunication.StreamingVideoControlResponse},
                        {Definitions.BufferStatusRequest, _digitiserCommunication.BufferStatusRequest},
                        {Definitions.ScreenCaptureResponse, _digitiserCommunication.ScreenCaptureResponse},
                        #endregion

                        #region File Transfer
                        {Definitions.OpenFileRequest, _fileTransferCommunication.OpenFileRequest},
                        {Definitions.OpenFileResponse, _fileTransferCommunication.OpenFileResponse},
                        {Definitions.DataReadRequest, _fileTransferCommunication.DataReadRequest},
                        {Definitions.DataReadResponse, _fileTransferCommunication.DataReadResponse},
                        {Definitions.ClearFileResponse, _fileTransferCommunication.ClearFileResponse},
                        #endregion 
                    };
                }
                return processMessageDictionary;
            }
        }

        public byte[] ProcessMessage(string deviceIpAddress, byte[] dataMessage, out RLMStatus status)
        {
            // Assume success until told otherwise
            status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };

            try
            {
                // Init return message
                returnMessage = new byte[0];

                processMessageFunc<string, byte[], RLMStatus, byte[]> messageToProcess;
                if (ValidateMessage(deviceIpAddress, dataMessage, out messageToProcess))
                {
                    // Validated, process message
                    returnMessage = messageToProcess(deviceIpAddress, dataMessage, out status);
                    if (status.Status == RLMStatus.StatusEnum.Failure)
                    {
                        CloseTCPSessionEvent(deviceIpAddress);
                    }
                }
                else
                {
                    CloseTCPSessionEvent(deviceIpAddress);
                }
            }
            catch (Exception)
            {
                Trace.TraceInformation(@"RLM {0} already removed, during processing of message received", deviceIpAddress);                    
            }
            return returnMessage;
        }
        #endregion

        #region Process Message Event Dictionary
        private delegate TV processMessageFuncEvent<in V, out TV>(V id);
        private Dictionary<string, processMessageFuncEvent<string, byte[]>> processMessageEventDictionary = new Dictionary<string, processMessageFuncEvent<string, byte[]>>();

        private Dictionary<string, processMessageFuncEvent<string, byte[]>> processMessageEvent
        {
            get
            {
                if (processMessageEventDictionary.Count == 0)
                {
                    processMessageEventDictionary = new Dictionary<string, processMessageFuncEvent<string, byte[]>>()
                    {                        
                        {Definitions.KeepAliveIndicationEvent, _sessionCommunication.KeepAliveIndication },
                        {Definitions.StatusIndicationEvent, _statusControlCommunication.StatusIndication },
                        {Definitions.BearerAuthenticationReadIndicationEvent, _statusControlCommunication.BearerAuthenticationReadIndication },
                        {Definitions.StreamingVideoControlIndicationEvent, _digitiserCommunication.StreamingVideoControlIndication },
                        {Definitions.ScreenCaptureIndicationEvent, _digitiserCommunication.ScreenCaptureIndication },
                        {Definitions.OpenRLMLogFileIndicationEvent, _fileTransferCommunication.OpenRLMLogFileIndication},
                        {Definitions.CloseSessionIndicationEvent, _sessionCommunication.CloseSessionIndication }
                    };
                } 
                return processMessageEventDictionary;
            }
        }

        public byte[] ProcessEvent(string deviceIpAddress, string message, string[] options)
        {
            // Init return message
            byte[] returnMessage = new byte[0];

            try
            {                
                // Split up messages via token '-'
                var splitMessage = message.Split('-');

                processMessageFuncEvent<string, byte[]> messageToProcess;
                var validMessage = processMessageEvent.TryGetValue(splitMessage[0], out messageToProcess);

                if (validMessage)
                {
                    returnMessage = messageToProcess(deviceIpAddress);
                }
                else
                {                    
                    if (message.Equals(Definitions.BearerChangeIndicationEvent))
                    {
                        Definitions.Bearer _bearer;
                        Enum.TryParse(options[0], out _bearer);
                        returnMessage = _sessionCommunication.BearerChangeIndication(deviceIpAddress, _bearer);
                    }
                    else if (message.Equals(Definitions.BearerAuthenticationUpdateIndicationEvent))
                    {
                        // todo check
                        if (options.Length < 2)
                        {
                            Authorization authorization = new Authorization();
                            authorization.AuthorizationInfo.Slot = Convert.ToUInt16(options[0]);
                            authorization.AuthorizationInfo.DeleteCredential = true;
                            returnMessage = _statusControlCommunication.BearerAuthenticationUpdateIndication(deviceIpAddress, authorization);
                        }
                        else
                        {
                            Authorization authorization = new Authorization();

                            //  slot, bearer, authentication type, SSID, PSK                        
                            authorization.AuthorizationInfo.Slot = Convert.ToUInt16(options[0]);

                            Definitions.Bearer bearer;
                            Enum.TryParse(options[1], out bearer);
                            authorization.AuthorizationInfo.BearerType = bearer;

                            Definitions.AuthenicationType authenicationType;
                            Enum.TryParse(options[2], out authenicationType);
                            authorization.AuthorizationInfo.AuthType = authenicationType;

                            authorization.AuthorizationInfo.SSID = options[3];

                            authorization.AuthorizationInfo.PSK = options[4];

                            returnMessage = _statusControlCommunication.BearerAuthenticationUpdateIndication(deviceIpAddress, authorization);
                        }
                    }
                }


            }
            catch (Exception)
            {
                //Trace.TraceInformation(@"RLM {0} already removed, during processing of message received", deviceIpAddress);                    
            }
            return returnMessage;
        }

        #endregion

        private void CloseTCPSessionEvent(string deviceIpAddress)
        {
            _redisDbRepository.Publish(Definitions.RemoveRLMDeviceRLR, deviceIpAddress);
        }

        public byte[] GenerateCloseSession(string deviceIpAddress)
        {
            return _sessionCommunication.CloseSessionIndication(deviceIpAddress);
        }

        public void RemoveRLMDeviceFromList(string deviceIpAddress)
        {
            RLMDevice rlmDevice;
            _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);

            if (rlmDevice != null)
            {
                // Delete from REDIS and local and Remove Keep Alive Timer
                _redisDbRepository.StringDelete(rlmDevice.SerialNo);
                _redisDbRepository.RemoveFromSet(Definitions.RLMDeviceSet, rlmDevice.SerialNo);
                _redisDbRepository.Publish(Definitions.DeleteRLMDevice, rlmDevice.SerialNo);

                _keepAliveManager.Remove(deviceIpAddress);
                _keepAliveManager.ImageTimerDelete(deviceIpAddress);
                _rlmDeviceList.RLMDevices.TryRemove(deviceIpAddress, out rlmDevice);
            }
        }

        public List<byte[]> SeperateMessages(string deviceIpAddress, byte[] dataMessage)
        {
            List<byte[]> messages = new List<byte[]>();
            bool multipleMessages = false;

            // Determine if continuing of partial message
            if (_partialPayloadDictionary.ContainsKey(deviceIpAddress))
            {
                PartialPayload partialPayload;
                _partialPayloadDictionary.TryGetValue(deviceIpAddress, out partialPayload);

                // Calculate missing area and grab the needed data and remove from array
                // Note _partialPayload does not have additional 6 bytes for header                
                int missingPayloadAmount = (partialPayload.Size + 6) - partialPayload.Message.Count;

                // todo check if sorted correctly!
                byte[] endOfPartial = dataMessage.Take(missingPayloadAmount).Reverse().ToArray();
                partialPayload.Message.AddRange(endOfPartial);

                byte[] fullMessage = partialPayload.Message.ToArray();

                messages.Add(fullMessage);

                // Clean up
                dataMessage = dataMessage.Skip(missingPayloadAmount).ToArray();
                _partialPayloadDictionary.Remove(deviceIpAddress);
            }

            // Determine Payload
            var payloadBytes = BitConverter.ToUInt16(dataMessage.Skip(2).Take(2).Reverse().ToArray(), 0);
            var difference = (dataMessage.Length - 6) - payloadBytes;
            if (difference != 0)
            {
                multipleMessages = true;
                Trace.TraceInformation(@"RLM sent multiple messages");
            }

            if (multipleMessages)
            {
                int offset = 0;
                try
                {
                    // Separate messages
                    while (multipleMessages)
                    {
                        // Get length of message
                        var payload = BitConverter.ToUInt16(dataMessage.Skip(offset + 2).Take(2).Reverse().ToArray(), 0);

                        // Grab message
                        byte[] data = dataMessage.Skip(offset).Take(payload + 6).ToArray();

                        // Determine if partial message                        
                        if (payload != (data.Length - 6))
                        {
                            Trace.TraceInformation(@"RLM sent Partial message");

                            // Hold onto message          
                            PartialPayload partialPayload = new PartialPayload();

                            partialPayload.Size = payload;
                            partialPayload.Message.AddRange(data);

                            _partialPayloadDictionary.Add(deviceIpAddress, partialPayload);
                        }
                        else
                        {
                            messages.Add(data);
                        }
                        offset += data.Count();
                        // Note: If message is incorrect, during validation of message individually, we will catch fault
                    }
                }
                catch (Exception)
                {
                    //_log.Error(@"Error passing multiple messages");                
                }
            }
            else // single message
            {
                messages.Add(dataMessage);
            }

            return messages;
        }

        private bool ValidateMessage(string deviceIpAddress, byte[] dataMessage, out processMessageFunc<string, byte[], RLMStatus, byte[]> messageToProcess)
        {

            var status = true;
            #region Validate Message ID
            // Grab first two bytes to determine message type, if invalid exit
            var msgId = BitConverter.ToUInt16(dataMessage.Take(2).Reverse().ToArray(), 0);

            var validMessage = processMessage.TryGetValue(msgId, out messageToProcess);
            if (validMessage == false)
            {
                status = false;
                Trace.TraceInformation("RLM {0} invalid message type", deviceIpAddress);
            }
            #endregion

            #region Payload
            if (status)
            {
                var payloadBytes = BitConverter.ToUInt16(dataMessage.Skip(2).Take(2).Reverse().ToArray(), 0);
                var difference = (dataMessage.Length - 6) - payloadBytes;
                if (difference != 0)
                {
                    status = false;
                    Trace.TraceInformation("RLM {0} invalid payload size, expected {1}, received {2}", deviceIpAddress, payloadBytes, (dataMessage.Length - 6));
                }

            }
            #endregion

            #region Sequence Number and Session has started
            if (status)
            {
                // Get sequence number and store
                UInt16 sequence = BitConverter.ToUInt16(dataMessage.Skip(4).Take(2).Reverse().ToArray(), 0);

                RLMDevice rlmDevice;
                _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);

                // Check if exist, may be first round
                if (rlmDevice != null)
                {
                    // Ensure the current sequence is less than 1. If not, throw status flag
                    if ((sequence - 1) != rlmDevice.ClientSequence)
                    {
                        status = false;
                        //Trace.TraceInformation("RLM {0} invalid sequence number expected {1}, received {2}", deviceIpAddress, (rlmDevice.ClientSequence + 1), sequence);
                    }
                    rlmDevice.ClientSequence = sequence;
                }
                // First round, make sure sequence = 0, but if Session Start okay!
                // TODO TODO TODO!, when restarting RLM need to see sequence, not a priority
                else if (sequence != 0 || msgId != Definitions.SessionRequest)
                {
                    //status = false;
                    //Trace.TraceInformation("RLM {0} invalid sequence number or not session start message. expected 0, received sequence {1}, MSG", deviceIpAddress, sequence, msgId);
                }
            }
            #endregion          
            return status;
        }

    }
}
