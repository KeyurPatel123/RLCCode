using Abiomed.DotNetCore.Configuration;
using Abiomed.DotNetCore.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System;
using System.Net;
using System.IO;

namespace Abiomed.DotNetCore.Business
{
    public class MediaManager : IMediaManager
    {
        private IConfigurationCache _configurationCache;
        private string _liveStreamUrl = string.Empty;
        private string _thumbnailUrl = string.Empty;
        private const string _ocrUrl = "https://vision.googleapis.com/v1/images:annotate?key=AIzaSyCAxnddJZfxRoQ0M0avm9nvaRrZrlAOyvQ";
        private const string _ocrContentType = "application/json; charset=utf-8";
        private const string _ocrJsonPackagePart1 = "{\"requests\":[ {\"image\":{\"content\":\"";
        private const string _ocrJsonPackagePart2 =  "\"},\"features\":[{\"type\":\"DOCUMENT_TEXT_DETECTION\", \"maxResults\":1}]}]}";
        private HttpClient _httpClient;

        public MediaManager(IConfigurationCache configurationCache)
        {
            _configurationCache = configurationCache;
            Initialize();
        }

        public async Task<List<string>> GetLiveStreamsAsync()
        {
            var stringTaskResult = await _httpClient.GetStringAsync(_liveStreamUrl);
            var liveStreams = JsonConvert.DeserializeObject<WowzaLiveStream>(stringTaskResult);

            List<string> serialNumbers = new List<string>();
            if (liveStreams != null && liveStreams.IncomingStreams != null)
            {
                foreach (IncomingStream incommingStream in liveStreams.IncomingStreams)
                {
                    bool.TryParse(incommingStream.IsConnected, out bool isConnected);
                    if (isConnected)
                    {
                        if (!serialNumbers.Contains(incommingStream.Name))
                        {
                            serialNumbers.Add(incommingStream.Name);
                        }
                    }
                }
            }

            return serialNumbers;
        }

        public async Task<string> GetImageTextAsync(string serialNumber)
        {
            var response = string.Empty;
            try
            {
                var thumbnail = await GetThumbNailAsync(serialNumber);

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(_ocrUrl);
                httpWebRequest.ContentType = _ocrContentType;
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(_ocrJsonPackagePart1 + Convert.ToBase64String(thumbnail) + _ocrJsonPackagePart2);
                    streamWriter.Flush();
                }
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                   response = streamReader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                // TODO - Handle Exception
            }

            return response;
        }

        private async Task<byte[]> GetThumbNailAsync(string serialNumber)
        {
            byte[] thumbnail = null;
            try
            {
                thumbnail = await _httpClient.GetByteArrayAsync(string.Format(_thumbnailUrl, serialNumber));
            }
            catch
            {
                // TODO Ignore 404 Error
            }

            return thumbnail;
        }

        private void NewClient()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private void Initialize()
        {
            _liveStreamUrl = _configurationCache.GetConfigurationItem("mediamanager", "livestreamurl");
            _thumbnailUrl = _configurationCache.GetConfigurationItem("mediamanager", "thumbnailurl");

            NewClient();
        }
    }
}