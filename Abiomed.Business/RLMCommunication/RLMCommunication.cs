using Abiomed.Models;
using Abiomed.Repository;
using log4net;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Abiomed.Business
{
    public class RLMCommunication : IRLMCommunication
    {
        private readonly ILog _log;
        private RLMDeviceList _RLMDeviceList;
        private string _currentDevice;
        private byte[] returnMessage;
        public event EventHandler SendMessage;

        public RLMCommunication(ILog logger, RLMDeviceList RLMDeviceList)
        {
            _log = logger;
            _RLMDeviceList = RLMDeviceList;            
       } 

        private void BearerRequest(byte[] message)
        {

        }

        private byte[] SessionRequest(byte[] message, out RLMStatus status)
        {
            byte[] returnMessage = new byte[0];
            try
            {
                SessionOpen sessionOpen = new SessionOpen();
                if (BitConverter.IsLittleEndian)
                {
                    sessionOpen.MsgSeq = BitConverter.ToUInt16(message.Skip(4).Take(2).Reverse().ToArray(), 0);
                    sessionOpen.IfaceVer = BitConverter.ToUInt16(message.Skip(6).Take(2).Reverse().ToArray(), 0);
                    sessionOpen.SerialNo = Encoding.ASCII.GetString(message.Skip(8).Take(8).ToArray());
                    sessionOpen.Bearer = (Definitions.Bearer)BitConverter.ToUInt16(message.Skip(16).Take(2).Reverse().ToArray(), 0);                    
                    sessionOpen.Text = Encoding.ASCII.GetString(message.Skip(18).Take(message.Length - 18).ToArray());
                }
                else
                {
                    //Todo
                    //state.payloadLength = BitConverter.ToUInt16(state.buffer, 2);
                }

                RLMDevice rlmDevice = new RLMDevice()
                {
                    Bearer = sessionOpen.Bearer,
                    IfaceVer = sessionOpen.IfaceVer,
                    SerialNo = sessionOpen.SerialNo,
                    ClientSequence = sessionOpen.MsgSeq,
                    KeepAliveTimer = new KeepAliveTimer(_currentDevice)
                };

                // Add or update dictionary
                _RLMDeviceList.RLMDevices[_currentDevice] = rlmDevice;

                _RLMDeviceList.RLMDevices[_currentDevice].KeepAliveTimer.ThresholdReached += KeepAliveTimer_ThresholdReached;

                _log.InfoFormat(@"Session Request Session {0}", _RLMDeviceList.RLMDevices[_currentDevice].SerialNo);
                
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };
                var sessionMessage = GenerateRequest(Definitions.SessionConfirm);

                // Check if we will video stream or screen grab
                byte[] streamIndicator = new byte[0];
                if (rlmDevice.Bearer != Definitions.Bearer.LTE)
                {
                    streamIndicator = GenerateRequest(Definitions.StreamViedoControlIndications);
                }
                else
                {

                }

                // Append to current Byte[]
                returnMessage = new byte[sessionMessage.Length + streamIndicator.Length];
                sessionMessage.CopyTo(returnMessage, 0);
                streamIndicator.CopyTo(returnMessage, sessionMessage.Length);                    
            }
            catch(Exception e)
            {
                _log.InfoFormat(@"Session Request Failure {0}", _RLMDeviceList.RLMDevices[_currentDevice].SerialNo);
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Failure };
            }
            
            return returnMessage;
        }

        private void KeepAliveTimer_ThresholdReached(object sender, EventArgs e)
        {
            RLMDevice RLMDevice;
            _RLMDeviceList.RLMDevices.TryGetValue(_currentDevice, out RLMDevice);

            // If null, message already processed and session cleared
            if (RLMDevice != null)
            {
                // Throw away session info build session message and fire event to server
                _log.InfoFormat(@"Keep Alive Threshold Reached {0}", _RLMDeviceList.RLMDevices[_currentDevice].SerialNo);
                
                var ev = (CommunicationsEvent)e;
                ev.Message = GenerateRequest(Definitions.SessionCloseIndicator);
                RLMDevice rLMDevice;
                _RLMDeviceList.RLMDevices.TryRemove(ev.Identifier, out rLMDevice);
                SendMessage?.Invoke(sender, ev);
            }
            else
            { 
                _log.InfoFormat(@"Keep Alive Threshold Reached, with RLM already off list. {0}", _currentDevice);
            }            
        }

        private byte[] KeepAlive(byte[] message, out RLMStatus status)
        {
            // Push back timer
            _RLMDeviceList.RLMDevices[_currentDevice].KeepAliveTimer.UpdateTimer();
            status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };
            return new byte[0];
        }

        private byte[] BufferStatusRequest(byte[] message, out RLMStatus status)
        {
            status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };
            return new byte[0];
        }

        private byte[] SessionCancel(byte[] message, out RLMStatus status)
        {
            _log.InfoFormat(@"Session Cancel {0}", _RLMDeviceList.RLMDevices[_currentDevice].SerialNo);
            status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };
            return new byte[0];
        }

        /// <summary>
        /// Generates message to be sent back to client
        /// </summary>
        /// <returns></returns>
        private byte[] GenerateRequest(byte[] msg)
        {
            // Update Payload
            var payload = BitConverter.GetBytes(msg.Length - Definitions.MsgHeaderLength);
            msg[2] = payload[1];
            msg[3] = payload[0];

            // Update sequence number
            UInt16 serverSequence =  _RLMDeviceList.RLMDevices[_currentDevice].ServerSequence++;
            byte[] serverSequenceBytes = BitConverter.GetBytes(serverSequence);
            msg[4] = serverSequenceBytes[0];
            msg[5] = serverSequenceBytes[1];

            return msg;
        }     

        private bool ValidateMessage(byte[] dataMessage, out processMessageFunc<byte[], RLMStatus, byte[]> messageToProcess)
        {
            var status = true;
            #region Validate Message ID
            // Grab first two bytes to determine message type, if invalid exit
            var msgId = BitConverter.ToUInt16(dataMessage.Take(2).Reverse().ToArray(), 0);

            var validMessage = processMessage.TryGetValue(msgId, out messageToProcess);            
            if(validMessage == false)
            {
                status = false;
                _log.InfoFormat("RLM {0} invalid message type", _currentDevice);
            }
            #endregion

            #region Payload
            if(status)
            {
                var payloadBytes = BitConverter.ToUInt16(dataMessage.Skip(2).Take(2).Reverse().ToArray(), 0);
                var difference = (dataMessage.Length - 6) - payloadBytes;
                if (difference != 0)
                {
                    status = false;
                    _log.InfoFormat("RLM {0} invalid payload size, expected {1}, received {2}", _currentDevice, (dataMessage.Length - 6), payloadBytes);
                }

            }
            #endregion

            #region Sequence Number and Session has started
            if (status)
            {
                // Get sequence number and store
                UInt16 sequence = BitConverter.ToUInt16(dataMessage.Skip(4).Take(2).ToArray(), 0);

                RLMDevice RLM;
                _RLMDeviceList.RLMDevices.TryGetValue(_currentDevice, out RLM);

                // Check if exist, may be first round
                if (RLM != null)
                {                    
                    // Ensure the current sequence is less than 1. If not, throw status flag
                    if ((sequence - 1) != RLM.ClientSequence)
                    {
                        status = false;
                        _log.InfoFormat("RLM {0} invalid sequence number expected {1}, received {2}", _currentDevice, (RLM.ClientSequence + 1), sequence);
                    }
                    RLM.ClientSequence = sequence;
                }
                // First round, make sure sequence = 0, but if Session Start okay!
                // TODO TODO TODO!
                else if(sequence != 0 || msgId != Definitions.SessionRequest)
                {
                    status = false;
                    _log.InfoFormat("RLM {0} invalid sequence number or not session start message. expected 0, received sequence {1}, MSG", _currentDevice, sequence, msgId);
                }
            }
            #endregion          


            return status;
        }

        #region Process Message Dictionary
        private delegate TV processMessageFunc<in T, TU, out TV>(T input, out TU output);

        private Dictionary<UInt16, processMessageFunc<byte[], RLMStatus, byte[]>> processMessageDictionary = new Dictionary<UInt16, processMessageFunc<byte[], RLMStatus, byte[]>>();
        private Dictionary<UInt16, processMessageFunc<byte[], RLMStatus, byte[]>> processMessage
        {
            get
            {
                if (processMessageDictionary.Count == 0)
                {
                    processMessageDictionary = new Dictionary<UInt16, processMessageFunc<byte[], RLMStatus, byte[]>>()
                    {
                        {Definitions.SessionRequest, SessionRequest},
                        {Definitions.KeepAlive, KeepAlive},
                        {Definitions.BufferStatusRequest, BufferStatusRequest},
                        {Definitions.SessionCancel, SessionCancel }
                    };
                }
                return processMessageDictionary;
            }
        }

        public byte[] ProcessMessage(string deviceId, byte[] dataMessage, out RLMStatus status)
        {
            // Assume success until told otherwise
            status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };

            try
            {                
                // Set current device
                _currentDevice = deviceId;

                // Init return message
                returnMessage = new byte[0];

                // If valid sequence number continue
                processMessageFunc<byte[], RLMStatus, byte[]> messageToProcess;
                if (ValidateMessage(dataMessage, out messageToProcess))
                {
                    // Validated, process message
                    returnMessage = messageToProcess(dataMessage, out status);                   
                }
                else
                {       
                    // Error in data, send request to cancel Session and Close Stream!                           
                    CommunicationsEvent communicationEvent = new CommunicationsEvent();
                    communicationEvent.Identifier = _currentDevice;
                    communicationEvent.Message = GenerateRequest(Definitions.SessionCloseIndicator);
                    RLMDevice rLMDevice;
                    _RLMDeviceList.RLMDevices.TryRemove(_currentDevice, out rLMDevice);

                    SendMessage?.Invoke(this, communicationEvent);
                }
                
            }
            catch(Exception e)
            {
                _log.Error(@"Exception ", e);                
            }
            return returnMessage;
        }
        #endregion
    }
}
