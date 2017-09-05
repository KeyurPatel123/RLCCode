/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * TCPStateObject.cs: TCP State Object Model
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;

namespace Abiomed.DotNetCore.Models
{
    public class TCPStateObject
    {
        // Device Id
        public string DeviceIpAddress = string.Empty;

        // TCP Client Object
        public TcpClient TcpClient;
        
        // Client  socket.
        public SslStream WorkStream = null;

        // Max size of payload is 1024 + 6 bytes of header
        public const int BufferSize = 1030;

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
