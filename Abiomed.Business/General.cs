/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * General.cs: General Business Helper 
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Abiomed.Business
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
    }
}
