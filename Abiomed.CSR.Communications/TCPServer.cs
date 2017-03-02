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
using log4net;
using Abiomed.Business;
using System.Linq;
using System.Collections.Concurrent;
using Abiomed.Models;

namespace Abiomed.RLR.Communications
{
    public class TCPServer : ITCPServer
    {
        private readonly ILog _log;
        private IRLMCommunication _RLMCommunication;
        private RLMDeviceList _RLMDeviceList;
        private static readonly int ServerPort = 443;

        private ConcurrentDictionary<string, TCPStateObject> tcpStateObjectList = new ConcurrentDictionary<string, TCPStateObject>();

        public TCPServer(ILog logger, IRLMCommunication RLMCommunication, RLMDeviceList RLMDeviceList)
        {
            _log = logger;
            _RLMCommunication = RLMCommunication;
            _RLMDeviceList = RLMDeviceList;
            
            _RLMCommunication.SendMessage += _RLMCommunication_SendMessage;
         }

        private void _RLMCommunication_SendMessage(object sender, EventArgs e)
        {
            try
            {
                var ev = (CommunicationsEvent)e;

                TCPStateObject handle;
                tcpStateObjectList.TryGetValue(ev.Identifier, out handle);

                if (handle != null)
                {
                    // Send Cancel Message - Non-Asynch
                    handle.workStream.Write(ev.Message, 0, ev.Message.Length);

                    // Remove from list
                    TCPStateObject tcpState;
                    tcpStateObjectList.TryRemove(ev.Identifier, out tcpState);

                    // Close Connection
                    handle.workStream.Close();

                    _log.InfoFormat("TCP Send: Closing RLM Connection {0}", handle.DeviceId);
                }
                else
                {
                    _log.Info("TCP Send: RLM Connection already closed");
                }
            }
            catch(Exception ex)
            {
                _log.Error("TCP Send: RLM Connection already closed", ex);
            }
        }

        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public void Run()
        {
            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                var listener = new TcpListener(IPAddress.Any, ServerPort);
                listener.Start();
                _log.Info("TCP Server Started Success");

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
                
                _log.Error("Cannot Start TCP Service Error ", e);
            }

        }

        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                // Signal the main thread to continue.
                allDone.Set();

                // Get the socket that handles the client request.
                TcpListener listener = (TcpListener)ar.AsyncState;
                TcpClient handler = listener.EndAcceptTcpClient(ar);
           
                // todo add try catch with bad creds!?

                // Ensure RLM serial number is on approved list!

                // Connect to SSL Stream and Authenticate
                //var sslStream = new SslStream(handler.GetStream(), false, App_CertificateValidation);
                //sslStream.AuthenticateAsServer(serverCertificate, true, SslProtocols.Tls12, false);
                

                // Create the state object and add to list
                TCPStateObject state = new TCPStateObject();
                state.TcpClient = handler;
                state.workStream = handler.GetStream();
                state.DeviceId = handler.Client.RemoteEndPoint.ToString();
                tcpStateObjectList.TryAdd(state.DeviceId, state);

                _log.InfoFormat("RLM connected at connection {0}", state.DeviceId);
                state.workStream.BeginRead(state.buffer, 0, TCPStateObject.BufferSize, new AsyncCallback(ReadCallback), state);
            }
            catch(Exception e)
            {
                TcpListener listener = (TcpListener)ar.AsyncState;
                _log.InfoFormat("RLM connection failed {0} Exception {1}", listener.LocalEndpoint, e.ToString());
            }
        }

        private void ReadCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the handler socket
                // from the asynchronous state object.
                TCPStateObject state = (TCPStateObject)ar.AsyncState;
                NetworkStream handler = state.workStream;

                // Read data from the client socket. 
                int bytesRead = handler.EndRead(ar);

                if (bytesRead > 0)
                {
                    var receivedBuffer = state.buffer.Take(bytesRead);
                    
                    _log.InfoFormat("Message received from RLM {0}, data {1}", state.DeviceId, General.ByteArrayToHexString(receivedBuffer.ToArray()));

                    // Process message
                    RLMStatus RLMStatus;

                    // Check if multiple messages, if so separate and process individually
                    //var messages = _RLMCommunication.SeperateMessages(receivedBuffer.ToArray());

                    byte[] returnMessage = _RLMCommunication.ProcessMessage(state.DeviceId, receivedBuffer.ToArray(), out RLMStatus);

                    // Send Message if there is something to send back
                    if (returnMessage.Length > 0)
                    {
                        _log.InfoFormat("Sending message to RLM {0}, data {1}", state.DeviceId, General.ByteArrayToHexString(returnMessage));

                        //need to test if (state.TcpClient.Connected)
                        {
                            Send(handler, returnMessage);
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
                }
            }
            catch (Exception)
            {
                TCPStateObject state = (TCPStateObject)ar.AsyncState;
                state.workStream.Close();

                // Remove from list
                TCPStateObject tcpState;
                tcpStateObjectList.TryRemove(state.DeviceId, out tcpState);

                RLMDevice device;
                _RLMDeviceList.RLMDevices.TryRemove(state.DeviceId ,out device);
                _RLMCommunication.UpdateSubscribedServers();

                _log.InfoFormat("ReadCallback - RLM {0} closed connection", state.DeviceId);                
            }
        }
        
        private void Send(NetworkStream handler, byte[] data)
        {
            // Begin sending the data to the remote device.
            handler.BeginWrite(data, 0, data.Length, new AsyncCallback(SendCallback), handler);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                NetworkStream handler = (NetworkStream)ar.AsyncState;

                // Complete sending the data to the remote device.
                handler.EndWrite(ar);
            }
            catch (Exception e)
            {
                _log.Error("Error ", e);
            }
        }
    }
}