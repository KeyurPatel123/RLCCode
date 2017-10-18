using System;
using System.Collections.Generic;
using Abiomed.DotNetCore.Configuration;
using Abiomed.DotNetCore.Repository;
using Abiomed.DotNetCore.Models;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Globalization;

namespace Abiomed.DotNetCore.Business
{
    public class CaseManager : ICaseManager
    {
        #region Private Member Variables

        private const string LongDateFormat = "yyyy-MM-dd HH:mm:ss.fff"; // TODO - Move this to a common place

        private ConfigurationCache _configurationCache;
        private IRedisDbRepository<Case> _redisDbRepositoryCase;
        private RemoteLinkCases _remoteLinkCases = new RemoteLinkCases();

        #endregion

        #region Constructors 

        public CaseManager(ConfigurationCache configurationCache, IRedisDbRepository<Case> redisDbRepositoryCase)
        {
            _configurationCache = configurationCache;
            _redisDbRepositoryCase = redisDbRepositoryCase;
            Initialize();
        }

        #endregion

        #region Server Side

        #region public Methods

        public static void CleanupExpiredCases(IRedisDbRepository<Case> redisDbRepositoryCase, DateTime expireTimeBeforeUtc)
        {
            var keys = redisDbRepositoryCase.GetKeys();
            foreach (var key in keys)
            {
                redisDbRepositoryCase.StringDelete(key);
            }
            redisDbRepositoryCase.Publish(Definitions.CleanupRemoteLinkCases, expireTimeBeforeUtc.ToString(LongDateFormat, CultureInfo.InvariantCulture));
        }

        public static void AddOrUpdate(IRedisDbRepository<Case> redisDbRepositoryCase, OcrResponse ocrResponse)
        {
            Case activeCase = new Case();
            try
            {
                activeCase = redisDbRepositoryCase.StringGet(ocrResponse.PumpSerialNumber);

            } catch(Exception EX)
            {
                var yyy = EX.Message; // TODO - Remove (Interim Testing Only
            }

            try
            {
                // Add/Update return new entry to store. 
                activeCase = (activeCase == null || activeCase.LastActiveUtc == DateTime.MinValue) ? CreateCase(ocrResponse) : UpdateCase(ocrResponse, activeCase);

                // Add entry back to the redis repository...
                redisDbRepositoryCase.StringSet(ocrResponse.PumpSerialNumber, activeCase);
                redisDbRepositoryCase.Publish(Definitions.UpdatedRemoteLinkCase, ocrResponse.PumpSerialNumber);
            }
            catch (Exception EX)
            {
                var ttt = EX.Message; // TODO - Remove (Interim Testing Only
            }
        }

        #endregion

        #region Private Methods

        private static Case UpdateCase(OcrResponse ocrResponse, Case activeCase)
        {
            DateTime updatedUtc = DateTime.UtcNow;

            // Performance Level is a change in which the UI needs to be 'notified'.
            if (ocrResponse.PerformanceLevel != activeCase.PerformanceLevel)
            {
                Tuple<DateTime, string> performanceLevel = new Tuple<DateTime, string>(activeCase.LastUpdateUtc, activeCase.PerformanceLevel);
                activeCase.PerformanceLevelHistory.Add(performanceLevel);
                activeCase.PerformanceLevel = ocrResponse.PerformanceLevel;
                activeCase.LastUpdateUtc = updatedUtc;
                activeCase.Updated = true;
            }

            // Alarms is a change in which the UI needs to be 'notified'.
            if ((ocrResponse.Alarm1 != activeCase.Alarm1.Type || ocrResponse.Alarm1Message != activeCase.Alarm1.Description) ||
                (ocrResponse.Alarm2 != activeCase.Alarm2.Type || ocrResponse.Alarm2Message != activeCase.Alarm2.Description) ||
                (ocrResponse.Alarm3 != activeCase.Alarm3.Type || ocrResponse.Alarm3Message != activeCase.Alarm3.Description))
            {
                Tuple<DateTime, Alarm, Alarm, Alarm> alarms = new Tuple<DateTime, Alarm, Alarm, Alarm>(activeCase.LastUpdateUtc, activeCase.Alarm1, activeCase.Alarm2, activeCase.Alarm3);
                activeCase.AlarmHistory.Add(alarms);
                activeCase.Alarm1 = new Alarm { Type = ocrResponse.Alarm1, Description = ocrResponse.Alarm1Message };
                activeCase.Alarm2 = new Alarm { Type = ocrResponse.Alarm2, Description = ocrResponse.Alarm2Message };
                activeCase.Alarm3 = new Alarm { Type = ocrResponse.Alarm3, Description = ocrResponse.Alarm3Message };
                activeCase.LastUpdateUtc = updatedUtc;
                activeCase.Updated = true;
            }

            // Changes to the Impella Flow do not need to 'notify' the UI.
            if (ocrResponse.FlowRateAverage != activeCase.ImpellaFlow.Avg ||
                ocrResponse.FlowRateMax != activeCase.ImpellaFlow.Max ||
                ocrResponse.FlowRateMin != activeCase.ImpellaFlow.Min)
            {
                Tuple<DateTime, ImpellaFlow> impellaFlow = new Tuple<DateTime, ImpellaFlow>(activeCase.LastUpdateUtc, activeCase.ImpellaFlow);
                activeCase.ImpellaFlowHistory.Add(impellaFlow);
                activeCase.ImpellaFlow.Avg = ocrResponse.FlowRateAverage;
                activeCase.ImpellaFlow.Max = ocrResponse.FlowRateMax;
                activeCase.ImpellaFlow.Min = ocrResponse.FlowRateMin;
            }

            activeCase.LastActiveUtc = updatedUtc;
            return activeCase;
        }

        private static Case CreateCase(OcrResponse ocrResponse)
        {
            DateTime timeCreated = DateTime.UtcNow;

            return new Case
            {
                RemoteLinkSerialNumber = ocrResponse.SerialNumber,
                PumpSerialNumber = ocrResponse.PumpSerialNumber,
                PumpType = ocrResponse.PumpType,
                AicSerialNumber = ocrResponse.AicSerialNumber,
                AicSoftwareVersion = ocrResponse.AicSoftwareVersion,
                PerformanceLevel = ocrResponse.PerformanceLevel,
                PerformanceLevelHistory = new List<Tuple<DateTime, string>>(),
                Alarm1 = new Alarm { Type = ocrResponse.Alarm1, Description = ocrResponse.Alarm1Message },
                Alarm2 = new Alarm { Type = ocrResponse.Alarm2, Description = ocrResponse.Alarm2Message },
                Alarm3 = new Alarm { Type = ocrResponse.Alarm3, Description = ocrResponse.Alarm3Message },
                AlarmHistory = new List<Tuple<DateTime, Alarm, Alarm, Alarm>>(),
                ImpellaFlow = new ImpellaFlow { Min = ocrResponse.FlowRateMin, Max = ocrResponse.FlowRateMax, Avg = ocrResponse.FlowRateAverage },
                ImpellaFlowHistory = new List<Tuple<DateTime, ImpellaFlow>>(),
                ConnectionStartUtc = timeCreated,
                LastActiveUtc = timeCreated,
                LastUpdateUtc = timeCreated,
                Updated = true
            };
        }
        #endregion

        #endregion

        #region Client Side

        #region Public

        /// <summary>
        /// Get a specific case by the Pump Serial Number
        /// </summary>
        /// <param name="PumpSerialNumber">Pump Serial Number</param>
        /// <returns>The Active Case</returns>
        public Case GetCase(string pumpSerialNumber)
        {
            _remoteLinkCases.Cases.TryGetValue(pumpSerialNumber, out Case theCase);
            return theCase;
        }

        /// <summary>
        /// Gets all the Active Cases
        /// </summary>
        /// <returns>List of Cases</returns>
        public RemoteLinkCases GetAll()
        {
            return _remoteLinkCases;
        }

        /// <summary>
        /// Gets the Cases with Updates
        /// </summary>
        /// <returns></returns>
        public RemoteLinkCases GetUpdated()
        {
            RemoteLinkCases onlyUpdatedCases = new RemoteLinkCases();
            foreach (var item in _remoteLinkCases.Cases)
            {
                if (item.Value.Updated)
                {
                    onlyUpdatedCases.Cases.AddOrUpdate(item.Key, item.Value, (key, oldValue) => item.Value);
                }
            }
            return onlyUpdatedCases;
        }

        /// <summary>
        /// Gets the Cases with Updates
        /// </summary>
        /// <returns></returns>
        public RemoteLinkCases GetUpdated(DateTime lastTimeCheckedUtc)
        {
            RemoteLinkCases onlyUpdatedCases = new RemoteLinkCases();
            foreach (var item in _remoteLinkCases.Cases)
            {
                if (item.Value.LastUpdateUtc > lastTimeCheckedUtc)
                {
                    onlyUpdatedCases.Cases.AddOrUpdate(item.Key, item.Value, (key, oldValue) => item.Value);
                }
            }
            return onlyUpdatedCases;
        }

        #endregion

        #endregion

        #region Private Methods 

        private void Initialize()
        {
            _redisDbRepositoryCase.Subscribe(Definitions.UpdatedRemoteLinkCase, (channel, message) =>
            {
                var theCase = GetCase((string)JsonConvert.DeserializeObject<string>(message));
                _remoteLinkCases.Cases.AddOrUpdate(theCase.PumpSerialNumber, theCase, (key, oldValue) => theCase);
            });

            _redisDbRepositoryCase.Subscribe(Definitions.CleanupRemoteLinkCases, (channel, message) =>
            {
                var expiredTimeUtc = (DateTime)JsonConvert.DeserializeObject<DateTime>(message);
                foreach (var item in _remoteLinkCases.Cases)
                {
                    if (item.Value.LastActiveUtc < expiredTimeUtc)
                    {
                        _remoteLinkCases.Cases.Remove(item.Key, out Case caseBeingDeleted);
                    }
                }
            });
        }

        #endregion
    }
}