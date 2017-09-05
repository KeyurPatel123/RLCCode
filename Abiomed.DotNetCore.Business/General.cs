/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * General.cs: General Business Helper 
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using Abiomed.DotNetCore.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Abiomed.DotNetCore.Business
{
    /// <summary>
    /// General Business Logic Help Methods
    /// </summary>
    public class General
    {
        public static string ByteArrayToHexString(byte[] bytes)
        {
            StringBuilder hex = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                hex.AppendFormat("0x{0:X2} ", b);
            }
            return hex.ToString();
        }

        public static bool CompareDictionaries<TKey, TValue>(ConcurrentDictionary<TKey, TValue> dict1, ConcurrentDictionary<TKey, TValue> dict2)
        {
            IEqualityComparer<TValue> valueComparer = EqualityComparer<TValue>.Default;

            return dict1.Count == dict2.Count &&
                    dict1.Keys.All(key => dict2.ContainsKey(key) && valueComparer.Equals(dict1[key], dict2[key]));
        }

        public static byte[] GenerateRequest(byte[] msg, RLMDevice device)
        {
            // Update Payload
            var payload = BitConverter.GetBytes(msg.Length - Definitions.MsgHeaderLength);
            msg[2] = payload[1];
            msg[3] = payload[0];

            // Update sequence number
            if (device != null)
            {
                UInt16 serverSequence = device.ServerSequence++;
                byte[] serverSequenceBytes = BitConverter.GetBytes(serverSequence);
                msg[4] = serverSequenceBytes[1];
                msg[5] = serverSequenceBytes[0];
            }           
            return msg;
        }

        public static byte[] VideoControlGeneration(bool enable, string serialNumber, List<byte> streamVideoControlIndications)
        {
            // If disabled then set to 0, default is true
            if (enable == false)
            {
                streamVideoControlIndications[9] = 0x00;
            }

            // Convert SerialNumber to ASCII, add to list and convert out as byte[]
            var serialBytes = Encoding.ASCII.GetBytes(serialNumber);

            // Add length 
            List<byte> streamControl = new List<byte>(streamVideoControlIndications);
            streamControl.Add(Convert.ToByte(serialNumber.Length));
            streamControl.AddRange(serialBytes);
            return streamControl.ToArray();
        }
    }
}
