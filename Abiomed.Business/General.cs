using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
