using System;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Security.Authentication;
using log4net;
using Abiomed.Business;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Abiomed.Models;
using System.Collections.Concurrent;

namespace Abiomed.CSR.Communications
{
    public class TLSServer : ITLSServer
    {
        private readonly ILog _log;
        private IRLMCommunication _RLMCommunication;
        private RLMDeviceList _RLMDeviceList;
        private static readonly int ServerPort = 443;

        private ConcurrentDictionary<string, TCPStateObject> tcpStateObjectList = new ConcurrentDictionary<string, TCPStateObject>();

        public TLSServer(ILog logger, IRLMCommunication RLMCommunication, RLMDeviceList RLMDeviceList)
        {
            _log = logger;
            _RLMCommunication = RLMCommunication;
            _RLMDeviceList = RLMDeviceList;
            
            _RLMCommunication.SendMessage += _RLMCommunication_SendMessage;
            //serverCertificate = new X509Certificate2(ServerCertificateFile, ServerCertificatePassword);
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

                    _log.InfoFormat("Error: Closing RLM Connection {0}", handle.DeviceId);
                }
                else
                {
                    _log.Info("Error: RLM Connection already closed");
                }
            }
            catch(Exception ex)
            {
                _log.Error("Error: RLM Connection already closed", ex);
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
                state.workStream = handler.GetStream();
                state.DeviceId = handler.Client.RemoteEndPoint.ToString();
                tcpStateObjectList.TryAdd(state.DeviceId, state);

                _log.InfoFormat("RLM connected at connection {0}", state.DeviceId);
                state.workStream.BeginRead(state.buffer, 0, TCPStateObject.BufferSize, new AsyncCallback(ReadCallback), state);
            }
            catch(Exception e)
            {
                TcpListener listener = (TcpListener)ar.AsyncState;
                _log.InfoFormat("RLM connection failed {0}", listener.LocalEndpoint);
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
                    byte[] returnMessage = _RLMCommunication.ProcessMessage(state.DeviceId, receivedBuffer.ToArray(), out RLMStatus);

                    // Send Message if there is something to send back
                    if (returnMessage.Length > 0)
                    {
                        _log.InfoFormat("Sending message to RLM {0}, data {1}", state.DeviceId, General.ByteArrayToHexString(returnMessage));
                        Send(handler, returnMessage);
                    }
                }

                // Await for more data
                handler.BeginRead(state.buffer, 0, TCPStateObject.BufferSize, new AsyncCallback(ReadCallback), state);
            }
            catch (Exception e)
            {
                TCPStateObject state = (TCPStateObject)ar.AsyncState;
                state.workStream.Close();
                _log.ErrorFormat("Read error from RLM {0}", state.DeviceId);
            }
        }

        /// <summary>
        /// Need to simplify, when getting files. Too Complex right now.
        /// </summary>
        /// <param name="ar"></param>
        private void ReadCallbackOld(IAsyncResult ar)
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
                    // Add bytes to received buffer tod0!
                    //state.receivedBuffer = state.buffer.Concat(state.receivedBuffer);

                    // Determine if first message, if so get payload amount and determine if more data is coming
                    if (state.firstMessage)
                    {
                        // bytesRead todo fix for appending!
                        state.receivedBuffer = state.buffer.Take(bytesRead);

                        if (BitConverter.IsLittleEndian)
                        {
                            var payloadBytes = state.buffer.Skip(2).Take(2).Reverse().ToArray();
                            state.payloadLength = BitConverter.ToUInt16(payloadBytes, 0);
                        }
                        else
                        {
                            state.payloadLength = BitConverter.ToUInt16(state.buffer, 2);
                        }

                        // Check if we have full data
                        if (state.payloadLength == TCPStateObject.MaxPayload)
                        {
                            // Need to get more data
                            state.fullPayloadReceived = false;
                        }

                        state.firstMessage = false;
                    }

                    if (state.fullPayloadReceived)
                    {
                        // Clear buffer
                        //state.receivedBuffer;

                        _log.InfoFormat("Message received from RLM {0}, data {1}", state.DeviceId, General.ByteArrayToHexString(state.receivedBuffer.ToArray()));

                        // Process message
                        RLMStatus RLMStatus;
                        byte[] returnMessage = _RLMCommunication.ProcessMessage(state.DeviceId, state.receivedBuffer.ToArray(), out RLMStatus);

                        // Send Message if there is something to send back
                        if (returnMessage.Length > 0)
                        {
                            _log.InfoFormat("Sending message to RLM {0}, data {1}", state.DeviceId, General.ByteArrayToHexString(returnMessage));
                            Send(handler, returnMessage);
                        }

                        // Reset
                        state.firstMessage = true;
                    }
                    else
                    {
                        // Not all data received. Get more.
                        handler.BeginRead(state.buffer, 0, TCPStateObject.BufferSize, new AsyncCallback(ReadCallback), state);
                    }
                }

                // Await for more data
                handler.BeginRead(state.buffer, 0, TCPStateObject.BufferSize, new AsyncCallback(ReadCallback), state);
            }
            catch (Exception e)
            {
                TCPStateObject state = (TCPStateObject)ar.AsyncState;
                state.workStream.Close();
                _log.ErrorFormat("Read error from . \n Data", state.DeviceId);

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

        private bool App_CertificateValidation(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None) { return true; }
            if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors) { return true; } //we don't have a proper certificate tree
            _log.Error("*** SSL Error: " + sslPolicyErrors.ToString());
            return false;
        }


    }
}