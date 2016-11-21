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

namespace Abiomed.CSR.Communications
{
    public class TLSServer : ITLSServer
    {
        private readonly ILog _log;
        private IRLMCommunication _RLMCommunication;
        private RLMDeviceList _RLMDeviceList;
        private static readonly int ServerPort = 8433;

        private static readonly string ServerCertificateFile = "server.pfx";
        private static readonly string ServerCertificatePassword = null;
        private X509Certificate2 serverCertificate;
        private Dictionary<string, TCPStateObject> tcpStateObjectList = new Dictionary<string, TCPStateObject>();

        public TLSServer(ILog logger, IRLMCommunication RLMCommunication, RLMDeviceList RLMDeviceList)
        {
            _log = logger;
            _RLMCommunication = RLMCommunication;
            _RLMDeviceList = RLMDeviceList;

            _RLMCommunication.SendMessage += _RLMCommunication_SendMessage;
            serverCertificate = new X509Certificate2(ServerCertificateFile, ServerCertificatePassword);
        }

        private void _RLMCommunication_SendMessage(object sender, EventArgs e)
        {
            var ev = (CommunicationsEvent)e;

            // Get Handle
            var handle = tcpStateObjectList[ev.Identifier];

            // Send Cancel Message
            Send(handle.workStream, ev.Message);

            // Remove from list
            tcpStateObjectList.Remove(ev.Identifier);

            // Close Connection
            handle.workStream.Close();
        }

        public static ManualResetEvent allDone = new ManualResetEvent(false);
       
        public void Run()
        {
            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {                
                var listener = new TcpListener(IPAddress.Any, ServerPort);
                listener.Start();
                _log.Info("TLS Server Started Success");

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
                _log.Error("Error ", e);
            }

        }

        private void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            TcpListener listener = (TcpListener)ar.AsyncState;
            TcpClient handler = listener.EndAcceptTcpClient(ar);

            // todo add try catch with bad creds!?

            // Ensure RLM serial number is on approved list!

            // Connect to SSL Stream and Authenticate
            var sslStream = new SslStream(handler.GetStream(), false, App_CertificateValidation);
            sslStream.AuthenticateAsServer(serverCertificate, true, SslProtocols.Tls12, false);

            // Create the state object and add to list
            TCPStateObject state = new TCPStateObject();
            state.workStream = sslStream;            
            state.DeviceId = handler.Client.RemoteEndPoint.ToString();
            tcpStateObjectList[state.DeviceId] = state;

            _log.InfoFormat("RLM connected at connection {0}", state.DeviceId);
            sslStream.BeginRead(state.buffer, 0, TCPStateObject.BufferSize, new AsyncCallback(ReadCallback), state);
        }

        private void ReadCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the handler socket
                // from the asynchronous state object.
                TCPStateObject state = (TCPStateObject)ar.AsyncState;
                SslStream handler = state.workStream;

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
            catch(Exception e)
            {
                TCPStateObject state = (TCPStateObject)ar.AsyncState;
                state.workStream.Close();
                _log.ErrorFormat("Read error from . \n Data", state.DeviceId);

            }
        }

        private void Send(SslStream handler, byte[] data)
        {
            // Begin sending the data to the remote device.
            handler.BeginWrite(data, 0, data.Length, new AsyncCallback(SendCallback), handler);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                SslStream handler = (SslStream)ar.AsyncState;

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