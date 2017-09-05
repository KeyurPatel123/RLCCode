using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Abiomed.DotNetCore.Common
{
    public static class Utilities
    {
        /// <summary>
        /// Converts an Object to a Byte Array.
        /// </summary>
        /// <param name="objectToConvert">The Object to Convert to Byte Array</param>
        /// <returns>Byte Array from the Object.</returns>
        public static byte[] ObjectToByteArray(Object objectToConvert)
        {
            if (objectToConvert == null)
            {
                throw new ArgumentNullException("Abiomed.Common.Utilities - ObjectToByteArray(): objectToConvert is null.");
            }

            using (var memoryStream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(memoryStream, objectToConvert);
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Converts a Byte Array to an Object.
        /// </summary>
        /// <param name="itemToConvert">Array of bytes to convert to an Object</param>
        /// <returns>Object that was converted from the ByteArray.</returns>
        public static Object ByteArrayToObject(byte[] itemToConvert)
        {
            if (itemToConvert == null || itemToConvert.Length == 0)
            {
                if (itemToConvert == null)
                {
                    throw new ArgumentNullException("Abiomed.Common.Utilities - ByteArrayToObject(): itemToConvert is null.");
                }

                throw new ArgumentOutOfRangeException("Abiomed.Common.Utilities - ByteArrayToObject(): itemToConvert is Empty.");
            }

            using (var memoryStream = new MemoryStream())
            {
                memoryStream.Write(itemToConvert, 0, itemToConvert.Length);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return new BinaryFormatter().Deserialize(memoryStream);
            }
        }
    }
}
