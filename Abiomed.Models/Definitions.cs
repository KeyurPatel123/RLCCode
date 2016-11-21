using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abiomed.Models
{
    public static class Definitions
    {
        #region MsgTypes

        public static int MsgHeaderLength = 6;

        #region Receiving 
        public static UInt16 SessionRequest = 0x8000;
        public static UInt16 KeepAlive = 0x0004;
        public static UInt16 BufferStatusRequest = 0x8201;
        public static UInt16 OpenBearerRequest = 0x8001;
        public static UInt16 SessionCancel = 0x8005;

        #endregion

        #region Send
        public static byte[] SessionConfirm = new byte[] { 0x40, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x01};
        public static byte[] StreamViedoControlIndications = new byte[] {
            0x02, 0x00, // MSGID
            0x00, 0x00, // MsgLen
            0x00, 0x00, // MsgSeq
            0x12, 0x34, // UserRef
            0x00, 0x20, // FRate - 20FPS
            0x00, 0x00, // Res - Max
            0x00, 0x00, // Timeout - None
            0x00, 0x02, // Status Rate - Send every 2 seconds
            0x68, 0x74, 0x74, 0x70, 0x73, 0x3a, 0x2f, 0x2f, 0x77, 0x77, 0x77, 0x2e, 0x72, 0x65, 0x6d, 0x6f, 0x74, 0x65, 0x6c, 0x69, 0x6e, 0x6b, 0x2e, 0x61, 0x62, 0x69, 0x6f, 0x6d, 0x65, 0x64, 0x2e, 0x63, 0x6f, 0x6d, // URL - https://www.remotelink.abiomed.com
            0x61, 0x62, 0x69, 0x6f, 0x6d, 0x65, 0x64, 0x2d, 0x52, 0x4c, 0x4d, // USN - abiomed-RLM
            0x24, 0x74, 0x72, 0x33, 0x40, 0x6d, // PWD - $tr3@m
            0x6c, 0x69, 0x76, 0x65, // Stream Application - live
            0x52, 0x4c, 0x4d, 0x31, 0x32, 0x33, 0x34, 0x35 // Stream Name - RLM12345
        };
        public static byte[] SessionCloseIndicator = new byte[] { 0x00, 0x05, 0x00, 0x00, 0x00, 0x00};
        #endregion

        #endregion
        enum MsgId
        {
            Unknown =-1,
            Session = 0,
            RLM = 1,
            Digitiser = 2,
            FileTransfer = 3            
        };

        public enum Bearer
        {
            Unknown = -1,
            Ethernet = 0,
            Wifi24Ghz = 1,
            Wifi5Ghz = 2,
            LTE = 3
        };

        enum LEDColor
        {
            Unknown = -1,
            Off = 0,
            Blue = 1,
            Green = 2,
            Cyan = 3,
            Red = 4,
            Magenta = 5,
            Orange = 6,
            White = 7
        };

        enum BearerInformation
        {
            Unknown = -1,
            Success = 0,
            AuthenticationFail = 1,
            FailedConnectCSR = 2,
            NoDNS = 3,
            NoAuthenticationData = 4,
            NoConnectionAttemptedLasReboot = 5,
        };

        enum AuthenicationType
        {
            Unknown = -1,
            None = 0,
            W8021X = 1,
            WEP = 2,
            WPA = 3,
            WPAPSK = 4,
            WPAEAP = 5,
            PEAP = 6
        };
    }
}
