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

        public static UInt16 SuccessStats = 0x8000;
        public static UInt16 UserRef = 0x1234;

        #region Receiving 
        public static UInt16 SessionRequest = 0x8000;
        public static UInt16 KeepAlive = 0x8004;
        public static UInt16 BufferStatusRequest = 0x8201;
        public static UInt16 OpenBearerRequest = 0x8001;
        public static UInt16 SessionCancel = 0x8005;
        public static UInt16 StreamVideoResponse = 0xC200;
        public static UInt16 ScreenCaptureResponse = 0xC202;
        public static UInt16 FileOpenResponse = 0xC300;
        public static UInt16 FileReadResponse = 0xC301;

        #endregion

        #region Send
        public static byte[] SessionConfirm = new byte[] { 0x40, 0x00, 0x00, 0x02, 0x00, 0x00, 0x80, 0x00};
        public static byte[] SessionCloseIndicator = new byte[] { 0x00, 0x05, 0x00, 0x00, 0x00, 0x00};
        
        #region Video
        public static List<byte> StreamVideoBase = new List<byte> 
        {
            0x02, 0x00, // MSGID
            0x00, 0x00, // MsgLen
            0x00, 0x00, // MsgSeq
            0x12, 0x34, // UserRef
            0x00, 0x01, // Enable            
            0x00, 0x00, // Timeout - None
            0x00, 0x02, // Status Rate - Send every 2 seconds
            0x10, 0x31, 0x33, 0x2e, 0x39, 0x32, 0x2e, 0x32, 0x35, 0x35, 0x2e, 0x33, 0x38, 0x3a, 0x34, 0x34, 0x33, // URL - 13.92.255.38:443
            0x0B, 0x61, 0x62, 0x69, 0x6f, 0x6d, 0x65, 0x64, 0x2d, 0x52, 0x4c, 0x4d, // USN - abiomed-RLM
            0x06, 0x24, 0x74, 0x72, 0x33, 0x40, 0x6d, // PWD - $tr3@m
            0x04, 0x6c, 0x69, 0x76, 0x65, // Stream Application - live
        };

        #endregion

        #region File
        // Screen 3
        public static byte[] ScreenCaptureIndicator = new byte[] { 0x02, 0x02, 0x00, 0x04, 0x00, 0x00, 0x03 };

        public static byte[] FileOpenIndicator = new byte[] {
          0x03, 0x00,// MSG ID
          0x00, 0x00, // MSG LEN
          0x00, 0x00, // SEQ 
          0x0D, 0xFC, // User Ref 3580
          // /rlm/capture/1.png
          0x12, 0x2F, 0x72, 0x6C, 0x6D, 0x2F, 0x63, 0x61, 0x70, 0x74, 0x75, 0x72, 0x65, 0x2F, 0x31, 0x2E, 0x70, 0x6E, 0x67          
        };

        public static byte[] FileReadIndicator = new byte[]
        {
            0x83, 0x01, //MSG ID
            0x00, 0x04, //MSG LEN
            0x00, 0x00, //MSG SEQ
            0x0D, 0xFD, //User Ref
            0x00, 0x00  //Block ID
        };

        public static byte[] FileCloseIndicator = new byte[]
        {
            0x03, 0x02,
            0x00, 0x00,
            0x00, 0x00
        };        
        #endregion

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
