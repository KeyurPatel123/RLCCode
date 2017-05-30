/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * tcpserver.cs: ASYNCH TCP Server
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Abiomed.Business;
using System.Linq;
using System.Collections.Concurrent;
using Abiomed.Models;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using System.Net.Security;
using Abiomed.Repository;
using System.Diagnostics;
using System.Collections.Generic;
using StackExchange.Redis;

namespace Abiomed.RLR.Communications
{
    public class InsecureTcpServer
    {       
        private IRLMCommunication _RLMCommunication;
        private Configuration _configuration;
        private ConcurrentDictionary<string, TCPStateObjectInsecure> _tcpStateObjectList = new ConcurrentDictionary<string, TCPStateObjectInsecure>();
        private static ManualResetEvent allDone = new ManualResetEvent(false);
        private IRedisDbRepository<RLMDevice> _redisDbRepository;

        private List<RedisChannel> userInteractionEvents = new List<RedisChannel>()
        {
            Definitions.KeepAliveIndicationEvent,
            Definitions.BearerChangeIndicationEvent,
            Definitions.StatusIndicationEvent,
            Definitions.BearerAuthenticationReadIndicationEvent,
            Definitions.BearerAuthenticationUpdateIndicationEvent,
            Definitions.StreamingVideoControlIndicationEvent,
            Definitions.ScreenCaptureIndicationEvent,
            Definitions.OpenRLMLogFileIndicationEvent,
            Definitions.CloseSessionIndicationEvent
        };

        public InsecureTcpServer(IRLMCommunication RLMCommunication, Configuration configuration, IRedisDbRepository<RLMDevice> redisDbRepository)
        {
            _RLMCommunication = RLMCommunication;
            _configuration = configuration;
            _redisDbRepository = redisDbRepository;

            // Subscribe to removal of RLM Device
            _redisDbRepository.Subscribe(Definitions.RemoveRLMDeviceRLR, (channel, message) => {
                string deviceIpAddress = (string)message;
                RemoveConnection(deviceIpAddress);
                });

            // Subscribe to all user interactions!
            _redisDbRepository.Subscribe(userInteractionEvents, (channel, message) => {
                string msg = (string)message;
                string deviceIpAddress;
                string[] msgSplit = new string[0];

                if (msg.Contains("-"))
                {
                    msgSplit = msg.Split('-');
                    deviceIpAddress = msgSplit[0];
                    msgSplit = msgSplit.Skip(1).ToArray();
                }
                else
                {
                    deviceIpAddress = msg;
                }
                ProcessUserInteractionEvent(deviceIpAddress, channel, msgSplit);                
            });
        }

        public void Run()
        {
            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                var listener = new TcpListener(IPAddress.Any, _configuration.TcpPort);
                listener.Start();
                Trace.TraceInformation("Insecure TCP Server Started Success");
                

                while (true)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    listener.BeginAcceptTcpClient(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Trace.TraceError("Cannot Start TCP Service Error ", e);
            }

        }

        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                // Signal the main thread to continue.
                allDone.Set();


                TcpListener listener = (TcpListener)ar.AsyncState;
                TcpClient handler = listener.EndAcceptTcpClient(ar);

                // todo add try catch with bad creds!?

                // Ensure RLM serial number is on approved list!

                // Connect to Stream and Authenticate
                var networkStream = handler.GetStream();

                // Create the state object and add to list
                TCPStateObjectInsecure state = new TCPStateObjectInsecure();
                state.TcpClient = handler;
                state.WorkStream = networkStream;
                state.DeviceIpAddress = handler.Client.RemoteEndPoint.ToString();
                _tcpStateObjectList.TryAdd(state.DeviceIpAddress, state);

                Trace.TraceInformation("RLM connected at connection {0}", state.DeviceIpAddress);
                state.WorkStream.BeginRead(state.buffer, 0, TCPStateObject.BufferSize, new AsyncCallback(ReadCallback), state);
            }
            catch (Exception e)
            {
                TcpListener listener = (TcpListener)ar.AsyncState;
                Trace.TraceInformation("RLM connection failed {0} Exception {1}", listener.LocalEndpoint, e.ToString());
            }
        }

        private void ReadCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the handler socket
                // from the asynchronous state object.
                TCPStateObjectInsecure state = (TCPStateObjectInsecure)ar.AsyncState;
                NetworkStream handler = state.WorkStream;

                // Read data from the client socket. 
                int bytesRead = handler.EndRead(ar);

                if (bytesRead > 0)
                {
                    var receivedBuffer = state.buffer.Take(bytesRead);

                    Trace.TraceInformation("Message received from RLM {0}, data {1}", state.DeviceIpAddress, General.ByteArrayToHexString(receivedBuffer.ToArray()));

                    // Process message
                    RLMStatus RLMStatus;

                    // Check state.DeviceId multiple messages, if so separate and process individually
                    var messages = _RLMCommunication.SeperateMessages(state.DeviceIpAddress, receivedBuffer.ToArray());

                    foreach (var message in messages)
                    {
                        byte[] returnMessage = _RLMCommunication.ProcessMessage(state.DeviceIpAddress, message, out RLMStatus);

                        // Send Message if there is something to send back
                        if (returnMessage.Length > 0)
                        {
                            Trace.TraceInformation("Sending message to RLM {0}, data {1}", state.DeviceIpAddress, General.ByteArrayToHexString(returnMessage));
                            Send(state.DeviceIpAddress, handler, returnMessage);
                        }
                    }                   
                }

                // Check if still connected, Await for more data
                if (state.TcpClient.Connected)
                {
                    handler.BeginRead(state.buffer, 0, TCPStateObject.BufferSize, new AsyncCallback(ReadCallback), state);
                }
                else
                {
                    // kill connection
                    Trace.TraceError("ReadCallback - RLM {0} closed connection", state.DeviceIpAddress);
                    RemoveConnection(state.DeviceIpAddress);
                }
            }
            catch (Exception)
            {
                TCPStateObjectInsecure state = (TCPStateObjectInsecure)ar.AsyncState;
                Trace.TraceError("ReadCallback:Catch - RLM {0} closed connection", state.DeviceIpAddress);
                RemoveConnection(state.DeviceIpAddress);
            }
        }

        private void Send(string deviceIpAddress, NetworkStream handler, byte[] data)
        {
            try
            {
                // Begin sending the data to the remote device.
                handler.BeginWrite(data, 0, data.Length, new AsyncCallback(SendCallback), handler);
            }
            catch (Exception e)
            {
                Trace.TraceError("Send Error, Closing connection ", e);
                RemoveConnection(deviceIpAddress);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            // Retrieve the socket from the state object.
            NetworkStream handler = (NetworkStream)ar.AsyncState;

            // Complete sending the data to the remote device.
            handler.EndWrite(ar);
        }

        private void RemoveConnection(string deviceIpAddress)
        {
            // Try to find entry. If not available, then already removed from list.          
            TCPStateObjectInsecure tcpState;
            _tcpStateObjectList.TryGetValue(deviceIpAddress, out tcpState);

            if (tcpState != null)
            {
                // Remove from list, Send Close Connection (if possible) and close connection
                // If connection alive, generate and send close session message, otherwise just close
                if (tcpState.TcpClient.Connected)
                {
                    byte[] closeMessage = _RLMCommunication.GenerateCloseSession(deviceIpAddress);

                    // Synchronous Write
                    tcpState.WorkStream.Write(closeMessage, 0, closeMessage.Length);
                }
                tcpState.WorkStream.Close();
                tcpState.TcpClient.Close();

                _tcpStateObjectList.TryRemove(deviceIpAddress, out tcpState);
            }

            // Clean up list
            _RLMCommunication.RemoveRLMDeviceFromList(deviceIpAddress);
        }

        private void ProcessUserInteractionEvent(string deviceIpAddress, string message, string[] options)
        {
            TCPStateObjectInsecure tcpState;
            _tcpStateObjectList.TryGetValue(deviceIpAddress, out tcpState);

            if(tcpState != null)
            {
                byte[] returnMessage = _RLMCommunication.ProcessEvent(deviceIpAddress, message, options);

                // Send off Client
                // Send Message if there is something to send back
                if (returnMessage.Length > 0)
                {
                    Trace.TraceInformation("Sending message to RLM {0}, data {1}", tcpState.DeviceIpAddress, General.ByteArrayToHexString(returnMessage));
                    Send(deviceIpAddress, tcpState.WorkStream, returnMessage);
                }
            }
            else // Kill Connection if not active
            {
                RemoveConnection(deviceIpAddress);
            }
        }        
    }

    public class TCPStateObjectInsecure
    {
        // Device Id
        public string DeviceIpAddress = string.Empty;

        // TCP Client Object
        public TcpClient TcpClient;

        // Client  socket.
        public NetworkStream WorkStream = null;

        // Max size of payload is 1024 + 6 bytes of header
        public const int BufferSize = 2000;

        public const int MaxPayload = 1024;

        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];

        // Received data bytes
        public IEnumerable<byte> receivedBuffer = Enumerable.Empty<byte>();

        // Full message received, assume yes on first round
        public bool fullPayloadReceived = true;

        // First message
        public bool firstMessage = true;

        // Total Payload of message
        public int payloadLength = 0;
    }
}