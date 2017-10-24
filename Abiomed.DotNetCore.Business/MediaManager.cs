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
        private const string PurgeFlowKeyword = "Purge Flow";
        private const string PurgePressureKeyword = "Purge Pressure";
        private const string AlarmStartKeyword = "_ALARMSTART_";
        private const string ImpellaFlowValidationFormat = "0.0";
        private const string MaxKeyword = "Max";
        private const string MinKeyword = "Min";
        private const string AicKeyword = "AIC";
        private const string AicSpaceKeyword = " AIC ";
        private const string AicSnKeyword = "AIC SN:";
        private const string ConfigurationSectionName = "mediamanager";
        private IConfigurationCache _configurationCache;
        private string _liveStreamUrl = string.Empty;
        private string _thumbnailUrl = string.Empty;
        private string _imageMaskFilePath = string.Empty;
        private const string _ocrUrl = "https://vision.googleapis.com/v1/images:annotate?key=AIzaSyCAxnddJZfxRoQ0M0avm9nvaRrZrlAOyvQ";
        private const string _ocrContentType = "application/json; charset=utf-8";
        private const string _ocrJsonPackagePart1 = "{\"requests\":[ {\"image\":{\"content\":\"";
        private const string _ocrJsonPackagePart2 = "\"},\"features\":[{\"type\":\"DOCUMENT_TEXT_DETECTION\", \"maxResults\":1}]}]}";
        private List<string> _impellaPumpTypes;
        private List<string> _aicSerialNumberPrefix;
        private Dictionary<string, string> _headerReplacements;
        private Dictionary<string, string> _generalReplacements;
        private Dictionary<string, string> _placementReplacements;
        private Dictionary<string, string> _motorReplacements;
        private Dictionary<string, string> _purgePressureReplacements;
        private Dictionary<string, string> _impellaFlowReplacements;
        private Dictionary<string, string> _numericFieldReplacements;
        private List<string> _performanceLevelValidationValues;
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
                            //if (incommingStream.Name == "RL00015")
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

        private bool ProcessPlacementSignalScreen(OcrResponse ocrResponse)
        {
            bool processPlacementSignalScreen = false;
            try
            {
                int placementSignalTextStartPosition = ocrResponse.RawMessage.IndexOf(PlacementSignalKeyword);
                if (placementSignalTextStartPosition > 0)
                {
                    var messages = GetMessageSegments(ocrResponse.RawMessage);

                    // Get Header 
                    var headerSection = GetHeaderSection(messages[0]);
                    ocrResponse.PumpType = headerSection.Item1;
                    ocrResponse.PumpSerialNumber = headerSection.Item2;
                    ocrResponse.AicSerialNumber = headerSection.Item3;
                    ocrResponse.AicSoftwareVersion = headerSection.Item4;
                    ocrResponse.IsDemo = ocrResponse.PumpSerialNumber.StartsWith('6') ? "true" : "false";
                    ocrResponse.ScreenName = ScreenName.PlacementSignal.ToString();

                    // Get Alarms 
                    ocrResponse.Alarm1Message = string.Join(" ", messages[1].ToArray()).Trim();
                    ocrResponse.Alarm2Message = string.Join(" ", messages[2].ToArray()).Trim();
                    ocrResponse.Alarm3Message = string.Join(" ", messages[3].ToArray()).Trim();

                    // Get Placement
                    var placementSignal = GetPlacementSignal(messages[4]);
                    ocrResponse.PlacementSignalSystole = placementSignal.Item1;
                    ocrResponse.PlacementSignalDistole = placementSignal.Item2;
                    ocrResponse.PlacementSignalAverage = placementSignal.Item3;
                    ocrResponse.PerformanceLevel = placementSignal.Item4;

                    // Get Motor Current
                    var motorCurrent = GetMotorCurrent(messages[5]);
                    ocrResponse.MotorCurrentSystole = motorCurrent.Item1;
                    ocrResponse.MotorCurrentDistole = motorCurrent.Item2;
                    ocrResponse.MotorCurrentAverage = motorCurrent.Item3;

                    // Get Impella Flow 
                    var impellaFlowSection = GetImpellaFlowSection(messages[6]);
                    ocrResponse.FlowRateMax = impellaFlowSection.Item1;
                    ocrResponse.FlowRateMin = impellaFlowSection.Item2;
                    ocrResponse.FlowRateAverage = impellaFlowSection.Item3;

                    // Get Purge Flow 
                    ocrResponse.PurgeFlow = GetPurgeFlowSection(messages[7]);

                    // Get Purge Pressure
                    bool foundFlowMin = !string.IsNullOrWhiteSpace(ocrResponse.FlowRateMin);
                    var purgePressureSection = GetPurgePressureSection(messages[8], foundFlowMin);
                    ocrResponse.Battery = purgePressureSection.Item1;
                    if (!foundFlowMin)
                    {
                        ocrResponse.FlowRateMin = purgePressureSection.Item2;
                    }
                    ocrResponse.PurgePressure = purgePressureSection.Item3;
                    processPlacementSignalScreen = true;

                    ValidateOcrResponse(ocrResponse);
                }
            } catch (Exception EX)
            {
                var xxx = EX.Message; // TODO Remove - for working/tweaking the OCR porocessing.
            }
            return processPlacementSignalScreen;
        }

        private Dictionary<int, List<string>> GetMessageSegments(string rawOcrResponseText)
        {
            Dictionary<int, List<string>> messages = new Dictionary<int, List<string>>();
            for (int i = 0; i<9; i++)
            {
                messages.Add(i, new List<string>());
            }

            string plainMessage = StandardizeMessageFormat(new StringBuilder(rawOcrResponseText, rawOcrResponseText.Length * 2), _generalReplacements);

            // Bracket/Format the Quadrants...
            string[] messageSegments = plainMessage.Replace("zz","ZZ").Replace("ZZ", "ZZ_ALARMSTART_ZZ").Replace("Purge Pressure:", "Purge Pressure ").Replace("Purge Pressure ", "Purge Pressure\n").Replace("Purge Flow:", "Purge Flow ").Replace("Purge Flow ", "Purge Flow\n").Split(new string[] { "ZZ", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            int type = 0;
            foreach (string segment in messageSegments)
            {
                switch(segment)
                {
                    case AlarmStartKeyword:
                        type++;
                        break;
                    case PlacementSignalKeyword:
                        type = 4;
                        break;
                    case MotorCurrentKeyword:
                        type = 5;
                        break;
                    case ImpellaFlowKeyword:
                        type = 6;
                        break;
                    case PurgeFlowKeyword:
                        type = 7;
                        break;
                    case PurgePressureKeyword:
                        type = 8;
                        break;
                    default:
                        messages[type].Add(segment);
                        break;
                }
            }

            return messages;
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
            _numericFieldReplacements = LoadReplacementRules("ocrreplacementsnumericfield");
            _alarm1Point = LoadAlarmPoint("ocralarm1coordinates");
            _alarm2Point = LoadAlarmPoint("ocralarm2coordinates");
            _alarm3Point = LoadAlarmPoint("ocralarm3coordinates");
            _alarmCodeRed = new MagickColor(_configurationCache.GetConfigurationItem(ConfigurationSectionName, "ocralarmcodered"));
            _alarmCodeYellow = new MagickColor(_configurationCache.GetConfigurationItem(ConfigurationSectionName, "ocralarmcodeyellow"));
            _alarmCodeWhite = new MagickColor(_configurationCache.GetConfigurationItem(ConfigurationSectionName, "ocralarmcodewhite"));
            _alarmCodeColorMatchTolerance = new Percentage(int.Parse(_configurationCache.GetConfigurationItem(ConfigurationSectionName, "ocralarmcodematchtolerance")));
            _ocrDebugMode = bool.Parse(_configurationCache.GetConfigurationItem(ConfigurationSectionName, "ocrdebug"));
            _performanceLevelValidationValues = _configurationCache.GetConfigurationItem(ConfigurationSectionName, "ocrvalidationperformancelevel").Split('|').ToList();
            _aicSerialNumberPrefix = _configurationCache.GetConfigurationItem(ConfigurationSectionName, "ocrvalidationaicserialnumber").Split('|').ToList();
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

        // The Header Section in the screen is started by the Pump Type ... 
        // However, the way the OCR works if a 'section' has pixels that are higher it can read those first.
        // Since we support different versions of RL the text in one section can be higher resulting in the following Results:
        // 1) The 'Pump Type' is folllowed by 'Pump Serial Number' followed by AIC SN Folloiwed by AIC Softare then ZZ
        // 2) The AIC SN is followed by AIC SOftware which is followed by 'Pump Type' Followed by SN: # then ZZ
        private Tuple<string, string, string, string> GetHeaderSection(List<string> headerParts)
        {
            string pumpType = string.Empty;
            string pumpSerialNumber = string.Empty;
            string aicSerialNumber = string.Empty;
            string aicSoftwareVersion = string.Empty;

            bool foundAicSection = false;
            bool foundPumpSection = false;
            if (headerParts.Count > 1)
            {
                foreach (string part in headerParts)
                {
                    var cleanedPart = StandardizeMessageFormat(new StringBuilder(part.ToUpper()), _headerReplacements);

                    int firstAicPosition = cleanedPart.IndexOf(AicKeyword);
                    if (!foundAicSection && firstAicPosition > -1)
                    {
                        aicSerialNumber = GetAicSerialNumber(cleanedPart);
                        aicSoftwareVersion = GetAicSoftwareVersion(cleanedPart, aicSerialNumber);
                        foundAicSection = true;
                    }
                    else
                    {
                        // Are we processing a 'Pump' or is it 'AIC'
                        // AIC is not in the same row as the 'Pump info'
                        if (!foundPumpSection && string.IsNullOrWhiteSpace(pumpType))
                        {
                            pumpType = GetPumpType(cleanedPart);
                            pumpSerialNumber = GetPumpSerialNumber(cleanedPart);
                        }

                        foundPumpSection = (pumpSerialNumber != string.Empty || pumpType != string.Empty);
                    }
                }
            }
            else
            {
                var parsedHeader = GetHeaderSection(new StringBuilder(headerParts[0]));
                pumpType = parsedHeader.Item1;
                pumpSerialNumber = parsedHeader.Item2;
                aicSerialNumber = parsedHeader.Item3;
                aicSoftwareVersion = parsedHeader.Item4;
            }

            return new Tuple<string, string, string, string>(pumpType, pumpSerialNumber, aicSerialNumber, aicSoftwareVersion);
        }

        /// <summary>
        /// Header Section is Comprised of:
        /// - Pump Type
        /// - Pump Serial Number
        /// - Aic Serial Number
        /// - Aic Software Version
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        private Tuple<string, string, string, string> GetHeaderSection(StringBuilder section)
        {
            string ocrText = StandardizeMessageFormat(section, _headerReplacements);
            string pumpType = GetPumpType(ocrText);
            string pumpSerialNumber = string.Empty;

            // The Header Section in the screen is started by the Pump Type ... 
            // However, the way the OCR works if a 'section' has pixels that are higher it can read those first.
            // Since we support different versions of RL the text in one section can be higher resulting in the following Results:
            // 1) The 'Pump Type' is folllowed by 'Pump Serial Number' followed by AIC SN Folloiwed by AIC Softare then ZZ
            // 2) The AIC SN is followed by AIC SOftware which is followed by 'Pump Type' Followed by SN: # then ZZ

            if (!string.IsNullOrWhiteSpace(pumpType))
            {
                pumpSerialNumber = GetPumpSerialNumber(ocrText.Substring(pumpType.Length + 1));
            }

            string aicSerialNumber = GetAicSerialNumber(ocrText);
            if (aicSerialNumber == pumpSerialNumber)
            {
                aicSerialNumber = string.Empty;
            }
            string aicSoftwareVersion = GetAicSoftwareVersion(ocrText, aicSerialNumber);

            return new Tuple<string, string, string, string>(pumpType, pumpSerialNumber, aicSerialNumber, aicSoftwareVersion);
        }

        private Tuple<string, string, string> GetImpellaFlowSection(List<string> impellaFlowParts)
        {
            string flowMax = string.Empty;
            string flowMin = string.Empty;
            string flow = string.Empty;

            try
            {
                if (impellaFlowParts.Count == 1)
                {
                    var impellaFlow = GetImpellaFlowSection(new StringBuilder(impellaFlowParts[0]));
                    flowMax = impellaFlow.Item1;
                    flowMin = impellaFlow.Item2;
                    flow = impellaFlow.Item3;
                }
                else
                {
                    foreach (string part in impellaFlowParts)
                    {
                        bool usedPart = false;
                        string cleanedPart = StandardizeMessageFormat(new StringBuilder(part), _impellaFlowReplacements);

                        // MAX 
                        if (string.IsNullOrWhiteSpace(flowMax))
                        {
                            int maxPosition = cleanedPart.IndexOf(MaxKeyword);
                            if (maxPosition > 0)
                            {
                                flowMax = new string(part.Substring(0, maxPosition).Where(c => char.IsDigit(c) || c == '.').ToArray());
                                usedPart = true;
                            }
                        }

                        // MIN - Sometimes Min is in this section - about 50% of the time it is in Purge Pressure Section...  So we check here too.
                        // If we find it we want to pass a flag to the caller telling them we found it so we do not check for it again
                        if (string.IsNullOrWhiteSpace(flowMin) && !usedPart)
                        {
                            int minPosition = part.IndexOf(MinKeyword);
                            if (minPosition > 0)
                            {
                                flowMin = new string(part.Substring(0, minPosition).Where(c => char.IsDigit(c) || c == '.').ToArray());
                                usedPart = true;
                            }
                        }

                        if (string.IsNullOrWhiteSpace(flow) && !usedPart)
                        {
                            // Get the flow - it is the left over string
                            flow = new string(part.Where(c => char.IsDigit(c) || c == '.').ToArray());
                        }
                    }
                }
            }
            catch (Exception EX)
            {
                var xxx = EX.Message; // TODO Remove - for working/tweaking the OCR porocessing.
            }

            flowMax = FormatDouble(flowMax, ImpellaFlowValidationFormat);
            flowMin = FormatDouble(flowMin, ImpellaFlowValidationFormat);
            flow = FormatDouble(flow, ImpellaFlowValidationFormat);
            
            return new Tuple<string, string, string>(flowMax, flowMin, flow);
        }

        private string FormatDouble(string value, string format)
        {
            string formattedValue = value;
            if (General.IsValidDouble(value))
            {
                formattedValue = double.Parse(value).ToString(format);
            }

            return value;
        }

        private Tuple<string, string, string> GetImpellaFlowSection(StringBuilder section)
        {
            string flowMax = string.Empty;
            string flowMin = string.Empty;
            string flow = string.Empty;
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

        private string GetPurgeFlowSection(List<string> purgeFlowParts)
        {
            string purgeFlow = string.Empty;
            foreach(string part in purgeFlowParts)
            {
                purgeFlow = new string(part.Where(c => char.IsDigit(c) || c == '.').ToArray());
                break;
            }

            return purgeFlow;
        }

        private Tuple<string, string, string> GetPurgePressureSection(List<string> purgePressureParts, bool hasFlowMinBeenFound)
        {
            string flowMin = string.Empty;
            string purgePressure = string.Empty;
            string battery = string.Empty;

            if (purgePressureParts.Count == 1)
            {
                var purgePressureResults = GetPurgePressureToEndSection(new StringBuilder(purgePressureParts[0]), hasFlowMinBeenFound);
                battery = purgePressureResults.Item1;
                flowMin = purgePressureResults.Item2;
                purgePressure = purgePressureResults.Item3;
            }
            else
            {
                foreach (string part in purgePressureParts)
                {
                    bool usedPart = false;
                    string cleanedPart = StandardizeMessageFormat(new StringBuilder(part), _purgePressureReplacements);

                    // Check For Flow Min
                    if (!hasFlowMinBeenFound && string.IsNullOrWhiteSpace(flowMin))
                    {
                        int minPosition = cleanedPart.IndexOf(MinKeyword);
                        if (minPosition > -1)
                        {
                            flowMin = cleanedPart.Substring(0, minPosition).Trim();
                            usedPart = true;
                        }
                    }

                    // Battery
                    if (!usedPart && string.IsNullOrEmpty(battery))
                    {
                        int percentPosition = cleanedPart.IndexOf('%');
                        if (percentPosition > 0)
                        {
                            battery = cleanedPart.Substring(0, percentPosition).Trim();
                            usedPart = true;
                        }
                    }

                    // Purge Pressure (if it is not one of the above - then assume)
                    if (!usedPart && string.IsNullOrWhiteSpace(purgePressure))
                    {
                        purgePressure = new string(cleanedPart.Where(c => char.IsDigit(c) || c == '.').ToArray());
                    }
                }
            }

            return new Tuple<string, string, string>(battery, flowMin, purgePressure);
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
                    int endPosition = ocrText.Substring(pumpType.Length).IndexOf("SN:");
                    if (endPosition > -1)
                    {
                        result = ocrText.Substring(0, endPosition + pumpType.Length).Trim();
                    }
                    else
                    {
                        result = pumpType;
                    }

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
                        result = workString;
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

            if (!General.IsNumeric(result))
            {
                result = StandardizeMessageFormat(new StringBuilder(result), _numericFieldReplacements);
                if (!General.IsNumeric(result))
                {
                    result = string.Empty;
                }
            }

            return result;
        }

        private string GetAicSerialNumber(string ocrText)
        {
            string result = string.Empty;
            int startPos = ocrText.IndexOf(AicSnKeyword) + AicSnKeyword.Length + 1;

            if (startPos >= AicSnKeyword.Length + 1)
            {
                int endPos = ocrText.Substring(startPos).IndexOf(AicKeyword);
                if (endPos > 0)
                {
                    result = ocrText.Substring(startPos, endPos).Trim();
                }
            }

            if (_aicSerialNumberPrefix.Contains(result.Substring(0, 2)))
            {
                if (!General.IsNumeric(result.Substring(2)))
                {
                    string cleanResult =StandardizeMessageFormat(new StringBuilder(result.Substring(2)), _numericFieldReplacements);
                    if (General.IsNumeric(cleanResult))
                    {
                        result = string.Format("{0}{1}", result.Substring(0, 2), cleanResult);
                    }
                }
            }

            return result;
        }

        private string GetAicSoftwareVersion(string ocrText, string aicSerialNumber)
        {
            string result = string.Empty;

            int index = ocrText.IndexOf(aicSerialNumber);
            if (index > 0)
            {
                int startPosition = index + aicSerialNumber.Length;
                // find the next ' AIC ' chunk and take the rest
                int aicPos = ocrText.Substring(startPosition).IndexOf(AicSpaceKeyword);
                if (aicPos > -1)
                {
                    startPosition += (aicPos + AicSpaceKeyword.Length);
                }
                result = ocrText.Substring(startPosition);
            }

            return result;
        }

        private Tuple<string, string, string, string> GetPlacementSignal(List<string> parts)
        {
            string systole = string.Empty;
            string distole = string.Empty;
            string average = string.Empty;
            string pLevel = string.Empty;

            if (parts.Count > 1)
            {
                foreach (string part in parts)
                {
                    string cleanedPart = StandardizeMessageFormat(new StringBuilder(part), _placementReplacements);
                    bool usedPart = false;

                    // Is Systole/Distole Part - it has a '/'
                    if (string.IsNullOrWhiteSpace(systole))
                    {
                        int slash = part.IndexOf('/');
                        if (slash > 0)
                        {
                            systole = part.Substring(0, slash);
                            distole = part.Substring(slash + 1);
                            usedPart = true;
                        }
                    }

                    // Is the Average - has a '('
                    if (string.IsNullOrWhiteSpace(average) && !usedPart)
                    {
                        int openParenthesisPosition = part.IndexOf('(');
                        if (openParenthesisPosition > -1)
                        {
                            average = new string(part.Where(c => char.IsDigit(c)).ToArray());
                            usedPart = true;
                        }
                    }

                    // Performance Level - All But Above...
                    if (!usedPart)
                    {
                        pLevel = DeterminePLevelValue(part, pLevel, parts.Count);
                    }
                }
            }
            else
            {
                if (parts.Count == 1)
                {
                    var placementSignal = GetPlacementSignal(new StringBuilder(parts[0]));
                    systole = placementSignal.Item1;
                    distole = placementSignal.Item2;
                    average = placementSignal.Item3;
                    pLevel = placementSignal.Item4;
                }
            }

            return new Tuple<string, string, string, string>(systole, distole, average, pLevel.Replace('.', '-'));
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
                    if (piece.Length > 0)
                    {
                        switch (piece.ToUpper())
                        {
                            case "AUTO":
                            case "BOOST":
                            case "OFF":
                                pLevel = piece;
                                break;
                            default:
                                switch (piece.Substring(0, 1).ToUpper())
                                {
                                    case "P":
                                        pLevel = string.IsNullOrWhiteSpace(pLevel) ? piece : piece + pLevel;
                                        break;
                                    case ".":
                                    case "-":
                                        if (pieces.Length > 1)
                                        {
                                            if (piece.Substring(1).IndexOf('-') == -1)
                                            {
                                                pLevel += piece;
                                            }
                                        }
                                        break;
                                    case "(":
                                        average = new string(piece.Where(c => char.IsDigit(c)).ToArray());
                                        break;
                                    default:
                                        break;
                                }
                                break;
                        }
                    }
                }
            }

            return new Tuple<string, string, string, string>(systole, distole, average, pLevel.Replace('.','-'));
        }

        private string DeterminePLevelValue(string stringToEvaluate, string currentPLevelValue, int size)
        {
            string pLevel = string.Empty;

            if (stringToEvaluate.Length > 0)
            {
                string piece = stringToEvaluate.ToUpper();
                switch (piece)
                {
                    case "AUTO":
                    case "BOOST":
                    case "OFF":
                        pLevel = piece;
                        break;
                    default:
                        switch (piece.Substring(0, 1).ToUpper())
                        {
                            case "P":
                                pLevel = string.IsNullOrWhiteSpace(pLevel) ? piece : piece + pLevel;
                                break;
                            case ".":
                            case "-":
                                if (size > 1)
                                {
                                    if (stringToEvaluate.Substring(1).IndexOf('-') == -1)
                                    {
                                        pLevel += piece;
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                        break;
                }
            }

            return pLevel;
        }

        private Tuple<string, string, string> GetMotorCurrent(List<string> parts)
        {
            string systole = string.Empty;
            string distole = string.Empty;
            string average = string.Empty;

            if (parts.Count > 1)
            {
                foreach (string part in parts)
                {
                    string cleanedPart = StandardizeMessageFormat(new StringBuilder(part), _motorReplacements);
                    bool usedPart = false;

                    // Is Systole/Distole Part - it has a '/'
                    if (string.IsNullOrWhiteSpace(systole))
                    {
                        int slash = part.IndexOf('/');
                        if (slash > 0)
                        {
                            systole = part.Substring(0, slash);
                            distole = part.Substring(slash + 1);
                            usedPart = true;
                        }
                    }

                    // get the average
                    if (string.IsNullOrWhiteSpace(average) && !usedPart)
                    {
                        int openParenthesisPosition = part.IndexOf('(');
                        if (openParenthesisPosition > -1)
                        {
                            average = new string(part.Where(c => char.IsDigit(c)).ToArray());
                        }
                    }
                }
            }
            else
            {
                if (parts.Count == 1)
                {
                    var motorCurrent = GetMotorCurrent(new StringBuilder(parts[0]));
                    systole = motorCurrent.Item1;
                    distole = motorCurrent.Item2;
                    average = motorCurrent.Item3;
                }
            }

            return new Tuple<string, string, string>(systole, distole, average);
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
            if (pieces.Length > 0)
            {
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
                        if (pieces.Length > 1)
                        {
                            average = new string(piece.Where(c => char.IsDigit(c)).ToArray());
                        }
                    }
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

        private void ValidateOcrResponse(OcrResponse ocrResponse)
        {
            ocrResponse.Result.PumpTypeValid = !string.IsNullOrEmpty(ocrResponse.PumpType);
            ocrResponse.Result.PumpSerialNumberValid = IsValidPumpSerialNumber(ocrResponse.PumpSerialNumber);
            ocrResponse.Result.AicSerialNumberValid = IsValidAicSerialNumber(ocrResponse.AicSerialNumber);
            ocrResponse.Result.PerformanceLevelValid = IsValidPerformanceLevel(ocrResponse.PerformanceLevel);
            ocrResponse.Result.FlowRateMaxValid = General.IsValidDouble(ocrResponse.FlowRateMax);
            ocrResponse.Result.FlowRateMinValid = General.IsValidDouble(ocrResponse.FlowRateMin);
            ocrResponse.Result.FlowRateAverageValid = General.IsValidDouble(ocrResponse.FlowRateAverage);

            ocrResponse.Result.Success = (ocrResponse.Result.PumpTypeValid &&
                                            ocrResponse.Result.PumpSerialNumberValid &&
                                            ocrResponse.Result.AicSerialNumberValid &&
                                            ocrResponse.Result.PerformanceLevelValid &&
                                            ocrResponse.Result.FlowRateMaxValid &&
                                            ocrResponse.Result.FlowRateMinValid &&
                                            ocrResponse.Result.FlowRateAverageValid);

        }

        private bool IsValidPumpSerialNumber(string pumpSerialNumber)
        {
            return (pumpSerialNumber.Length < 7 && int.TryParse(pumpSerialNumber, out int n));
        }

        private bool IsValidPerformanceLevel(string performanceLevel)
        {
            if (_performanceLevelValidationValues.Contains(performanceLevel))
            {
                return true;
            }

            return false;
        }

        private bool IsValidAicSerialNumber(string aicSerialNumber)
        {
            if (aicSerialNumber.Length == 6)
            {
                if (_aicSerialNumberPrefix.Contains(aicSerialNumber.Substring(0, 2)))
                {
                    if (int.TryParse(aicSerialNumber.Substring(2), out int n))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion
    }
}