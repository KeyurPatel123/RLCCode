using System;
using System.Collections.Generic;
using System.Text;
using Abiomed.DotNetCore.Configuration;
using Abiomed.DotNetCore.Repository;
using Abiomed.DotNetCore.Models;
using Microsoft.Azure.Documents.Client;

namespace Abiomed.DotNetCore.Business
{
    public class InstitutionManager : IInstitutionManager
{
        #region Private Member Variables

        private IConfigurationCache _configurationCache;
        private IAzureCosmosDB _azureCosmosDB;
        private string _databaseName;
        private string _collectionName;
        private Uri _uri;

        #endregion

        #region Constructors 

        public InstitutionManager(IConfigurationCache configurationCache)
        {
            _configurationCache = configurationCache;
            Initialize();
        }

        private void Initialize()
        {
            _databaseName = _configurationCache.GetConfigurationItem("azurecosmos", "coredatabasename");
            _collectionName = _configurationCache.GetConfigurationItem("azurecosmos", "institutioncollectionname");

            _azureCosmosDB = new AzureCosmosDB(_configurationCache);
            _azureCosmosDB.SetContext(_databaseName, _collectionName);
            _uri = UriFactory.CreateDocumentCollectionUri(_databaseName, _collectionName);
        }

        #endregion

        #region public Methods

        public List<Institution> GetInstitutions()
        {
            return _azureCosmosDB.ExecuteQuery<Institution>(_uri, _collectionName, string.Empty);
        }

        public List<Institution> GetInstitution(string sapCustomerId)
        {
            return _azureCosmosDB.ExecuteQuery<Institution>(_uri, _collectionName, string.Format("WHERE Institution.SapCustomerId = '{0}'", sapCustomerId));
        }

        #endregion
    }
}