/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * Credentials.cs: Login Credentials Model
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using static Abiomed.Models.Definitions;

namespace Abiomed.Models
{
    public class WifiCredentials
    {
        public string SerialNumber = string.Empty;
        public int Slot = int.MaxValue;
        public AuthenicationType AuthType = AuthenicationType.Unknown;
        public string SSID = string.Empty;     
        public string PSK = string.Empty;      
    }
}
