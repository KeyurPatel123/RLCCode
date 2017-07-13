using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abiomed.Models;
using Abiomed.Repository;
using Abiomed.Storage;
using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Table; // Namespace for Table storage types
using Microsoft.Azure.Documents;
using System.Globalization;
using System.IO;

namespace Abiomed.Business
{
    public class LogManager : ILogManager
    {
        private IBlobStorage _iBlobStorage;
        private ITableStorage _iTableStorage;
        private string _logName;
        private string _logMessageType;

     //  // private IImageManager _iImageManager; // This was added for testing the Image - It canbe removed

        public LogManager()
        {
            Initialize();
        }

        public async Task Create<T>(string deviceIpAddress, string rlmSerial, T message, Definitions.LogMessageType logMessageType)
        {
            AzureLog<T> log = new AzureLog<T>
            {
                Message = message,
                DeviceIpAddress = deviceIpAddress,
                RLMSerial = rlmSerial,
                LogMessageType = logMessageType.ToString()
            };

            DateTime currentDateTime = DateTime.UtcNow;

            // Create the Metadata to associate with the blob being stored.
            List<KeyValuePair<string, string>> metadata = new List<KeyValuePair<string, string>>();
            metadata.Add(new KeyValuePair<string,string>(_logMessageType, log.LogMessageType)); 
            await _iBlobStorage.UploadAsync(log, CreateLogBlobName(log.RLMSerial, currentDateTime), metadata, _logName);

            //      // await CreateUploadedImage("RL00001"); // THis is for testing only

            // Try testing the TableStorage Repository
            // - Insert
            ApplicationConfiguration appConfig = new ApplicationConfiguration();
            appConfig.PartitionKey = "remotelink";
            //appConfig.RowKey = "logname";
            //appConfig.Value = "remotelink-logs";
            //await _iTableStorage.InsertAsync("config", appConfig);
            appConfig.RowKey = "bloblogmetadata2";
            appConfig.Value = "LogMessageType9";
            //await _iTableStorage.InsertAsync("config", appConfig);

            //var queryResult = await _iTableStorage.GetItemAsync<ApplicationConfiguration>(appConfig.PartitionKey, "RepositoryName", "config");
            //var queryResult1 = await _iTableStorage.GetPartitionItemsAsync<ApplicationConfiguration>("RemoteLink", "config");
            //var queryResult2 = await _iTableStorage.GetAllAsync<ApplicationConfiguration>("config");
            //var queryResult3 = await _iTableStorage.InsertOrMergeAsync("config", appConfig);
            //var queryResult4 = await _iTableStorage.UpdateAsync("config", appConfig);
            //var queryResult5 = await _iTableStorage.DeleteAsync("config", appConfig);
        }

        private string CreateLogBlobName(string deviceName, DateTime currentDateTime)
        {
            StringBuilder blobName = new StringBuilder(deviceName);

            blobName.Append("/" + currentDateTime.Year.ToString("0000"));
            blobName.Append("/" + currentDateTime.Month.ToString("00"));
            blobName.Append("/" + currentDateTime.Day.ToString("00"));
            blobName.Append("/" + currentDateTime.Hour.ToString("00"));
            blobName.Append("/" + currentDateTime.Minute.ToString("00") + "m" + currentDateTime.Second.ToString("00") +"s" + currentDateTime.Millisecond.ToString("000") + "ms");

            return blobName.ToString();
        }

        private void Initialize()
        {
            _iBlobStorage = new BlobStorage();
            _iTableStorage = new TableStorage();
            _logName = "remotelink-logs"; // GetLogName();
            _logMessageType = "LogMessageType"; // GetLogMessageType(); 
     //       //_iImageManager = new ImageManager(); // This was added for testing the Image Storage and can be removed...
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