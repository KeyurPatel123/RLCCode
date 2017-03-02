/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * RLMCommunication.cs: Main Business Logic between RLM and RLR
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using Abiomed.Models;
using Abiomed.Models.Communications;
using log4net;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StackExchange.Redis;
using System.Collections.Concurrent;

namespace Abiomed.Business
{
    public class RLMCommunication : IRLMCommunication
    {
        private readonly ILog _log;
        private RLMDeviceList _RLMDeviceList;
        private Configuration _configuration;
        private string _currentDevice;
        private byte[] returnMessage;
        public event EventHandler SendMessage;
        private ConcurrentDictionary<string, RLMDevice> _oldDevices = new ConcurrentDictionary<string, RLMDevice>();
      //  private ConnectionMultiplexer _redis;
      //  private IDatabase _db;
      //  private ISubscriber _sub;
      //  private int messageCount = 1;

        public RLMCommunication(ILog logger, RLMDeviceList RLMDeviceList, Configuration Configuration)
        {
            _log = logger;
            _RLMDeviceList = RLMDeviceList;
            _configuration = Configuration;
            
            //_redis = ConnectionMultiplexer.Connect("localhost");
            //_db = _redis.GetDatabase();
            //_sub = _redis.GetSubscriber();
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
                    sessionOpen.SerialNo = Encoding.ASCII.GetString(message.Skip(8).Take(7).ToArray());
                    sessionOpen.Bearer = (Definitions.Bearer)BitConverter.ToUInt16(message.Skip(15).Take(2).Reverse().ToArray(), 0);                    
                    sessionOpen.Text = Encoding.ASCII.GetString(message.Skip(17).Take(message.Length - 18).ToArray());
                }
                else
                {
                    //Todo
                    //state.payloadLength = BitConverter.ToUInt16(state.buffer, 2);
                }

                RLMDevice rlmDevice = new RLMDevice()
                {
                    Identifier = _currentDevice,
                    ConnectionTime = DateTime.UtcNow,
                    Bearer = sessionOpen.Bearer,
                    IfaceVer = sessionOpen.IfaceVer,
                    SerialNo = sessionOpen.SerialNo,
                    ClientSequence = sessionOpen.MsgSeq,
                    KeepAliveTimer = new KeepAliveTimer(_currentDevice, _configuration.KeepAliveTimer)
                };

                // Add or update dictionary
                var device = _RLMDeviceList.RLMDevices.Where(x => x.Value.SerialNo == rlmDevice.SerialNo).FirstOrDefault();

                if (!device.Equals(new KeyValuePair<string, RLMDevice>()))
                {
                    _log.InfoFormat(@"Found duplicate RLM Serial {0}", _RLMDeviceList.RLMDevices[device.Value.Identifier].SerialNo);

                    device.Value.KeepAliveTimer.DestroyTimer();

                    RLMDevice tempDevice;
                    _RLMDeviceList.RLMDevices.TryRemove(device.Key, out tempDevice);
                }                

                _RLMDeviceList.RLMDevices[_currentDevice] = rlmDevice;

                _RLMDeviceList.RLMDevices[_currentDevice].KeepAliveTimer.ThresholdReached += KeepAliveTimer_ThresholdReached;

                _log.InfoFormat(@"Session Request Session {0}", _RLMDeviceList.RLMDevices[_currentDevice].SerialNo);
                
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };
                var sessionMessage = GenerateRequest(Definitions.SessionConfirm);

                // Check if we will video stream or screen grab
                byte[] streamIndicator = new byte[0];

                var videoControl = VideoControlGeneration(rlmDevice.Bearer, rlmDevice.SerialNo, Definitions.StreamVideoBase);
                streamIndicator = GenerateRequest(videoControl);
                _RLMDeviceList.RLMDevices[_currentDevice].Streaming = true;

                /* Re-enable after M1
                if (rlmDevice.Bearer != Definitions.Bearer.LTE)
                {
                    var videoControl = VideoControlGeneration(rlmDevice.Bearer, rlmDevice.SerialNo, Definitions.StreamVideoBase);
                    streamIndicator = GenerateRequest(videoControl);
                    _RLMDeviceList.RLMDevices[_currentDevice].Streaming = true;
                }
                else
                {
                    // Ask for image 3
                    streamIndicator = GenerateRequest(Definitions.ScreenCaptureIndicator);
                    _RLMDeviceList.RLMDevices[_currentDevice].FileTransfer = true;
                }
                */
                // Append to current Byte[]
                returnMessage = new byte[sessionMessage.Length + streamIndicator.Length];
                sessionMessage.CopyTo(returnMessage, 0);
                streamIndicator.CopyTo(returnMessage, sessionMessage.Length);

                // Send Update
                UpdateSubscribedServers();
            }
            catch(Exception e)
            {
                _log.InfoFormat(@"Session Request Failure {0} Exception {1}", _RLMDeviceList.RLMDevices[_currentDevice].SerialNo, e.ToString());
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Failure };
            }
            
            return returnMessage;
        }

        private byte[] VideoControlGeneration(Definitions.Bearer bearer, string serialNumber, List<byte> streamVideoControlIndications)
        {
            // todo look at bearer!

            // Convert SerialNumber to ASCII, add to list and convert out as byte[]
            var serialBytes = Encoding.ASCII.GetBytes(serialNumber);

            // Add length 
            List<byte> streamControl = new List<byte>(streamVideoControlIndications);                       
            streamControl.Add(Convert.ToByte(serialNumber.Length));
            streamControl.AddRange(serialBytes);
            return streamControl.ToArray();
        }

        private byte[] StreamVideoResponse(byte[] message, out RLMStatus status)
        {
            status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };

            // Check for status and user ref
            var statusCode = BitConverter.ToUInt16(message.Skip(6).Take(2).Reverse().ToArray(), 0);
            var userRef = BitConverter.ToUInt16(message.Skip(8).Take(2).Reverse().ToArray(), 0);
            
            // Error checking
            if(statusCode != Definitions.SuccessStats || userRef != Definitions.UserRef)
            {
                // Close Current Session
                SessionCloseIndicator();
            }

            return new byte[0];
        }

        private byte[] ScreenCaptureResponse(byte[] message, out RLMStatus status)
        {
            status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success};

            byte[] returnMessage = new byte[0];
            try
            {
                ScreenCaptureResponse screenCaptureResponse = new ScreenCaptureResponse();
                if (BitConverter.IsLittleEndian)
                {
                    screenCaptureResponse.Status = BitConverter.ToUInt16(message.Skip(6).Take(2).Reverse().ToArray(), 0);
                    screenCaptureResponse.UserRef = BitConverter.ToUInt16(message.Skip(8).Take(2).Reverse().ToArray(), 0);                    
                }
                else
                {
                    //Todo
                    //state.payloadLength = BitConverter.ToUInt16(state.buffer, 2);
                }

                // todo; check for status and userRef
                if(screenCaptureResponse.Status == 0 && screenCaptureResponse.UserRef == 3579)
                {
                    // Good to go!
                    returnMessage = GenerateRequest(Definitions.FileOpenIndicator);
                }
                else
                {
                    // Close Current Session
                    SessionCloseIndicator();
                }
            }
            catch(Exception e)
            {
                _log.InfoFormat(@"Screen Capture Response Failure {0} Exception {1}", _RLMDeviceList.RLMDevices[_currentDevice].SerialNo, e.ToString());
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Failure };
            }
            return returnMessage;
        }

        private byte[] FileOpenResponse(byte[] message, out RLMStatus status)
        {
            status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };

            FileOpenResponse fileOpenResponse = new FileOpenResponse();

            byte[] returnMessage = new byte[0];
            try
            {
                if (BitConverter.IsLittleEndian)
                {
                    fileOpenResponse.Status = BitConverter.ToUInt16(message.Skip(6).Take(2).Reverse().ToArray(), 0);
                    fileOpenResponse.UserRef = BitConverter.ToUInt16(message.Skip(8).Take(2).Reverse().ToArray(), 0);
                    fileOpenResponse.Size = BitConverter.ToUInt16(message.Skip(10).Take(4).Reverse().ToArray(), 0);

                    var time = BitConverter.ToUInt16(message.Skip(14).Take(8).Reverse().ToArray(), 0);
                    fileOpenResponse.Time = DateTime.FromFileTimeUtc(time);
                }
                else
                {
                    //Todo
                    //state.payloadLength = BitConverter.ToUInt16(state.buffer, 2);
                }

                // todo, add for block x not just 0
                returnMessage = GenerateRequest(Definitions.FileReadIndicator);

            }
            catch (Exception e)
            {
                _log.InfoFormat(@"File Open Response Failure {0} Exception {1}", _RLMDeviceList.RLMDevices[_currentDevice].SerialNo, e.ToString());
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Failure };
            }
            return returnMessage;
        }

        private byte[] FileReadResponse(byte[] message, out RLMStatus status)
        {
            status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };

            FileReadResponse fileReadResponse = new FileReadResponse();

            byte[] returnMessage = new byte[0];
            try
            {
                if (BitConverter.IsLittleEndian)
                {
                    var payloadBytes = BitConverter.ToUInt16(message.Skip(2).Take(2).Reverse().ToArray(), 0);
                    fileReadResponse.Status = BitConverter.ToUInt16(message.Skip(6).Take(2).Reverse().ToArray(), 0);
                    fileReadResponse.UserRef = BitConverter.ToUInt16(message.Skip(8).Take(2).Reverse().ToArray(), 0);
                    fileReadResponse.Data = message.Skip(10).Take(payloadBytes).Reverse().ToArray();
                }
                else
                {
                    //Todo
                    //state.payloadLength = BitConverter.ToUInt16(state.buffer, 2);
                }

                // todo, determine if full message, if so send file close ind and send off to Web Server, otherwise ask for next block

                // Assume full message for now; send to IIS; send file close
                SendImage(fileReadResponse.Data, _RLMDeviceList.RLMDevices[_currentDevice].SerialNo);
                returnMessage = GenerateRequest(Definitions.FileCloseIndicator);
            }
            catch (Exception e)
            {
                _log.InfoFormat(@"File Open Response Failure {0} Exception {1}", _RLMDeviceList.RLMDevices[_currentDevice].SerialNo, e.ToString());
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

            // Send updated list
            UpdateSubscribedServers();
        }

        private byte[] KeepAlive(byte[] message, out RLMStatus status)
        {
            // Push back timer
            _RLMDeviceList.RLMDevices[_currentDevice].KeepAliveTimer.UpdateTimer();
            _log.InfoFormat(@"Keep Alive RLM {0}", _RLMDeviceList.RLMDevices[_currentDevice].SerialNo);
            status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };
            return new byte[0];
        }

        private byte[] BufferStatusRequest(byte[] message, out RLMStatus status)
        {
            status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };

            // Check for status and user ref
            var statusCode = BitConverter.ToUInt16(message.Skip(6).Take(2).Reverse().ToArray(), 0);

            // Error checking
            if (statusCode != Definitions.SuccessStats)
            {
                // Close Current Session
            // put back after M1 and discuss with CC   
            // SessionCloseIndicator();
            }

            return new byte[0];
        }

        private byte[] SessionCancel(byte[] message, out RLMStatus status)
        {
            _log.InfoFormat(@"Session Cancel {0}", _RLMDeviceList.RLMDevices[_currentDevice].SerialNo);
            // Push back timer, for the last time.
            _RLMDeviceList.RLMDevices[_currentDevice].KeepAliveTimer.UpdateTimer();
            UpdateSubscribedServers();
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
            RLMDevice device;
            _RLMDeviceList.RLMDevices.TryGetValue(_currentDevice, out device);
            if(device != null)
            {
                UInt16 serverSequence = device.ServerSequence++;
                byte[] serverSequenceBytes = BitConverter.GetBytes(serverSequence);
                msg[4] = serverSequenceBytes[0];
                msg[5] = serverSequenceBytes[1];
            }
            else
            {
                // error device does not exist!
            }

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

            return true;

            // todo remove
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
                UInt16 sequence = BitConverter.ToUInt16(dataMessage.Skip(4).Take(2).Reverse().ToArray(), 0);

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
                        {Definitions.SessionCancel, SessionCancel },
                        {Definitions.StreamVideoResponse, StreamVideoResponse},
                        { Definitions.ScreenCaptureResponse, ScreenCaptureResponse},
                        {Definitions.FileOpenResponse, FileOpenResponse },
                        {Definitions.FileReadResponse, FileOpenResponse }
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
                
                processMessageFunc<byte[], RLMStatus, byte[]> messageToProcess;
                if (ValidateMessage(dataMessage, out messageToProcess))
                {
                    // Validated, process message
                    returnMessage = messageToProcess(dataMessage, out status);                   
                }
                else
                {
                    // Close Current Session
                    SessionCloseIndicator();
                }
                
           } 
            catch(Exception)
            {
                _log.InfoFormat(@"RLM {0} already removed, during processing of message received", deviceId);                    
            }
            return returnMessage;
        }
        #endregion

        private void SessionCloseIndicator()
        {
            // Error in data, send request to cancel Session and Close Stream!                           
            CommunicationsEvent communicationEvent = new CommunicationsEvent();
            communicationEvent.Identifier = _currentDevice;
            communicationEvent.Message = GenerateRequest(Definitions.SessionCloseIndicator);
            RLMDevice rLMDevice;
            _RLMDeviceList.RLMDevices.TryRemove(_currentDevice, out rLMDevice);
            UpdateSubscribedServers();
            SendMessage?.Invoke(this, communicationEvent);
        }

        public void UpdateSubscribedServers()
        {
            //var status = General.CompareDictionaries(_oldDevices, _RLMDeviceList.RLMDevices);
            //Console.WriteLine("Dict status " + status);

            // If not equal, update
            //if (!status)
            {
                List<DeviceStatus> devices = new List<DeviceStatus>();

                try
                {
                    foreach (var device in _RLMDeviceList.RLMDevices)
                    {
                        DeviceStatus ds = new DeviceStatus { Bearer = device.Value.Bearer.ToString(), ConnectionTime = device.Value.ConnectionTime, SerialNumber = device.Value.SerialNo };
                        devices.Add(ds);
                    }

                    // Sort!

                    var sortedDevices = devices.OrderBy(o => o.SerialNumber).ToList();

                    //var deviceStatus = JsonConvert.SerializeObject(devices);
                    //
                    //_sub.Publish(@"RLMUpdate", deviceStatus);

                    var client = new RestClient(_configuration.DeviceStatus);
                    var request = new RestRequest(Method.POST);

                    var deviceStatus = JsonConvert.SerializeObject(sortedDevices);

                    request.AddParameter("application/json; charset=utf-8", deviceStatus, ParameterType.RequestBody);

                    client.ExecuteAsync(request, response =>
                    {
                        Console.WriteLine(response.Content);
                    });

                    // Update Old Devices                    
                    _oldDevices = new ConcurrentDictionary<string, RLMDevice>(_RLMDeviceList.RLMDevices);
                }
                catch (Exception)
                {
                    _log.Error(@"Error transmitted updated devices");
                }
            }
        }

        private void SendImage(byte[] image, string serialNumber)
        {
            var client = new RestClient(_configuration.ImageSend);
            var request = new RestRequest(Method.POST);
            RLMImage rLMImage = new RLMImage { Data = image, SerialNumber = serialNumber };
            request.AddParameter(@"rLMImage", rLMImage);

            //IRestResponse response;
            client.ExecuteAsync(request, response => {
                Console.WriteLine(response.Content);
            });
            
        }

        public bool StartVideo(string serialNumber)
        {
            bool status = true;
            
            // Go through list of active devices, find serial number and send message
            var rlmDevice = _RLMDeviceList.RLMDevices.Values.FirstOrDefault(x => x.SerialNo == serialNumber);
            
            if(rlmDevice == null)
            {
                status = false;
            }

            if (status)
            {
                CommunicationsEvent ev = new CommunicationsEvent();
                ev.Identifier = rlmDevice.Identifier;

                var videoControl = VideoControlGeneration(rlmDevice.Bearer, rlmDevice.SerialNo, Definitions.StreamVideoBase);
                // Update Generate Request to include device! Right now sequence will fail
                ev.Message = GenerateRequest(videoControl);

                SendMessage?.Invoke(this, ev);
            }
            return status;
        }

        public List<byte[]> SeperateMessages(byte[] dataMessage)
        {
            List<byte[]> messages = new List<byte[]>();
            bool multipleMessages = false;

            // Determine Payload
            var payloadBytes = BitConverter.ToUInt16(dataMessage.Skip(2).Take(2).Reverse().ToArray(), 0);
            var difference = (dataMessage.Length - 6) - payloadBytes;
            if (difference != 0)
            {
                multipleMessages = true;
                _log.InfoFormat(@"RLM sent multiple messages");
            }

            int offset = 0;
            try
            {
                // Separate messages
                while (multipleMessages)
                {
                    // Get length of message
                    var messageBytes = BitConverter.ToUInt16(dataMessage.Skip(offset + 2).Take(2).Reverse().ToArray(), 0);

                    // Grab message and add into List<byte[]>
                    byte[] data = dataMessage.Skip(offset).Take(messageBytes + 6).ToArray();

                    messages.Add(data);

                    offset += data.Count();
                    // Note: If message is incorrect, during validation of message individually, we will catch fault
                }
            }
            catch(Exception)
            {
                _log.Error(@"Error passing multiple messages");                
            }
            return messages;
        }

    }
}
