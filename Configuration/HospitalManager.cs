using Abiomed.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SODA;
using System.Threading;
using Microsoft.WindowsAzure.Storage;

namespace Abiomed.Configuration
{
    public class HospitalManager
    {
        private ITableStorage _iTableStorage;
        private string _tableContext;
        private const string SodaHospitalGeneralInformationClientUrl = @"https://data.medicare.gov";
        private const string SodaHospitalGeneralInformationEndpointName = @"rbry-mqwu";
        private const string SodaHospitalGeneralInformationName = @"Hospital General Information";

        #region Public Methods

        #region Constructors
        public HospitalManager(string tableContext)
        {
            SetTableContext(tableContext); 
            Initialize();
        }

        public void SetTableContext(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentOutOfRangeException("Hospital Table Context cannot be null, empty, or whitespace.");
            }

            _tableContext = tableName;
        }

        #endregion

        #region Get methods

        public async Task<List<HospitalConfiguration>> GetAllAsync()
        {
            return await _iTableStorage.GetAllAsync<HospitalConfiguration>(_tableContext);
        }

        public List<HospitalConfiguration> GetAll()
        {
            return _iTableStorage.GetAll<HospitalConfiguration>(_tableContext);
        }

        public List<HospitalConfiguration> GetCountry(string countryIsoCode)
        {
            return _iTableStorage.GetPartitionItems<HospitalConfiguration>(countryIsoCode, _tableContext);
        }

        public HospitalConfiguration Get(string countryIsoCode, string providerId)
        {
            return _iTableStorage.GetItem<HospitalConfiguration>(countryIsoCode, providerId, _tableContext);
        }

        public async Task<List<HospitalConfiguration>> GetCountryAsync(string countryIsoCode)
        {
            return await _iTableStorage.GetPartitionItemsAsync<HospitalConfiguration>(countryIsoCode, _tableContext);
        }

        public async Task<HospitalConfiguration> GetAsync(string countryIsoCode, string providerId)
        {
            return await _iTableStorage.GetItemAsync<HospitalConfiguration>(countryIsoCode, providerId, _tableContext);
        }

        #endregion

        #region Store Methods

        public async Task LoadHospitalFactoryDataAsync()
        {
            MetadataManager metadataManager = new MetadataManager();
            var metadataCloud = metadataManager.Get("Configuration", SodaHospitalGeneralInformationName);
            SodaClient client = new SodaClient(SodaHospitalGeneralInformationClientUrl);
            var metadata = client.GetMetadata(SodaHospitalGeneralInformationEndpointName);

            if (!(metadataCloud.RowKey == metadata.Name && metadataCloud.RowsLastUpdated == metadata.RowsLastUpdated.ToString() && metadataCloud.SchemaLastUpdated == metadata.SchemaLastUpdated.ToString()))
            {
                await metadataManager.AddMetadataAsync(metadata.Name, metadata.Description, metadata.RowsLastUpdated.ToString(), metadata.SchemaLastUpdated.ToString());
                await _iTableStorage.DropAsync(_tableContext); // Clear out existing information
                var dataset = client.GetResource<Hospital>(SodaHospitalGeneralInformationEndpointName);

                // Resource objects read their own data
                var hospitals = dataset.GetRows();
                foreach (Hospital hospital in hospitals)
                {
                    await AddHospitalAsync(hospital);
                }
            }
        }

        #endregion

        #endregion

        #region Private Members

        private async Task AddMetadata()
        {

        }

        private async Task AddHospitalAsync(Hospital hospital, string countryCode = "USA")
        {
            int retryCount = 0;

            HospitalConfiguration hospitalConfiguration = new HospitalConfiguration();

            hospitalConfiguration.PartitionKey = countryCode;
            hospitalConfiguration.RowKey = hospital.ProviderId ;
            hospitalConfiguration.HospitalName = hospital.HospitalName;
            hospitalConfiguration.Address = hospital.Address;
            hospitalConfiguration.City = hospital.City;
            hospitalConfiguration.County = hospital.County;
            hospitalConfiguration.PhoneNumber = hospital.PhoneNumber;
            hospitalConfiguration.State = hospital.State;
            hospitalConfiguration.ZipCode = hospital.ZipCode;

            if (hospital.HospitalPointCoordinates != null &&
                    hospital.HospitalPointCoordinates.Coordinates != null &&
                    hospital.HospitalPointCoordinates.Coordinates.Count == 2)
            {
                hospitalConfiguration.HospitalLongitude = hospital.HospitalPointCoordinates.Coordinates[0];
                hospitalConfiguration.HospitalLatitude = hospital.HospitalPointCoordinates.Coordinates[1];
            }

            bool retry = true;
            do
            {
                
                try
                {
                    await _iTableStorage.InsertOrMergeAsync(_tableContext, hospitalConfiguration);
                    retry = false;
                }
                catch (StorageException e)
                {
                    retryCount++;
                    if ((e.RequestInformation.HttpStatusCode == 409) && retryCount < 120)
                    {
                        Thread.Sleep(1000);// The table is currently being deleted. Try again until it works.
                    }
                    else
                    {
                        throw;
                    }
                }
            } while (retry);
        }

        private void Initialize()
        {
            _iTableStorage = new TableStorage();
        }

        #endregion
    }
}
