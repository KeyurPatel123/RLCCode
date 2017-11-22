using Abiomed.DotNetCore.Configuration;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Abiomed.DotNetCore.Repository
{
    public class AzureCosmosDB : IAzureCosmosDB
    {
        #region Private Member Variables

        private const string UriNotSetInvalidOperationException = "Uri for Azure Cosmos Database not specified. Call SetContext to specify Uri.";
        private const string DatabaseNameCannotBeEmptyNullOrWhitespace = "Database Name cannot be null, empty, or whitespace.";
        private const string CollectionNameCannotBeEmptyNullOrWhitespace = "Collection Name cannot be null, empty, or whitespace.";
        private const string payloadNotspecifiedInvalidOperationException = "Cannot add a null object";

        private IConfigurationCache _configurationCache;
        private string _endpointUri = string.Empty; 
        private string _primaryKey = string.Empty; 
        private Uri _uri;
        private DocumentClient _client;
        FeedOptions _queryOptions;


        #endregion

        #region Constructors

        public AzureCosmosDB(IConfigurationCache configurationCache)
        {
            _configurationCache = configurationCache;

            Initialize();
        }

        #endregion

        #region Public methods

        public void SetContext(string databaseName, string collectionName)
        {
            if (string.IsNullOrWhiteSpace(databaseName))
            {
                throw new ArgumentOutOfRangeException(DatabaseNameCannotBeEmptyNullOrWhitespace);
            }

            if (string.IsNullOrWhiteSpace(collectionName))
            {
                throw new ArgumentOutOfRangeException(CollectionNameCannotBeEmptyNullOrWhitespace);
            }

            _uri = UriFactory.CreateDocumentCollectionUri(databaseName, collectionName);
        }

        public async Task AddAsync<T>(T payload)
        {
            if (_uri == null)
            {
                throw new System.InvalidOperationException(UriNotSetInvalidOperationException);
            }

            if (payload == null)
            {
                throw new System.InvalidOperationException(payloadNotspecifiedInvalidOperationException);
            }
            try
            {
                await _client.CreateDocumentAsync(_uri, payload);
            }
            catch (Exception EX)
            {
                string sss = EX.Message;
            }
        }

        public List<T> ExecuteQuery<T>(Uri documentCollectionUri, string collectionName, string where)
        {
            return _client.CreateDocumentQuery<T>(
                documentCollectionUri,
                string.Format("SELECT * FROM {0} {1}", collectionName, where),
                _queryOptions).ToList();
        }

        public List<T> ExecuteQuery<T>(string databaseName, string collectionName, string where)
        {
            return _client.CreateDocumentQuery<T>(
                UriFactory.CreateDocumentCollectionUri(databaseName, collectionName),
                string.Format("SELECT * FROM {0} {1}", collectionName, where),
                _queryOptions).ToList();
        }

        #endregion

        #region Private Methods 

        private void Initialize()
        {
            _endpointUri = _configurationCache.GetConfigurationItem("azurecosmosdb", "endpointuri");
            _primaryKey = _configurationCache.GetConfigurationItem("azurecosmosdb", "primarykey");

            _client = new DocumentClient(new Uri(_endpointUri), _primaryKey);
            _queryOptions = new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true};
        }

        #endregion 
    }
}
