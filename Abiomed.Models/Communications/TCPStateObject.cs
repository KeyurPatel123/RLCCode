using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace Abiomed.Models
{
    public class TCPStateObject
    {
        // Device Id
        public string DeviceId = string.Empty;

        // Client  socket.
        public SslStream workStream = null;

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
