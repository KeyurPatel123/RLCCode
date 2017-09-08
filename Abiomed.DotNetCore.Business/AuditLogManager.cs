using System;
using System.Threading.Tasks;
using Abiomed.DotNetCore.Storage;
using Abiomed.Models;
using Abiomed.DotNetCore.Configuration;

namespace Abiomed.DotNetCore.Business
{
    public class AuditLogManager : IAuditLogManager
    {
        #region Private Member Variables 

        private ITableStorage _iTableStorage;
        private string _auditTableName;
        private IConfigurationCache _configurationCache; 

        #endregion

        #region Constructors

        public AuditLogManager(string tableName)
        {
            Initialize(tableName);
        }

        public AuditLogManager(ITableStorage tableStorage, IConfigurationCache configurationCache)
        {
            _configurationCache = configurationCache;
            _iTableStorage = tableStorage;

            _auditTableName = string.Empty;

            Initialize();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates an Audit Log Entry
        /// </summary>
        /// <param name="userName">The UserName of the user logged in</param>
        /// <param name="logTime">The time (UTC) of the event</param>
        /// <param name="ipAddress">The User's IP Address</param>
        /// <param name="action">The Audit Action</param>
        /// <param name="message">The Audit Result/message</param>
        /// <returns></returns>
        public async Task AuditAsync(string userName, DateTime logTime, string ipAddress, string action, string message)
        {
            AuditLog auditLog = BuildAuditLogItem(userName, logTime, ipAddress, action, message);
            await _iTableStorage.InsertAsync(_auditTableName, auditLog);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Builds an Audit Log Entry
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="logTime"></param>
        /// <param name="ipAddress"></param>
        /// <param name="action"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private AuditLog BuildAuditLogItem(string userName, DateTime logTime, string ipAddress, string action, string message)
        {
            return new AuditLog
            {
                PartitionKey = userName,
                RowKey = logTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                IpAddress = ipAddress,
                Action = action,
                Message = message
            };
        }

        /// <summary>
        /// Shared Constructor Logic
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="storageConnection"></param>
        private void Initialize()
        {
            _iTableStorage.SetTableContextAsync(_auditTableName).Wait();
        }

        private void Initialize(string tableName)
        {
            _iTableStorage.SetTableContextAsync(tableName).Wait();
        }

        #endregion
    }
}