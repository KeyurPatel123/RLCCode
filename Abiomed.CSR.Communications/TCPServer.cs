﻿/*
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
    public class TCPServer : ITCPServer
    {       
        private IRLMCommunication _RLMCommunication;
        private Configuration _configuration;
        private X509Certificate2 serverCertificate = null;
        private ConcurrentDictionary<string, TCPStateObject> _tcpStateObjectList = new ConcurrentDictionary<string, TCPStateObject>();
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
            Definitions.CloseSessionIndicationEvent,
            Definitions.VideoStopEvent,
            Definitions.ImageStopEvent,
        };

        public TCPServer(IRLMCommunication RLMCommunication, Configuration configuration, IRedisDbRepository<RLMDevice> redisDbRepository)
        {
            _RLMCommunication = RLMCommunication;
            _configuration = configuration;
            _redisDbRepository = redisDbRepository;
            CreateCertificate();

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

        private void CreateCertificate()
        {
            serverCertificate = new X509Certificate2(_configuration.CertLocation, "Abiomed123");
        }

        public void Run()
        {
            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                var listener = new TcpListener(IPAddress.Any, _configuration.TcpPort);
                listener.Start();
                Trace.TraceInformation("TCP Server Started Success");
                

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

                // Connect to SSL Stream and Authenticate
                var sslStream = new SslStream(handler.GetStream(), false);
                sslStream.AuthenticateAsServer(serverCertificate, false, SslProtocols.Tls12, false);

                // Display the properties and settings for the authenticated stream.
                //DisplaySecurityLevel(sslStream);
                //DisplaySecurityServices(sslStream);
                //DisplayCertificateInformation(sslStream);
                //DisplayStreamProperties(sslStream);                

                // Create the state object and add to list
                TCPStateObject state = new TCPStateObject();
                state.TcpClient = handler;
                state.WorkStream = sslStream;
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
                TCPStateObject state = (TCPStateObject)ar.AsyncState;
                SslStream handler = state.WorkStream;

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
                TCPStateObject state = (TCPStateObject)ar.AsyncState;
                Trace.TraceError("ReadCallback:Catch - RLM {0} closed connection", state.DeviceIpAddress);
                RemoveConnection(state.DeviceIpAddress);
            }
        }

        private void Send(string deviceIpAddress, SslStream handler, byte[] data)
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
            SslStream handler = (SslStream)ar.AsyncState;

            // Complete sending the data to the remote device.
            handler.EndWrite(ar);
        }

        private void RemoveConnection(string deviceIpAddress)
        {
            // Try to find entry. If not available, then already removed from list.          
            TCPStateObject tcpState;
            _tcpStateObjectList.TryGetValue(deviceIpAddress, out tcpState);

            if (tcpState != null)
            {
                // Remove from list, Send Close Connection (if possible) and close connection
                // If connection alive, generate and send close session message, otherwise just close
                if (tcpState.TcpClient.Connected)
                {
                    byte[] closeMessage = _RLMCommunication.GenerateCloseSession(deviceIpAddress);

                    // Synchronous Write
                    tcpState.WorkStream.Write(closeMessage);
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
            TCPStateObject tcpState;
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

        #region Security Cert Info
        private static void DisplaySecurityLevel(SslStream stream)
        {
            Console.WriteLine("Cipher: {0} strength {1}", stream.CipherAlgorithm, stream.CipherStrength);
            Console.WriteLine("Hash: {0} strength {1}", stream.HashAlgorithm, stream.HashStrength);
            Console.WriteLine("Key exchange: {0} strength {1}", stream.KeyExchangeAlgorithm, stream.KeyExchangeStrength);
            Console.WriteLine("Protocol: {0}", stream.SslProtocol);
        }
        private static void DisplaySecurityServices(SslStream stream)
        {
            Console.WriteLine("Is authenticated: {0} as server? {1}", stream.IsAuthenticated, stream.IsServer);
            Console.WriteLine("IsSigned: {0}", stream.IsSigned);
            Console.WriteLine("Is Encrypted: {0}", stream.IsEncrypted);
        }
        private static void DisplayStreamProperties(SslStream stream)
        {
            Console.WriteLine("Can read: {0}, write {1}", stream.CanRead, stream.CanWrite);
            Console.WriteLine("Can timeout: {0}", stream.CanTimeout);
        }
        private static void DisplayCertificateInformation(SslStream stream)
        {
            Console.WriteLine("Certificate revocation list checked: {0}", stream.CheckCertRevocationStatus);

            X509Certificate localCertificate = stream.LocalCertificate;
            if (stream.LocalCertificate != null)
            {
                Console.WriteLine("Local cert was issued to {0} and is valid from {1} until {2}.",
                    localCertificate.Subject,
                    localCertificate.GetEffectiveDateString(),
                    localCertificate.GetExpirationDateString());
            }
            else
            {
                Console.WriteLine("Local certificate is null.");
            }
            // Display the properties of the client's certificate.
            X509Certificate remoteCertificate = stream.RemoteCertificate;
            if (stream.RemoteCertificate != null)
            {
                Console.WriteLine("Remote cert was issued to {0} and is valid from {1} until {2}.",
                    remoteCertificate.Subject,
                    remoteCertificate.GetEffectiveDateString(),
                    remoteCertificate.GetExpirationDateString());
            }
            else
            {
                Console.WriteLine("Remote certificate is null.");
            }
        }
        #endregion


    }
}