using System;
using System.Collections.Generic;
using Abiomed.DotNetCore.Configuration;
using Abiomed.DotNetCore.Repository;
using Abiomed.DotNetCore.Models;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Globalization;
using System.Linq;

namespace Abiomed.DotNetCore.Business
{
    public class CaseManager : ICaseManager
    {
        #region Private Member Variables
        private ConfigurationCache _configurationCache;
        private IRedisDbRepository<Case> _redisDbRepositoryCase;
        private RemoteLinkCases _remoteLinkCases = new RemoteLinkCases();
        #endregion

        #region Constructor
        public CaseManager(ConfigurationCache configurationCache, IRedisDbRepository<Case> redisDbRepositoryCase)
        {
            _configurationCache = configurationCache;
            _redisDbRepositoryCase = redisDbRepositoryCase;
            Initialize();
        }
        #endregion

        #region Server Side

        #region Private Methods 

        private void Initialize()
        {
            _redisDbRepositoryCase.Subscribe(Definitions.UpdatedRemoteLinkCases, async (channel, message) =>
            {
                var RlmDevices = (List<string>)JsonConvert.DeserializeObject<List<string>>(message);
                await GetCases(RlmDevices);

                CleanupExpiredCases();                
            });
        }

        private Task GetCases(List<string> RlmDevices)
        {
            var tasks = RlmDevices.Select(i =>
                {
                    return AddOrUpdate(i);
                });
            return Task.WhenAll(tasks);
        }

        private async Task AddOrUpdate(string RlmDevice)
        {
            try
            {
                OcrResponse ocrResponse = JsonConvert.DeserializeObject<OcrResponse>(await _redisDbRepositoryCase.StringGetBaseAsync(RlmDevice + ":OCR", true));
                
                if (ocrResponse.PumpSerialNumber != string.Empty)
                {
                    Case activeCase = new Case();
                    activeCase = await _redisDbRepositoryCase.StringGetAsync(ocrResponse.PumpSerialNumber);

                    // Add/Update return new entry to store. 
                    activeCase = (activeCase == null || activeCase.LastActiveUtc == DateTime.MinValue) ? CreateCase(ocrResponse) : UpdateCase(ocrResponse, activeCase);

                    // Add entry back to the redis repository...
                    await _redisDbRepositoryCase.StringSetAsync(ocrResponse.PumpSerialNumber, activeCase);

                    // Add/Update active case to list
                    _remoteLinkCases.Cases.AddOrUpdate(activeCase.PumpSerialNumber, activeCase, (key, oldValue) => activeCase);
                }
            }
            catch(Exception e)
            {
                
            }
        }

        private void CleanupExpiredCases()
        {
            DateTime dateTime = DateTime.UtcNow.AddHours(-4);

            Parallel.ForEach(_remoteLinkCases.Cases, checkedCase =>
            {
                if (checkedCase.Value.LastActiveUtc < dateTime)
                {
                    // todo save case to Storage???
                    _remoteLinkCases.Cases.TryRemove(checkedCase.Value.PumpSerialNumber, out Case caseBeingDeleted);
                }
            });            
        }
        #endregion

        #region Private Methods

        private Case UpdateCase(OcrResponse ocrResponse, Case activeCase)
        {
            DateTime updatedUtc = DateTime.UtcNow;
            
            // Alarms is a change in which the UI needs to be 'notified'.
            if ((ocrResponse.Alarm1 != activeCase.Alarm1.Type || ocrResponse.Alarm1Message != activeCase.Alarm1.Description) ||
                (ocrResponse.Alarm2 != activeCase.Alarm2.Type || ocrResponse.Alarm2Message != activeCase.Alarm2.Description) ||
                (ocrResponse.Alarm3 != activeCase.Alarm3.Type || ocrResponse.Alarm3Message != activeCase.Alarm3.Description))
            {
                Tuple<DateTime, Alarm> alarms = new Tuple<DateTime, Alarm>(activeCase.LastUpdateUtc, activeCase.Alarm1);
                activeCase.AlarmHistory.Add(alarms);

                alarms = new Tuple<DateTime, Alarm>(activeCase.LastUpdateUtc, activeCase.Alarm2);
                activeCase.AlarmHistory.Add(alarms);

                alarms = new Tuple<DateTime, Alarm>(activeCase.LastUpdateUtc, activeCase.Alarm3);
                activeCase.AlarmHistory.Add(alarms);

                // Figure out change of state and add to alertHistory, prior to overriding old values
                NewAlarm(ocrResponse.Alarm1, ocrResponse.Alarm1Message, activeCase, updatedUtc);
                NewAlarm(ocrResponse.Alarm2, ocrResponse.Alarm2Message, activeCase, updatedUtc);
                NewAlarm(ocrResponse.Alarm3, ocrResponse.Alarm3Message, activeCase, updatedUtc);

                activeCase.Alarm1 = new Alarm { Type = ocrResponse.Alarm1, Description = ocrResponse.Alarm1Message };
                activeCase.Alarm2 = new Alarm { Type = ocrResponse.Alarm2, Description = ocrResponse.Alarm2Message };
                activeCase.Alarm3 = new Alarm { Type = ocrResponse.Alarm3, Description = ocrResponse.Alarm3Message };
                activeCase.LastUpdateUtc = updatedUtc;
                
                activeCase.Updated = true;
            }

            // Performance Level is a change in which the UI needs to be 'notified'.
            if (ocrResponse.PerformanceLevel != activeCase.PerformanceLevel)
            {
                activeCase.AlertSummary.Add(new AlertSummary()
                {
                    Time = updatedUtc,
                    Type = "PLevel",
                    Message = string.Format("From {0} to {1}", activeCase.PerformanceLevel, ocrResponse.PerformanceLevel)
                });

                Tuple<DateTime, string> performanceLevel = new Tuple<DateTime, string>(activeCase.LastUpdateUtc, activeCase.PerformanceLevel);
                activeCase.PerformanceLevelHistory.Add(performanceLevel);
                activeCase.PerformanceLevel = ocrResponse.PerformanceLevel;
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

        private void NewAlarm(string alarmType, string alarmDescription, Case activeCase, DateTime updatedUtc)
        {
            bool found = false;

            // Check all active Alarms
            if((alarmType == activeCase.Alarm1.Type && alarmDescription == activeCase.Alarm1.Description) ||
               (alarmType == activeCase.Alarm2.Type && alarmDescription == activeCase.Alarm2.Description) ||
               (alarmType == activeCase.Alarm3.Type && alarmDescription == activeCase.Alarm3.Description))
            {
                found = true;
            }
                        
            if(!found)
            {
                activeCase.AlertSummary.Add(new AlertSummary()
                {
                    Time = updatedUtc,
                    Type = alarmType                    
                });
            }
        }

        private Case CreateCase(OcrResponse ocrResponse)
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
                AlarmHistory = new List<Tuple<DateTime, Alarm>>(),
                ImpellaFlow = new ImpellaFlow { Min = ocrResponse.FlowRateMin, Max = ocrResponse.FlowRateMax, Avg = ocrResponse.FlowRateAverage },
                ImpellaFlowHistory = new List<Tuple<DateTime, ImpellaFlow>>(),
                ConnectionStartUtc = timeCreated,
                LastActiveUtc = timeCreated,
                LastUpdateUtc = timeCreated,
                AlertSummary = new List<AlertSummary>(),
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


    }
}