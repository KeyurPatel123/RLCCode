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
using ImageMagick;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Globalization;

namespace Abiomed.DotNetCore.Business
{
    public class MediaManager : IMediaManager
    {

        #region Private Member Variables

        private const string LongDateFormat = "yyyy-MM-dd HH:mm:ss.fff";
        private IConfigurationCache _configurationCache;
        private string _liveStreamUrl = string.Empty;
        private string _thumbnailUrl = string.Empty;
        private string _imageMaskFilePath = string.Empty;
        private const string _ocrUrl = "https://vision.googleapis.com/v1/images:annotate?key=AIzaSyCAxnddJZfxRoQ0M0avm9nvaRrZrlAOyvQ";
        private const string _ocrContentType = "application/json; charset=utf-8";
        private const string _ocrJsonPackagePart1 = "{\"requests\":[ {\"image\":{\"content\":\"";
        private const string _ocrJsonPackagePart2 =  "\"},\"features\":[{\"type\":\"DOCUMENT_TEXT_DETECTION\", \"maxResults\":1}]}]}";
        private MagickImage _imageMask;
        private HttpClient _httpClient;

        #endregion

        #region Constructors

        public MediaManager(IConfigurationCache configurationCache)
        {
            _configurationCache = configurationCache;
            Initialize();
        }

        #endregion

        #region Public Methods 
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

        public async Task<OcrResponse> GetImageTextAsync(string serialNumber, DateTime batchStartTimeUtc, bool applyMaskToImage = true)
        {
            OcrResponse response = new OcrResponse();
            string payload = string.Empty;
            try
            {
                var httpWebRequest = PrepareOcrWebRequest();
                payload = await CreateOcrPayloadAsync(serialNumber, applyMaskToImage);
                MakeOcrRequest(httpWebRequest, payload);
                response = GetOcrResponse(httpWebRequest, serialNumber, batchStartTimeUtc);
            }
            catch (Exception ex)
            {
                response = SetException(serialNumber, batchStartTimeUtc, ex.Message);
            }

            return response;
        }

        #endregion

        #region Private Methods 

        private byte[] ApplyMaskToImage(byte[] thumbnail)
        {
            MagickImageCollection images = new MagickImageCollection();
            images.Add(new MagickImage(thumbnail));
            images.Add(_imageMask);

            return images.Mosaic().ToByteArray();
        }

        private async Task<byte[]> GetImageAsync(string serialNumber, bool applyMaskToImage)
        {
            byte[] imageToOcr = null;
            try
            {
                byte[] image = await _httpClient.GetByteArrayAsync(string.Format(_thumbnailUrl, serialNumber));
                imageToOcr = applyMaskToImage ? ApplyMaskToImage(image) : image;
            }
            catch
            {
                // TODO Ignore 404 Error
            }

            return imageToOcr;
        }

        private async Task<string> CreateOcrPayloadAsync(string serialNumber, bool applyMaskToImage)
        {
            var imageToOcr = await GetImageAsync(serialNumber, applyMaskToImage);
            StringBuilder payload = new StringBuilder(_ocrJsonPackagePart1);
            payload.Append(Convert.ToBase64String(imageToOcr));
            payload.Append(_ocrJsonPackagePart2);

            return payload.ToString();
        }

        private HttpWebRequest PrepareOcrWebRequest()
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(_ocrUrl);
            httpWebRequest.ContentType = _ocrContentType;
            httpWebRequest.Method = "POST";

            return httpWebRequest;
        }

        private void MakeOcrRequest(HttpWebRequest httpWebRequest, string payload)
        {
            var ocrStreamWriter = new StreamWriter(httpWebRequest.GetRequestStream());
            ocrStreamWriter.Write(payload);
            ocrStreamWriter.Flush();
        }

        private OcrResponse GetOcrResponse(HttpWebRequest httpWebRequest, string serialNumber, DateTime batchStartTimeUtc)
        {
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            var streamReader = new StreamReader(httpResponse.GetResponseStream());
            var jsonText = JObject.Parse(streamReader.ReadToEnd());

           return ParseOcrResponse(serialNumber, batchStartTimeUtc, jsonText["responses"][0]["fullTextAnnotation"]["text"].ToString());
        }

        private OcrResponse SetException(string serialNumber, DateTime batchStartTimeUtc, string exceptionText)
        {
            OcrResponse ocrResponse = new OcrResponse();

            ocrResponse.SerialNumber = serialNumber;
            ocrResponse.ProcessDateTimeUtc = DateTime.UtcNow.ToString(LongDateFormat, CultureInfo.InvariantCulture);
            ocrResponse.BatchStartTimeUtc = batchStartTimeUtc.ToString(LongDateFormat, CultureInfo.InvariantCulture);
            ocrResponse.ResultStatusNote = exceptionText;

            return ocrResponse;
        }

        private string GetCharactersLeft(string line,string seperator)
        {
            string result = string.Empty;

            int iPos = line.IndexOf(seperator);
            if (iPos > 1)
            {
                result = line.Substring(0, iPos);
            }

            return result;
        }

        private string GetCharactersLeft(string line, char seperator)
        {
            string result = string.Empty;

            int iPos = line.IndexOf(seperator);
            if (iPos > 1)
            {
                result = line.Substring(0, iPos);
            }

            return result;
        }

        private string GetCharactersRight(string line, char seperator)
        {
            string result = string.Empty;

            int iPos = line.IndexOf(seperator);
            if (iPos > 1)
            {
                if (iPos + 1 <= line.Length)
                {
                    result = line.Substring(iPos + 1);
                }
            }

            return result;
        }

        private string GetCharactersRight(string line, string seperator)
        {
            string result = string.Empty;

            int iPos = line.IndexOf(seperator);
            if (iPos > 1)
            {
                if (iPos + 1 <= line.Length)
                {
                    result = line.Substring(iPos + 1);
                }
            }

            return result;
        }

        private string GetCharactersRightFromEnd(string line, char seperator)
        {
            string result = string.Empty;

            int iPos = line.LastIndexOf(seperator);
            if (iPos > 1)
            {
                if (iPos + 1 <= line.Length)
                {
                    result = line.Substring(iPos + 1);
                }
            }

            return result;
        }

        private string GetAicSerialNumber(string line)
        {
            // Pattern AIC SN <Aic_Serial_Number> AIC 5.1
            string result = string.Empty;

            result = GetCharactersRight(line, "SN ");
            result = GetCharactersLeft(result, ' ');

            return result;
        }

        private OcrResponse ParseOcrResponse(string serialNumber, DateTime batchStartTimeUtc, string rawText)
        {
            OcrResponse ocrResponse = new OcrResponse();

            ocrResponse.SerialNumber = serialNumber;
            ocrResponse.ProcessDateTimeUtc = DateTime.UtcNow.ToString(LongDateFormat, CultureInfo.InvariantCulture);
            ocrResponse.BatchStartTimeUtc = batchStartTimeUtc.ToString(LongDateFormat, CultureInfo.InvariantCulture);
            ocrResponse.RawMessage = rawText;

            string[] lines = rawText.Split('\n');
            if (lines.Length == 17)
            {
                ocrResponse.PumpType = GetCharactersLeft(lines[0], "SN:");
                ocrResponse.ImpellaSerialNumber = GetCharactersRight(lines[0], ':');
                ocrResponse.IsDemo = ocrResponse.ImpellaSerialNumber.StartsWith('6') ? "true" : "false";
                ocrResponse.AicSerialNumber = GetAicSerialNumber(lines[1]);
                ocrResponse.SoftwareVersion = GetCharactersRightFromEnd(lines[1].TrimEnd(), ' ');
                ocrResponse.PlacementSignal = lines[4];
                ocrResponse.PLevel = lines[5];
                ocrResponse.MotorCurrent = lines[8];
                ocrResponse.MotorCurrentAverage = lines[9];
                ocrResponse.ImpellaFlow = lines[14];
                ocrResponse.ImpellaFlowMax = GetCharactersLeft(lines[12], ' ');
                ocrResponse.PurgeFlow = GetCharactersRightFromEnd(lines[12].TrimEnd(), ':');
                ocrResponse.ImpellaFlowMin = GetCharactersLeft(lines[13], ' ');
                ocrResponse.PurgePressure = GetCharactersRightFromEnd(lines[13].TrimEnd(), ':');
                ocrResponse.SystemPower = lines[15];
            }
            else
            {
                ocrResponse.ResultStatusNote = string.Format("OCR Response is not in the exected format.  Expected a Line count of 17 and received {0}.", lines.Length);
            }

            return ocrResponse;
        }

        private void Initialize()
        {
            _liveStreamUrl = _configurationCache.GetConfigurationItem("mediamanager", "livestreamurl");
            _thumbnailUrl = _configurationCache.GetConfigurationItem("mediamanager", "thumbnailurl");
            string imageMaskFilePath = Directory.GetCurrentDirectory() + _configurationCache.GetConfigurationItem("mediamanager", "imagemaskpath");
            var imageMaskStream = File.OpenRead(imageMaskFilePath);
           _imageMask = new MagickImage(imageMaskStream);

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        #endregion
    }
}