using Abiomed.DotNetCore.Configuration;
using Abiomed.DotNetCore.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System;
using System.Linq;
using System.Net;
using System.IO;
using ImageMagick;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Drawing;

namespace Abiomed.DotNetCore.Business
{
    public class MediaManager : IMediaManager
    {

        #region Private Member Variables

        private const string LongDateFormat = "yyyy-MM-dd HH:mm:ss.fff";
        private const string PlacementSignalKeyword = "Placement";
        private const string MotorCurrentKeyword = "Motor";
        private const string ImpellaFlowKeyword = "Impella Flow";
        private const string PurgeFlowKeyword = "Purge Flow:";
        private const string PurgePressureKeyword = "Purge Pressure:";
        private const string PurgePlacementKeyword = "Purge Placement:";
        private const string MaxKeyword = "Max";
        private const string MinKeyword = "Min";
        private const string AicKeyword = "AIC";
        private const string AicSnKeyword = "AIC SN:";
        private const string ConfigurationSectionName = "mediamanager";
        private IConfigurationCache _configurationCache;
        private string _liveStreamUrl = string.Empty;
        private string _thumbnailUrl = string.Empty;
        private string _imageMaskFilePath = string.Empty;
        private const string _ocrUrl = "https://vision.googleapis.com/v1/images:annotate?key=AIzaSyCAxnddJZfxRoQ0M0avm9nvaRrZrlAOyvQ";
        private const string _ocrContentType = "application/json; charset=utf-8";
        private const string _ocrJsonPackagePart1 = "{\"requests\":[ {\"image\":{\"content\":\"";
        private const string _ocrJsonPackagePart2 =  "\"},\"features\":[{\"type\":\"DOCUMENT_TEXT_DETECTION\", \"maxResults\":1}]}]}";
        private List<string> _impellaPumpTypes;
        private Dictionary<string, string> _headerReplacements;
        private Dictionary<string, string> _generalReplacements;
        private Dictionary<string, string> _placementReplacements;
        private Dictionary<string, string> _motorReplacements;
        private Dictionary<string, string> _purgePressureReplacements;
        private Dictionary<string, string> _impellaFlowReplacements;
        private Point _alarm1Point;
        private Point _alarm2Point;
        private Point _alarm3Point;
        private MagickImage _imageMask;
        private HttpClient _httpClient;
        private MagickColor _alarmCodeWhite;
        private MagickColor _alarmCodeYellow;
        private MagickColor _alarmCodeRed;
        private Percentage _alarmCodeColorMatchTolerance;
        private bool _ocrDebugMode;
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
                            //if (incommingStream.Name == "RL00005")
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
            try
            {
                var httpWebRequest = PrepareOcrWebRequest();
                var objectToOcr = await CreateOcrPayloadAsync(serialNumber, applyMaskToImage);
                MakeOcrRequest(httpWebRequest, objectToOcr.Item1);
                response = GetOcrResponse(httpWebRequest, serialNumber, batchStartTimeUtc);
                response.Alarm1 = DetermineAlarmCode(objectToOcr.Item2, _alarm1Point).ToString();
                response.Alarm2 = DetermineAlarmCode(objectToOcr.Item2, _alarm2Point).ToString();
                response.Alarm3 = DetermineAlarmCode(objectToOcr.Item2, _alarm3Point).ToString();
            }
            catch (Exception ex)
            {
                response = SetException(serialNumber, batchStartTimeUtc, ex.Message);
            }

            return response;
        }

        #endregion

        #region Private Methods 

        private string DetermineAlarmCode(MagickImage image, Point point)
        {
            var alarmColor = new MagickColor(image.GetPixels().GetPixel(point.X, point.Y).ToColor().ToString());
            if (_alarmCodeRed.FuzzyEquals(alarmColor, _alarmCodeColorMatchTolerance))
            {
                return AlarmCodes.Red.ToString();
            }

            if (_alarmCodeYellow.FuzzyEquals(alarmColor, _alarmCodeColorMatchTolerance))
            {
                return AlarmCodes.Yellow.ToString();
            }

            if (_alarmCodeWhite.FuzzyEquals(alarmColor, _alarmCodeColorMatchTolerance))
            {
                return AlarmCodes.White.ToString();
            }

            return AlarmCodes.Blank.ToString();
        }

        private byte[] ApplyMaskToImage(byte[] thumbnail)
        {
            MagickImageCollection images = new MagickImageCollection();
            images.Add(new MagickImage(thumbnail));
            images.Add(_imageMask);

            return images.Mosaic().ToByteArray();
        }

        private async Task<MagickImage> GetImageAsync(string serialNumber, bool applyMaskToImage)
        {
            var imageToOcr = new MagickImage();
            try
            {
                byte[] rawImage = await _httpClient.GetByteArrayAsync(string.Format(_thumbnailUrl, serialNumber));
                imageToOcr = new MagickImage(applyMaskToImage ? ApplyMaskToImage(rawImage) : rawImage);
 
                if (_ocrDebugMode)
                {
                    var raw = new MagickImage(rawImage);
                    raw.Write("C:\\Development\\" + serialNumber + "_raw.jpg");
                    imageToOcr.Write("C:\\Development\\" + serialNumber + "_GetImageAync.jpg");
                }
            }
            catch
            {
                // TODO Ignore 404 Error
            }

            return imageToOcr;
        }

        private async Task<Tuple<string, MagickImage>> CreateOcrPayloadAsync(string serialNumber, bool applyMaskToImage)
        {
            var imageToOcr = await GetImageAsync(serialNumber, applyMaskToImage);
            StringBuilder payload = new StringBuilder(_ocrJsonPackagePart1);
            payload.Append(Convert.ToBase64String(imageToOcr.ToByteArray()));
            payload.Append(_ocrJsonPackagePart2);
            return new Tuple<string, MagickImage>(payload.ToString(), imageToOcr);
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

        private OcrResponse ParseOcrResponse(string serialNumber, DateTime batchStartTimeUtc, string rawText)
        {
            OcrResponse ocrResponse = new OcrResponse();

            ocrResponse.SerialNumber = serialNumber;
            ocrResponse.ProcessDateTimeUtc = DateTime.UtcNow.ToString(LongDateFormat, CultureInfo.InvariantCulture);
            ocrResponse.BatchStartTimeUtc = batchStartTimeUtc.ToString(LongDateFormat, CultureInfo.InvariantCulture);
            ocrResponse.ScreenName = ScreenName.Unknown.ToString();
            ocrResponse.RawMessage = rawText;

            if (!ProcessPlacementSignalScreen(ocrResponse))
            {
                // to do check for oher screens
            }

            return ocrResponse;
        }

        private string GetTextFrom(string text, string beginParseKey)
        {
            string result = string.Empty;

            int beginPosition = text.IndexOf(beginParseKey);
            if (beginPosition > 0)
            {
                result = text.Substring(beginPosition + beginParseKey.Length);
            }

            return result;
        }

        private string GetTextBetween(string text, string beginParseKey, string endParseKey)
        {
            string result = string.Empty;

            int beginPosition = text.IndexOf(beginParseKey);
            if (beginPosition > 0)
            {
                int beginPositionOffset = beginPosition + beginParseKey.Length;
                int endPosition = text.Substring(beginPosition + beginParseKey.Length).IndexOf(endParseKey);
                if (endPosition > 0)
                {
                    result = text.Substring(beginPositionOffset, endPosition);
                }
            }

            return result;
        }

        private bool ProcessPlacementSignalScreen(OcrResponse ocrResponse)
        {
            bool processPlacementSignalScreen = false;
            try
            {
                int placementSignalTextStartPosition = ocrResponse.RawMessage.IndexOf(PlacementSignalKeyword);
                if (placementSignalTextStartPosition > 0)
                {
                    int messageLengthBuffer = ocrResponse.RawMessage.Length * 2;
                    StringBuilder message = new StringBuilder(ocrResponse.RawMessage, messageLengthBuffer);

                    string plainMessage = StandardizeMessageFormat(message, _generalReplacements);
                    placementSignalTextStartPosition = plainMessage.IndexOf(PlacementSignalKeyword);

                    ocrResponse.ScreenName = ScreenName.PlacementSignal.ToString();
                    processPlacementSignalScreen = true;

                    bool foundAlarmSection = true;
                    int endOfHeaderSection = plainMessage.IndexOf("ZZ");
                    if (endOfHeaderSection < 0)
                    {
                        endOfHeaderSection = placementSignalTextStartPosition;
                        foundAlarmSection = false;
                    }

                    var headerSection = GetHeaderSection(new StringBuilder(plainMessage.Substring(0, endOfHeaderSection).Trim(), messageLengthBuffer));
                    ocrResponse.PumpType = headerSection.Item1;
                    ocrResponse.PumpSerialNumber = headerSection.Item2;
                    ocrResponse.AicSerialNumber = headerSection.Item3;
                    ocrResponse.AicSoftwareVersion = headerSection.Item4;
                    ocrResponse.IsDemo = ocrResponse.PumpSerialNumber.StartsWith('6') ? "true" : "false";

                    if (foundAlarmSection)
                    {
                        var alarms = GetAlarmSection(plainMessage.Substring(endOfHeaderSection + 2, placementSignalTextStartPosition - (endOfHeaderSection + 2)).Trim());
                        ocrResponse.Alarm1Message = alarms.Item1;
                        ocrResponse.Alarm2Message = alarms.Item2;
                        ocrResponse.Alarm3Message = alarms.Item3;
                    }

                    var placementSignal = GetPlacementSignal(new StringBuilder(GetTextBetween(plainMessage, PlacementSignalKeyword, MotorCurrentKeyword).Trim(), messageLengthBuffer));
                    ocrResponse.PlacementSignalSystole = placementSignal.Item1;
                    ocrResponse.PlacementSignalDistole = placementSignal.Item2;
                    ocrResponse.PlacementSignalAverage = placementSignal.Item3;
                    ocrResponse.PerformanceLevel = placementSignal.Item4;

                    var motorCurrent = GetMotorCurrent(new StringBuilder(GetTextBetween(plainMessage, MotorCurrentKeyword, ImpellaFlowKeyword).Trim(), messageLengthBuffer));
                    ocrResponse.MotorCurrentSystole = motorCurrent.Item1;
                    ocrResponse.MotorCurrentDistole = motorCurrent.Item2;
                    ocrResponse.MotorCurrentAverage = motorCurrent.Item3;

                    var impellaFlowSection = GetImpellaFlowSection(new StringBuilder(GetTextBetween(plainMessage, ImpellaFlowKeyword, PurgeFlowKeyword).Trim(), messageLengthBuffer), out bool foundFlowMin);
                    ocrResponse.FlowRateMax = impellaFlowSection.Item1;
                    ocrResponse.FlowRateMin = impellaFlowSection.Item2;
                    ocrResponse.FlowRateAverage = impellaFlowSection.Item3;

                    ocrResponse.PurgeFlow = GetPurgeFlowSection(GetTextBetween(plainMessage, PurgeFlowKeyword, PurgePressureKeyword).Trim());

                    var purgePressureToEndSection = GetPurgePressureToEndSection(new StringBuilder(GetTextFrom(plainMessage, PurgePressureKeyword), messageLengthBuffer), foundFlowMin);
                    ocrResponse.Battery = purgePressureToEndSection.Item1;
                    if (!foundFlowMin)
                    {
                        ocrResponse.FlowRateMin = purgePressureToEndSection.Item2;
                    }

                    ocrResponse.PurgePressure = purgePressureToEndSection.Item3;
                }
            } catch(Exception EX)
            {
                var xxx = EX.Message; // TODO Remove - for working/tweaking the OCR porocessing.
            }
            return processPlacementSignalScreen;
        }

        private string StandardizeMessageFormat(StringBuilder ocrText, Dictionary<string,string> replacements)
        {
            foreach (string key in replacements.Keys)
            {
                ocrText.Replace(key, replacements[key]);
            }

            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);
            return regex.Replace(ocrText.ToString(), " ").Trim();
        }


        private void Initialize()
        {
            _liveStreamUrl = _configurationCache.GetConfigurationItem(ConfigurationSectionName, "livestreamurl");
            _thumbnailUrl = _configurationCache.GetConfigurationItem(ConfigurationSectionName, "thumbnailurl");
            string imageMaskFilePath = Directory.GetCurrentDirectory() + _configurationCache.GetConfigurationItem(ConfigurationSectionName, "imagemaskpath");
            var imageMaskStream = File.OpenRead(imageMaskFilePath);
           _imageMask = new MagickImage(imageMaskStream);

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _impellaPumpTypes = new List<string>(_configurationCache.GetConfigurationItem(ConfigurationSectionName, "impellapumptypes").Split('|'));

            _generalReplacements = LoadReplacementRules("ocrreplacementsgeneral");
            _headerReplacements = LoadReplacementRules("ocrreplacementsheader");
            _placementReplacements = LoadReplacementRules("ocrreplacementsplacement");
            _motorReplacements = LoadReplacementRules("ocrreplacementsmotor");
            _purgePressureReplacements = LoadReplacementRules("ocrreplacementspurgepressure");
            _impellaFlowReplacements = LoadReplacementRules("ocrreplacementsimpellaflow");
            _alarm1Point = LoadAlarmPoint("ocralarm1coordinates");
            _alarm2Point = LoadAlarmPoint("ocralarm2coordinates");
            _alarm3Point = LoadAlarmPoint("ocralarm3coordinates");
            _alarmCodeRed = new MagickColor(_configurationCache.GetConfigurationItem(ConfigurationSectionName, "ocralarmcodered"));
            _alarmCodeYellow = new MagickColor(_configurationCache.GetConfigurationItem(ConfigurationSectionName, "ocralarmcodeyellow"));
            _alarmCodeWhite = new MagickColor(_configurationCache.GetConfigurationItem(ConfigurationSectionName, "ocralarmcodewhite"));
            _alarmCodeColorMatchTolerance = new Percentage(int.Parse(_configurationCache.GetConfigurationItem(ConfigurationSectionName, "ocralarmcodematchtolerance")));
            _ocrDebugMode = bool.Parse(_configurationCache.GetConfigurationItem(ConfigurationSectionName, "ocrdebug"));
        }

        private Point LoadAlarmPoint(string configurationName)
        {
            string[] coords = _configurationCache.GetConfigurationItem(ConfigurationSectionName, configurationName).Split(',');
            return new Point(int.Parse(coords[0]), int.Parse(coords[1]));
        }

        private Dictionary<string, string> LoadReplacementRules(string configuredParseRuleName)
        {
            Dictionary<string, string>  dictionary = new Dictionary<string, string>();
            foreach (var item in _configurationCache.GetConfigurationItem(ConfigurationSectionName, configuredParseRuleName).Split('|'))
            {
                var parts = item.Split('^');
                dictionary.Add(parts[0], parts.Count() > 1 ? parts[1] : string.Empty);
            }

            return dictionary;
        }

        private Tuple<string,string,string> GetAlarmSection(string section)
        {
            string alarm1 = string.Empty;
            string alarm2 = string.Empty;
            string alarm3 = string.Empty;

            int i = 1;
            foreach (string alarmMessage in section.Split("ZZ"))
            {
                switch (i)
                {
                    case 1:
                        alarm1 = alarmMessage.Trim();
                        break;
                    case 2:
                        alarm2 = alarmMessage.Trim();
                        break;
                    case 3:
                        alarm3 = alarmMessage.Trim();
                        break;
                    default:
                        alarm3 = string.Format("{0}. {1}", alarm3, alarmMessage.Trim());
                        break;
                }

                i++;
            }

            return new Tuple<string, string, string>(alarm1, alarm2, alarm3);
        }

        private Tuple<string, string, string, string> GetHeaderSection(StringBuilder section)
        {
            string ocrText = StandardizeMessageFormat(section, _headerReplacements);
            string pumpType = GetPumpType(ocrText);
            string pumpSerialNumber = string.Empty;

            if (!string.IsNullOrWhiteSpace(pumpType))
            {
                pumpSerialNumber = GetPumpSerialNumber(ocrText.Substring(pumpType.Length + 1));
            }

            string aicSerialNumber = GetAicSerialNumber(ocrText);
            if (aicSerialNumber == pumpSerialNumber)
            {
                aicSerialNumber = string.Empty;
            }
            string aicSoftwareVersion = GetAicSoftwareVersion(ocrText);

            return new Tuple<string, string, string, string>(pumpType, pumpSerialNumber, aicSerialNumber, aicSoftwareVersion);
        }

        private Tuple<string, string, string> GetImpellaFlowSection(StringBuilder section, out bool foundMinValue)
        {
            string flowMax = string.Empty;
            string flowMin = string.Empty;
            string flow = string.Empty;
            foundMinValue = false;
            try
            {
                string ocrText = StandardizeMessageFormat(section, _impellaFlowReplacements);

                // MAX 
                int maxPosition = ocrText.IndexOf(MaxKeyword);
                if (maxPosition > 0)
                {
                    flowMax = new string(ocrText.Substring(0, maxPosition).Where(c => char.IsDigit(c) || c == '.').ToArray());
                    int fieldLength = maxPosition + MaxKeyword.Length + 1;
                    ocrText = fieldLength >= ocrText.Length ? string.Empty : ocrText.Substring(fieldLength);
                }

                // MIN - Sometimes Min is in this section - about 50% of the time it is in Purge Pressure Section...  So we check here too.
                // If we find it we want to pass a flag to the caller telling them we found it so we do not check for it again
                int minPosition = ocrText.IndexOf(MinKeyword);
                if (minPosition > 0)
                {
                    flowMin = new string(ocrText.Substring(0, minPosition).Where(c => char.IsDigit(c) || c == '.').ToArray());
                    int fieldLength = minPosition + MinKeyword.Length + 1;
                    ocrText = fieldLength >= ocrText.Length ? string.Empty : ocrText.Substring(fieldLength);
                    foundMinValue = true;
                }

                // Get the flow - it is the left over string
                flow = new string(ocrText.Where(c => char.IsDigit(c) || c == '.').ToArray());
            }
            catch(Exception EX)
            {
                var xxx = EX.Message; // TODO Remove - for working/tweaking the OCR porocessing.
            }

            return new Tuple<string, string, string>(flowMax, flowMin, flow);
        }

        private string GetPurgeFlowSection(string ocrText)
        {
            return new string(ocrText.Where(c => char.IsDigit(c) || c == '.').ToArray());
        }

        private Tuple<string, string, string> GetPurgePressureToEndSection(StringBuilder section, bool hasFlowMinBeenFound)
        {
            string ocrText = StandardizeMessageFormat(section, _purgePressureReplacements);
            string flowMin = string.Empty;
            string purgePressure = string.Empty;
            string battery = GetBattery(ocrText);

            if (battery != string.Empty)
            {
                ocrText = ocrText.Replace(battery, "").Replace("%", "");
            }

            if (!hasFlowMinBeenFound)
            {
                int minPosition = ocrText.IndexOf(MinKeyword);
                if (minPosition > 0)
                {
                    flowMin = ocrText.Substring(0, minPosition).Trim();
                    int priorSpacePosition = flowMin.LastIndexOf(' ');
                    if (priorSpacePosition > -1)
                    {
                        flowMin = flowMin.Substring(priorSpacePosition).Trim();
                    }
                    if (flowMin != string.Empty)
                    {
                        ocrText = ocrText.Replace(flowMin, "").Replace(MinKeyword, "");
                    }
                }
            }

            purgePressure = new string(ocrText.Where(c => char.IsDigit(c) || c == '.').ToArray());
            return new Tuple<string, string, string>(battery, flowMin, purgePressure);
        }

        private string GetPumpType(string ocrText)
        {
            string result = string.Empty;

            foreach(string pumpType in _impellaPumpTypes)
            {
                if (ocrText.StartsWith(pumpType))
                {
                    result = pumpType;
                    break;
                }
            }

            return result;
        }

        private string GetPumpSerialNumber(string ocrText)
        {
            string result = string.Empty;
            int startPos = ocrText.IndexOf(':') + 1;

            if (startPos > 1)
            {
                string workString = ocrText.Substring(startPos).Trim();
                int endPos = workString.IndexOf(AicKeyword);
                if (endPos > 0)
                {
                    result = ocrText.Substring(startPos, endPos).Trim();
                }
                else
                {
                    endPos = workString.IndexOf(' ');
                    if (endPos > 0)
                    {
                        result = workString.Substring(0, endPos);
                    }
                    else
                    {
                        result = new string(ocrText.Where(c => char.IsDigit(c)).ToArray());
                    }
                }
            }
            else
            {
                int spacePos = ocrText.IndexOf(' ') + 1;
                if (spacePos > 0)
                {
                    result = ocrText.Substring(0, startPos).Trim();
                }
                else // There are no other strings...
                {
                    result = new string(ocrText.Where(c => char.IsDigit(c)).ToArray());
                }
            }

            return result;
        }

        private string GetAicSerialNumber(string ocrText)
        {
            string result = string.Empty;
            int startPos = ocrText.IndexOf(AicSnKeyword) + AicSnKeyword.Length+1;

            if (startPos > AicSnKeyword.Length + 1)
            {
                int endPos = ocrText.Substring(startPos).IndexOf(AicKeyword);
                if (endPos > 0)
                {
                    result = ocrText.Substring(startPos, endPos).Trim();
                }
            }

            return result;
        }

        private string GetAicSoftwareVersion(string ocrText)
        {
            string result = string.Empty;

            int index = ocrText.LastIndexOf(' ');
            if (index>0)
            {
                result = new string(ocrText.Substring(index).Where(c => char.IsDigit(c) || c=='.').ToArray());
            }

            return result;
        }

        private Tuple<string, string, string, string> GetPlacementSignal(StringBuilder section)
        {
            string systole = string.Empty;
            string distole = string.Empty;
            string average = string.Empty;
            string pLevel = string.Empty;

            string ocrText = StandardizeMessageFormat(section, _placementReplacements);

            // Get the string between 'Placement Signal' And 'Motor Current'
            // This may Contain (in any order - well for the p-n) and may be missing average
            //   - systole/distole (average) P-n

            string[] pieces = ocrText.Trim().Split();
            foreach (string piece in pieces)
            {
                int slash = piece.IndexOf('/');
                if (slash > 0)
                {
                    systole = piece.Substring(0, slash);
                    distole = piece.Substring(slash + 1);
                }
                else
                {
                    switch (piece.Substring(0, 1))
                    {
                        case "P":
                            pLevel = string.IsNullOrWhiteSpace(pLevel) ? piece : piece + pLevel;
                            break;
                        case ".":
                        case "-":
                            pLevel += piece;
                            break;
                        case "(":
                            average = new string(piece.Where(c => char.IsDigit(c)).ToArray());
                            break;
                        default:
                            break;
                    }
                }
            }

            return new Tuple<string, string, string, string>(systole, distole, average, pLevel.Replace('.','-'));
        }

        private Tuple<string, string, string> GetMotorCurrent(StringBuilder section)
        {
            string systole = string.Empty;
            string distole = string.Empty;
            string average = string.Empty;
            string ocrText = StandardizeMessageFormat(section, _motorReplacements);

            // Get the string between 'Motor Current' and 'Impella'
            // This may Contain:
            //   - systole/distole (average)
            string[] pieces = ocrText.Trim().Split();
            foreach (string piece in pieces)
            {
                int slash = piece.IndexOf('/');
                if (slash > 0)
                {
                    systole = piece.Substring(0, slash);
                    distole = piece.Substring(slash + 1);
                }
                else
                {
                    average = new string(piece.Where(c => char.IsDigit(c)).ToArray());
                }
            }

            return new Tuple<string, string, string>(systole, distole, average);
        }

        private string GetBattery(string ocrText)
        {
            string battery = string.Empty;

            int batteryPercentPosition = ocrText.IndexOf("%");
            if (batteryPercentPosition < 0)
            {
                //Couple OCR Issues to clenaup.
                //- Trailing '.' and embedded '. '
                if (ocrText.EndsWith('.'))
                {
                    ocrText = ocrText.Remove(ocrText.Length - 1, 1) + "%";
                    batteryPercentPosition = ocrText.Length;
                }
                else
                {
                    ocrText = ocrText.Replace(". ", "% ");
                    batteryPercentPosition = ocrText.IndexOf('%');
                }
            }

            if (batteryPercentPosition > 0)
            {
                string workSpace = ocrText.Substring(0, batteryPercentPosition).Trim();
                int priorSpace = workSpace.LastIndexOf(" ");
                if (priorSpace > 0)
                {
                    battery = workSpace.Substring(priorSpace);
                }
            }

            return battery;
        }

        #endregion
    }
}