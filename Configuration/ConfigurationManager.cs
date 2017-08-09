using Abiomed.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abiomed.Configuration
{
    /// <summary>
    /// Cloud Table Storage uses the following structure.
    /// 1) PartitionKey - Grouping of data
    /// 2) RowKey - Unique Key within Group
    /// 3) Additional Feilds can be added.
    /// 
    /// In context to Configuration...
    /// The construct implemented is...
    /// - PartitionKey is equivalent to the Feature (or application)
    /// - Rowkey is the Configuration Item Key within the Partition
    /// - Then Value
    /// </summary>
    public class ConfigurationManager
    {
        private ITableStorage _iTableStorage;
        private string _tableContext;

        public ConfigurationManager()
        {
            SetTableContext("config"); // TODO Need to set from somewhere instead of HardCode.
            Initialize();
        }

        public ConfigurationManager(string tableContext)
        {
            SetTableContext(tableContext);
            Initialize();
        }

        public void SetTableContext(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentOutOfRangeException("Configuration Table Context cannot be null, empty, or whitespace.");
            }

            _tableContext = tableName;
        }

        /// <summary>
        /// In Azure Terms this is a point query and known as the most efficient.
        /// </summary>
        /// <param name="featureKey">In Azure Terms this is the PartitionKey</param>
        /// <param name="itemKey">In Azure Terms this is the RowKey</param>
        /// <returns></returns>
        public async Task<ApplicationConfiguration> GetItemAsync(string featureKey, string itemKey)
        {
            return await _iTableStorage.GetItemAsync<ApplicationConfiguration>(featureKey, itemKey, _tableContext);
        }

        public async Task<List<ApplicationConfiguration>> GetFeatureAsync(string featureKey)
        {
           return await _iTableStorage.GetPartitionItemsAsync<ApplicationConfiguration>(featureKey, _tableContext);
        }

        public List<ApplicationConfiguration> GetFeature(string featureKey)
        {
            return _iTableStorage.GetPartitionItems<ApplicationConfiguration>(featureKey, _tableContext);
        }


        public async Task<List<ApplicationConfiguration>> GetAllAsync()
        {
            return await _iTableStorage.GetAllAsync<ApplicationConfiguration>(_tableContext);
        }

        public List<ApplicationConfiguration> GetAll()
        {
            return  _iTableStorage.GetAll<ApplicationConfiguration>(_tableContext);
        }

        public async Task StoreConfigurationItemsAsync(List<ApplicationConfiguration> configurationItems, string configurationContext = null)
        {
            if (configurationItems == null)
            {
                throw new ArgumentNullException("ConfigurationItems cannot be null.");
            }

            if (configurationItems.Count == 0)
            {
                throw new ArgumentOutOfRangeException("ConfigurationItems cannot be an empty list.");
            }

            if (!string.IsNullOrWhiteSpace(configurationContext))
            {
                _tableContext = configurationContext;
            }

            if (string.IsNullOrWhiteSpace(_tableContext))
            {
                throw new ArgumentOutOfRangeException("Configuration Table Context cannot be null, empty, or whitespace.");
            }

            foreach (ApplicationConfiguration item in configurationItems)
            {
                await _iTableStorage.InsertAsync(_tableContext, item);
            }
        }

        public async Task LoadFactoryConfiguration()
        {
            // TODO These will need to be mpoved to an XML file or something similar.
            await AddConfigurationItem(@"connectionmanager", @"run", @"localhost");
            await AddConfigurationItem(@"connectionmanager", @"wowza", @"rtmp://live-remotelink.channel.mediaservices.windows.net:1935/live/584ea49698714ec4872ba4db0b44fc6");
            await AddConfigurationItem(@"connectionmanager", @"web", @"10.11.0.19");
            await AddConfigurationItem(@"connectionmanager", @"rlr", @"10.11.0.12");
            await AddConfigurationItem(@"connectionmanager", @"docdburi", @"https://abmd.documents.azure.com:443/");
            await AddConfigurationItem(@"connectionmanager", @"docdbpwd", @"7KA3x3At9kGf8ZGo98xHx7d3h1G3nZw7HqxY3FhQKnZdZayrkal7gIPMK9FKf39UisSUxcWdLAfkjtDRZhlXtQ==");
            await AddConfigurationItem(@"connectionmanager", @"redisconnect", @"abmd.redis.cache.windows.net, abortConnect=false,ssl=true,password=6sfN8jGyo0al+dMDdML4KMt0f59lCuqX0Wk9FJfxwPw=");
            await AddConfigurationItem(@"connectionmanager", @"security", @"false");

            await AddConfigurationItem(@"optionsmanager", @"keepalivetimer", @"600000");
            await AddConfigurationItem(@"optionsmanager", @"certkey", @"C:\Certs\RLR.abiomed.com\rlr.abiomed.com.pfx");
            await AddConfigurationItem(@"optionsmanager", @"tcpport", @"443");
            await AddConfigurationItem(@"optionsmanager", @"imagecountdowntimer", @"60000000");
        }

        private async Task AddConfigurationItem(string partitionKey, string rowKey, string value)
        {
            ApplicationConfiguration appConfig = new ApplicationConfiguration();
            appConfig.PartitionKey = partitionKey;
            appConfig.RowKey = rowKey;
            appConfig.Value = value;
            await _iTableStorage.InsertOrMergeAsync(_tableContext, appConfig);
        }

        private void Initialize()
        {
            _iTableStorage = new TableStorage();
        }
    }
}