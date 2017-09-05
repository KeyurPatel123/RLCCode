using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Abiomed.DotNetCore.Models;
using Abiomed.DotNetCore.Storage;
using System.Diagnostics;
using Abiomed.DotNetCore.Configuration;

namespace Abiomed.DotNetCore.Business
{
    public class LogManager : ILogManager
    {
        private IBlobStorage _iBlobStorage;
        private ITableStorage _iTableStorage;
        private string _logName;
        private string _logMessageType;
        private string _logMessageSeverity;        

        public LogManager()
        {            
            // todo - fix
            Initialize("DefaultEndpointsProtocol=https;AccountName=remotelink;AccountKey=ykKtbMsrZJI8DvikFLhWgy7EpGheIfUJzKB87nTgoQm0hwLcYBYhnMxEJhcD+HIHMZ/bBvSf9kjHNg+4CnYd4w==");
        }

        public async Task LogAsync<T>(string deviceIpAddress, string rlmSerial, T message, Definitions.LogMessageType logMessageType, Definitions.LogType logSeverityType = Definitions.LogType.NoTrace, string traceMessage = null)
        {
            TraceIt(logSeverityType, traceMessage);
            AzureLog<T> log = GenerateLogMessage(deviceIpAddress, rlmSerial, message, logMessageType, logSeverityType);
            DateTime currentDateTime = DateTime.UtcNow;
            List<KeyValuePair<string, string>> metadata = GenerateMetadata(logMessageType.ToString(), logSeverityType.ToString());

            await _iBlobStorage.UploadAsync(log, CreateLogBlobName(log.RLMSerial, currentDateTime), metadata, _logName);
        }

        public void Log<T>(string deviceIpAddress, string rlmSerial, T message, Definitions.LogMessageType logMessageType, Definitions.LogType logSeverityType = Definitions.LogType.NoTrace, string traceMessage = null)
        {
            TraceIt(logSeverityType, traceMessage);
            AzureLog<T> log = GenerateLogMessage(deviceIpAddress, rlmSerial, message, logMessageType, logSeverityType);
            DateTime currentDateTime = DateTime.UtcNow;
            List<KeyValuePair<string, string>> metadata = GenerateMetadata(logMessageType.ToString(), logSeverityType.ToString());

            _iBlobStorage.UploadAsync(log, CreateLogBlobName(log.RLMSerial, currentDateTime), metadata, _logName);
        }
    
        public void TraceIt(Definitions.LogType logType, string message)
        {
            if (logType != Definitions.LogType.NoTrace && !string.IsNullOrWhiteSpace(message))
            {
                switch (logType)
                {
                    case Definitions.LogType.Information:
                        Trace.TraceInformation(message);
                        break;
                    case Definitions.LogType.Warning:
                        Trace.TraceWarning(message);
                        break;
                    case Definitions.LogType.Error:
                    case Definitions.LogType.Exception:
                        Trace.TraceError(message);
                        break;
                    default:
                        break;
                }
            }
        }

        private string CreateLogBlobName(string deviceName, DateTime currentDateTime)
        {
            StringBuilder blobName = new StringBuilder(deviceName);

            blobName.Append("/" + currentDateTime.Year.ToString("0000"));
            blobName.Append("/" + currentDateTime.Month.ToString("00"));
            blobName.Append("/" + currentDateTime.Day.ToString("00"));
            blobName.Append("/" + currentDateTime.Hour.ToString("00"));
            blobName.Append("/" + currentDateTime.Minute.ToString("00") + "m" + currentDateTime.Second.ToString("00") + "s" + currentDateTime.Millisecond.ToString("000") + "ms");

            return blobName.ToString();
        }

        private void Initialize(string connection)
        {
            _iBlobStorage = new BlobStorage();
            _iTableStorage = new TableStorage(connection);
            _logName = "remotelink-logs"; // TODO: GetLogName();
            _logMessageType = "LogMessageType"; // TODO: GetLogMessageType(); 
            _logMessageSeverity = "LogMessageSeverity"; // TODO: GetLogMessageSeverity();
        }

        private string GetLogName()
        {
            var queryResult =  _iTableStorage.GetItemAsync<ApplicationConfiguration>("remotelink", "logname", "config");
            return queryResult.Result.Value;
        }

        private string GetLogMessageType()
        {
            var queryResult = _iTableStorage.GetItemAsync<ApplicationConfiguration>("remotelink", "bloblogmetadata", "config");
            return queryResult.Result.Value;
        }

        private AzureLog<T> GenerateLogMessage<T>(string deviceIpAddress, string rlmSerialNumber, T message, Definitions.LogMessageType logMessageType, Definitions.LogType logType)
        {
            AzureLog<T> log = new AzureLog<T>
            {
                Message = message,
                DeviceIpAddress = deviceIpAddress,
                RLMSerial = rlmSerialNumber,
                LogMessageType = logMessageType.ToString(),
                LogSeverityType = logType.ToString()
            };

            return log;
        }

        private List<KeyValuePair<string, string>> GenerateMetadata(string logMessageType, string logSeverityType)
        {
            List<KeyValuePair<string, string>> metadata = new List<KeyValuePair<string, string>>();
            metadata.Add(new KeyValuePair<string, string>(_logMessageType, logMessageType));
            metadata.Add(new KeyValuePair<string, string>(_logMessageSeverity, logSeverityType));

            return metadata;
        }

        //    /// <summary>
        //    /// This is for testing the Image Functionality Only...
        //   /// </summary>
        //   /// <param name="deviceName"></param>
        //   /// <returns></returns>
        //  public async Task CreateUploadedImage(string deviceName)
        //   {
        //       List<KeyValuePair<string, string>> metadata = new List<KeyValuePair<string, string>>();
        //       metadata.Add(new KeyValuePair<string, string>("institutionName", "Massachusets General Hospital")); // Values for test
        //       
        //       await _iImageManager.UploadImageFromFile(deviceName, @"C:\Temp\RL00001.png", System.Drawing.Imaging.ImageFormat.Png, metadata, "remotelink-images");
        //   }
    }
}