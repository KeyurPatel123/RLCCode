using System;
using System.Collections.Generic;

namespace Abiomed.DotNetCore.Models
{
    public static class Definitions
    {
        public static string RemoveRLMDeviceRLR = @"RemoveRLMDeviceRLR";
        public static string ImageCapture = @"ImageCapture";
        public static string RLMDeviceSet = @"RLMDeviceSet";
        public static string RLMDeviceSetWOWZA = @"RLMDeviceSetWOWZA";
        public static string UpdatedRLMDevices = @"UpdatedRLMDevices";

        public static string AddRLMDevice = @"AddRLMDevice";
        public static string UpdateRLMDevice = @"UpdateRLMDevice";
        public static string DeleteRLMDevice = @"DeleteRLMDevice";
        public static string BearerInfoRLMDevice = @"BearerInfoRLMDevice";

        public static int MaxBearerSlot = 20;

        #region RLR to RLM Event
        public static string KeepAliveIndicationEvent = @"KeepAliveIndication";
        public static string BearerChangeIndicationEvent = @"BearerChangeIndication";
        public static string StatusIndicationEvent = @"StatusIndication";
        public static string BearerAuthenticationReadIndicationEvent = @"BearerAuthenticationReadIndication";
        public static string BearerAuthenticationUpdateIndicationEvent = @"BearerAuthenticationUpdateIndication";
        public static string BearerDeleteEvent = @"BearerDeleteEvent";
        public static string BearerPriorityIndicationEvent = @"BearerPriorityIndicationEvent";
        public static string StreamingVideoControlIndicationEvent = @"StreamingVideoControlIndication";
        public static string ScreenCaptureIndicationEvent = @"ScreenCaptureIndication";
        public static string VideoStopEvent = @"VideoStop";
        public static string ImageStopEvent = @"ImageStop";
        public static string OpenRLMLogFileIndicationEvent = @"OpenFileIndication";
        public static string CloseSessionIndicationEvent = @"CloseSessionIndication";
        #endregion

        public enum RLMFileTransfer
        {
            ScreenCapture0 = 0,
            ScreenCapture1 = 1,
            ScreenCapture2 = 2,
            ScreenCapture3 = 3,
            ScreenCapture4 = 4,
            ScreenCapture5 = 5,
            ScreenCapture6 = 6,
            ScreenCapture7 = 7,
            RLMEventLog = 8
        }
        
        #region MsgTypes

        public static int MsgHeaderLength = 6;

        public static UInt16 SuccessStats = 0x8000;
        public static UInt16 UserRef = 0x1234;
        public static UInt16 UserRefFileTransfer = 0x0DFD;

        #region Receiving

        #region Session
        public static UInt16 SessionRequest = 0x8000;
        public static UInt16 BearerRequest = 0x8001;
        public static UInt16 KeepAliveRequest = 0x8004;
        public static UInt16 SessionCloseRequest = 0x8003;
        public static UInt16 SessionCloseResponse = 0xC002;
        #endregion

        #region Status Control
        public static UInt16 StatusResponse = 0xC100;
        public static UInt16 BearerAuthenticationUpdateResponse = 0xC101;
        public static UInt16 BearerAuthenticationReadResponse = 0xC102;
        public static UInt16 BearerPriorityConfirm = 0x4105;
        public static UInt16 LimitWarningRequest = 0x8103;
        public static UInt16 LimitCriticalRequest = 0x8104;        
        #endregion

        #region Digitiser
        public static UInt16 StreamVideoControlResponse = 0xC200;
        public static UInt16 BufferStatusRequest = 0x8201;
        public static UInt16 ScreenCaptureResponse = 0xC202;
        #endregion

        #region File Transfer
        public static UInt16 OpenFileRequest = 0x8300;
        public static UInt16 OpenFileResponse = 0xC300;
        public static UInt16 DataReadRequest = 0x8301;
        public static UInt16 DataReadResponse = 0x4301;
        public static UInt16 CloseFileRequest = 0x8302;
        public static UInt16 ClearFileResponse = 0xC303;
        #endregion

        #endregion

        #region Send

        #region Session
        public static byte[] SessionConfirm = new byte[] { 0x40, 0x00, 0x00, 0x02, 0x00, 0x00, 0x80, 0x00 };
        public static byte[] BearerConfirm = new byte[] { 0x40, 0x01, 0x00, 0x02, 0x00, 0x00, 0x80, 0x00 };
        public static byte[] BearerChangeIndication = new byte[] { 0x00, 0x02, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x80, 0x00 };
        public static byte[] SessionCloseConfirm = new byte[] { 0x40, 0x03, 0x00, 0x00, 0x00, 0x00, 0x80, 0x00};
        public static byte[] KeepAliveIndication = new byte[] { 0x00, 0x04, 0x00, 0x00, 0x00, 0x00 };
        public static byte[] CloseSessionIndication = new byte[] { 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x20};
        #endregion

        #region Status Control
        public static byte[] StatusIndication = new byte[] { 0x01, 0x02, 0x00, 0x00, 0x00, 0x00};
        public static List<byte> BearerAuthenticationUpdateIndication = new List<byte>
        {
            0x01, 0x01, // MSGID
            0x00, 0x00, // MsgLen
            0x00, 0x00, // MsgSeq
            0x12, 0x34, // UserRef
            0x00, 0x01, // Bearer            
            0x00, 0x00, // Slot
            0x00, 0x00, // AuthInfo
            // SSID and PSK will be added manually            
        };

        public static byte[] EmptySSIDPSK = new byte[]
        {
            0x00, 0x00
        };

        public static byte[] BearerAuthenticationReadIndication = new byte[]
        {
            0x01, 0x02, // MSGID
            0x00, 0x00, // MsgLen
            0x00, 0x00, // MsgSeq
            0x12, 0x34, // UserRef
            0x00, 0x00, // Slot
        };

        public static byte[] BearerPriorityIndication = new byte[] 
        {
            0x81, 0x05, // MSGID
            0x00, 0x08, // MsgLen
            0x00, 0x00, // MsgSeq
            0x00, 0xFF, // Ethernet
            0x00, 0xFF, // Wifi
            0x00, 0xFF, // Cellular
        };

        public static byte[] LimitWarningConfirm = new byte[]  { 0x41, 0x03, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00 };
        public static byte[] LimitCriticalConfirm = new byte[] { 0x41, 0x04, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00 };

        #endregion

        #region Digitiser
        public static List<byte> StreamVideoControlIndication = new List<byte>
        {
            0x02, 0x00, // MSGID
            0x00, 0x00, // MsgLen
            0x00, 0x00, // MsgSeq
            0x12, 0x34, // UserRef
            0x00, 0x01, // Enable            
            0x00, 0x00, // Timeout - None
            0x00, 0x02, // Status Rate - Send every 2 seconds
            //0x16, 0x72, 0x74, 0x6d, 0x70, 0x3a, 0x2f, 0x2f, 0x72, 0x6c, 0x76, 0x2e, 0x61, 0x62, 0x69, 0x6f, 0x6d, 0x65, 0x64, 0x2e, 0x63, 0x6f, 0x6d, // URL rtmp://rlv.abiomed.com
            0x0B, 0x61, 0x62, 0x69, 0x6f, 0x6d, 0x65, 0x64, 0x2d, 0x52, 0x4c, 0x4d, // USN - abiomed-RLM
            0x06, 0x53, 0x74, 0x72, 0x65, 0x61, 0x6d, // PWD - Stream                  
            0x04, 0x6c, 0x69, 0x76, 0x65, // Stream Application - live
        };
        /*
        public static List<byte> StreamVideoControlIndication = new List<byte>
        {
            0x02, 0x00, // MSGID
            0x00, 0x00, // MsgLen
            0x00, 0x00, // MsgSeq
            0x12, 0x34, // UserRef
            0x00, 0x01, // Enable            
            0x00, 0x00, // Timeout - None
            0x00, 0x02, // Status Rate - Send every 2 seconds
            //0x16, 0x72, 0x74, 0x6d, 0x70, 0x3a, 0x2f, 0x2f, 0x72, 0x6c, 0x76, 0x2e, 0x61, 0x62, 0x69, 0x6f, 0x6d, 0x65, 0x64, 0x2e, 0x63, 0x6f, 0x6d, // URL rtmp://rlv.abiomed.com
            0x0B, 0x61, 0x62, 0x69, 0x6f, 0x6d, 0x65, 0x64, 0x2d, 0x52, 0x4c, 0x4d, // USN - abiomed-RLM
            0x06, 0x53, 0x74, 0x72, 0x65, 0x61, 0x6d, // PWD - Stream                  
            0x04, 0x6c, 0x69, 0x76, 0x65, // Stream Application - live
        };*/

        public static List<byte> StreamVideoControlIndicationRTMP = new List<byte>
        {
            0x02, 0x00, // MSGID
            0x00, 0x00, // MsgLen
            0x00, 0x00, // MsgSeq
            0x12, 0x34, // UserRef
            0x00, 0x01, // Enable            
            0x00, 0x00, // Timeout - None
            0x00, 0x02, // Status Rate - Send every 2 seconds
            0x16, 0x72, 0x74, 0x6d, 0x70, 0x3a, 0x2f, 0x2f, 0x72, 0x6c, 0x76, 0x2e, 0x61, 0x62, 0x69, 0x6f, 0x6d, 0x65, 0x64, 0x2e, 0x63, 0x6f, 0x6d, // URL rtmp://rlv.abiomed.com
            //0x10, 0x72, 0x74, 0x6d, 0x70, 0x3a, 0x2f, 0x2f, 0x6c, 0x6f, 0x63, 0x61, 0x6c, 0x68, 0x6f, 0x73, 0x74,
            0x0B, 0x61, 0x62, 0x69, 0x6f, 0x6d, 0x65, 0x64, 0x2d, 0x52, 0x4c, 0x4d, // USN - abiomed-RLM
            0x06, 0x53, 0x74, 0x72, 0x65, 0x61, 0x6d, // PWD - Stream                  
            0x04, 0x6c, 0x69, 0x76, 0x65, // Stream Application - live
        };
      
        public static List<byte> StreamVideoControlIndicationRTMPS = new List<byte>
        {
            0x02, 0x00, // MSGID
            0x00, 0x00, // MsgLen
            0x00, 0x00, // MsgSeq
            0x12, 0x34, // UserRef
            0x00, 0x01, // Enable            
            0x00, 0x00, // Timeout - None
            0x00, 0x02, // Status Rate - Send every 2 seconds
            0x17, 0x72, 0x74, 0x6d, 0x70, 0x73, 0x3a, 0x2f, 0x2f, 0x72, 0x6c, 0x76, 0x2e, 0x61, 0x62, 0x69, 0x6f, 0x6d, 0x65, 0x64, 0x2e, 0x63, 0x6f, 0x6d, // URL rtmps://rlv.abiomed.com
            0x0B, 0x61, 0x62, 0x69, 0x6f, 0x6d, 0x65, 0x64, 0x2d, 0x52, 0x4c, 0x4d, // USN - abiomed-RLM
            0x06, 0x53, 0x74, 0x72, 0x65, 0x61, 0x6d, // PWD - Stream                  
            0x04, 0x6c, 0x69, 0x76, 0x65, // Stream Application - live
        };

        public static byte[] VideoStopIndicator = new byte[]
        {
            0x02, 0x00, // MSGID
            0x00, 0x00, // MsgLen
            0x00, 0x00, // MsgSeq
            0x12, 0x34, // UserRef
            0x00, 0x00, // Enable            
            0x00, 0x00, // Timeout - None
            0x00, 0x02, // Status Rate - Send every 2 seconds
            0x00, 0x00, // URL
            0x00, 0x00, // USN 
            0x00, 0x00, // PWD 
            0x00, 0x00, // Stream Application
        };

        public static byte[] ScreenCaptureIndicator = new byte[] 
        {
            0x02, 0x02, // MsgId
            0x00, 0x04, // MsgLen
            0x00, 0x00, // MsgSeq
            0x12, 0x34, // User Ref
            0x00, 0x00 // Screen 0
        };

        public static byte[] RLMLogCaptureIndicator = new byte[]
        {
            0x02, 0x02, // MsgId
            0x00, 0x04, // MsgLen
            0x00, 0x00, // MsgSeq
            0x12, 0x34, // User Ref
            0x00, 0x08  // Log
        };

        #endregion

        #region File Transfer
        public static byte[] OpenFileConfirm = new byte[]
        {
            0x43, 0x00, //MSG ID
            0x00, 0x10, //MSG LEN
            0x00, 0x00, //MSG SEQ
            0x80, 0x00, //Status
            0x0D, 0xFD, //User Ref
            0x00, 0x00, 0x00, 0x00, //Size
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 // Time
        };

        public static byte[] OpenScreenFileIndication = new byte[] {
          0x03, 0x00,// MSG ID
          0x00, 0x00, // MSG LEN
          0x00, 0x00, // SEQ 
          0x12, 0x34, // User Ref 1234          
          0x00, 0x00 // File ID
        };

        public static byte[] OpenRLMLogFileIndication = new byte[] {
          0x03, 0x00,// MSG ID
          0x00, 0x00, // MSG LEN
          0x00, 0x00, // SEQ 
          0x12, 0x34, // User Ref 1234          
          0x00, 0x08 // File ID
        };

        public static List<byte> DataReadConfirm = new List<byte>
       {
            0x83, 0x01, //MSG ID
            0x00, 0x04, //MSG LEN
            0x00, 0x00, //MSG SEQ
            0x80, 0x00,  //Status
            0x0D, 0xFD, //User Ref
            0x00, 0x00  //Data
       };

        public static byte[] DataReadIndication = new byte[]
        {
            0x83, 0x01, //MSG ID
            0x00, 0x04, //MSG LEN
            0x00, 0x00, //MSG SEQ
            0x0D, 0xFD, //User Ref
            0x00, 0x00, 0x00, 0x00  //Block ID
        };

        public static byte[] CloseFileIndication = new byte[]
        {
            0x03, 0x02,//MSG ID
            0x00, 0x00,//MSG LEN
            0x00, 0x00 //MSG SEQ
        };

        public static byte[] ClearScreenFileIndication = new byte[]
        {
            0x03, 0x03,//MSG ID
            0x00, 0x00,//MSG LEN
            0x00, 0x00, //MSG SEQ
            0x12, 0x34, // User Ref
            0x00, 0x00 // Name String
        };

        public static byte[] ClearRLMLogFileIndication = new byte[]
        {
            0x03, 0x03,//MSG ID
            0x00, 0x00,//MSG LEN
            0x00, 0x00, //MSG SEQ
            0x12, 0x34, // User Ref
            0x00, 0x08 // Log
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

        public enum Bearer : int 
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

        [Serializable]
        public enum AuthenicationType
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
        
        [Serializable]
        public enum LogMessageType
        {
            Unknown = -1,
            #region Session
            SessionRequest = 0,
            SessionConfirm = 1,
            BearerRequest = 2,
            BearerConfirm = 3,
            BearerChangeIndication = 4,
            SessionCloseResponse = 5,
            CloseBearerRequest = 6,
            CloseBearerConfirm = 7,
            KeepAliveRequest = 8,
            KeepAliveIndication = 9,
            SessionCloseRequest = 10,
            CloseSessionIndication = 11,
            #endregion

            #region Status
            StatusResponse = 12,
            BearerAuthenticationUpdateResponse = 13,
            BearerAuthenticationReadResponse = 14,
            LimitWarningRequest = 15,
            LimitCriticalRequest = 16,
            StatusIndication = 17,
            BearerAuthenticationUpdateIndication = 18,
            BearerAuthenticationReadIndication = 19,
            BearerPriorityIndication = 20,
            BearerPriorityConfirm = 27,
            #endregion

            #region Digitizer
            StreamingVideoControlResponse = 22,
            BufferStatusRequest = 23,
            ScreenCaptureResponse = 24,            
            #endregion

            #region File Transfer
            FileOpenResponse = 25,
            ClearFileResponse = 26,
            #endregion
            AcceptCallback = 27,
            ReadCallback = 28,
            SendCallback = 29,
            DataReadResponse = 30,
            BearerSlotDelete = 31
        };

        public enum Status
        {
            Unknown = -1,
            General = 0,
            InvalidSequenceNumber = 1 ,
            InvalidSerialNumber = 2 ,
            InvalidSessionID = 3 ,
            InvalidBearer = 4 ,
            UnableVideoServer = 5 ,
            InvalidLength = 6 ,
            InvalidCommand = 7 ,
            InsufficientBandwidth = 8 ,
            VideoEnabled = 9 ,
            VideoDisabled = 10,
            FileOpen = 11,
            FileFail = 12,
            InvalidBlockFile = 13,
            FileReadError = 14,
            EthernetNotAvailable = 15,
            WiFi24NotAvailable = 16,
            WiFi5NotAvailable = 17,
            LTENotAvailable = 18,
            Valid = 0x8000
        };

        public enum LogType
        {
            NoTrace = 0,
            Information = 1,
            Warning = 2,
            Error = 3,
            Exception = 4
        };
    }
}
