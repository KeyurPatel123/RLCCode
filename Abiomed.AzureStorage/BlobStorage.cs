using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using Abiomed.DotNetCore.Common;

namespace Abiomed.DotNetCore.Storage
{
    /// <summary>
    /// Class Implementation to support Blob Storage
    /// </summary>
    public class BlobStorage:IBlobStorage
    {
        #region Private Member Variables

        private const string payloadToUploadCannotBeNull = @"Payload to upload cannot be null.";
        private const string payloadToUploadCannotBeEmpty = @"Payload to upload cannot be empty.";
        private const string blobNamecannotBeEmpty = @"BlobName cannot be null, empty, or whitespace.";
        private const string metadataCannotBeNull = @"metaData cannot be null or empty";
        private const string cloudBlockBlobCannotBeNull = @"CloudBlockBlob cannot be null.";

        private CloudStorageAccount _storageAccount = null;
        private CloudBlobClient _blobClient = null;
        private CloudBlobContainer _blobContainer = null;

        #endregion

        #region Constructors

        /// <summary>
        /// Blob Storage Constrcutor - No Arguments
        /// </summary>
        public BlobStorage()
        {
            Initialize();
        }

        /// <summary>
        /// Blob Storage Constructor taking Container name as A Parameter
        /// </summary>
        /// <param name="containerName">The Blob Storage Container Name</param>
        public BlobStorage(string containerName)
        {
            Initialize();
            SetContainerContextAsync(containerName).Wait();

            // Impoprtant Note - According to https://docs.microsoft.com/en-us/azure/storage/storage-dotnet-how-to-use-blobs 
            // Anyone on the Internet can see blobs in a public container. However, you can modify or delete them only if you have the appropriate account access key or a shared access signature.
            // By default, the new container is private, meaning that you must specify your storage access key to download blobs from this container.
            // container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });  This is a public def.
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Async Task to upload an Object to Azure Blob Storage.
        /// </summary>
        /// <param name="objectToUpload">The Blob to upload</param>
        /// <param name="blobName">The Name of the Blob</param>
        /// <param name="metadata">metadata to associate with the Blob Obce Stored</param>
        /// <param name="containerName">The Name of the Container the Blob should be stored in - Optional</param>
        public virtual async Task UploadAsync(Object objectToUpload, string blobName, List<KeyValuePair<string, string>> metadata, string containerName = null)
        {
            if (objectToUpload == null)
            {
                throw new ArgumentNullException(payloadToUploadCannotBeNull);
            }

            await SetContainerContextAsync(containerName);
            byte[] payload = Utilities.ObjectToByteArray(objectToUpload);
            await UploadAsync(payload, blobName, metadata);
        }

        /// <summary>
        /// Uploads the Byte Array to Blob Storage.
        /// </summary>
        /// <param name="payload">The Payload to store</param>
        /// <param name="blobName">The Name of the Blob</param>
        /// <param name="metadata">List of Metadata to store with the Blob</param>
        /// <param name="containerName">Optional: Changes the Container Context to the specified Container Name.</param>
        public virtual async Task UploadAsync(byte[] payload, string blobName, List<KeyValuePair<string, string>> metadata, string containerName = null)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(payloadToUploadCannotBeNull);
            }

            if (payload.Length == 0)
            {
                throw new ArgumentOutOfRangeException(payloadToUploadCannotBeEmpty);
            }

            if (string.IsNullOrWhiteSpace(blobName))
            {
                throw new ArgumentNullException(blobNamecannotBeEmpty);
            }

            if (metadata == null)
            {
                throw new ArgumentNullException(metadataCannotBeNull);
            }

            await SetContainerContextAsync(containerName);
            CloudBlockBlob blockBlob = _blobContainer.GetBlockBlobReference(blobName);
            using (var memoryStream = new MemoryStream(payload, writable: false))
            {
                await blockBlob.UploadFromStreamAsync(memoryStream);
            }

            if (metadata.Count > 0)
            {
                await AddMetadataToBlob(blockBlob, metadata);
            }
        }

        /// <summary>
        /// Adds Metadata to a Blob.
        /// </summary>
        /// <param name="blockBlob">The Blob in which the metadata is to bre added.</param>
        /// <param name="metadata">The metadata to add to a blob</param>
       public virtual async Task AddMetadataToBlob(CloudBlockBlob blockBlob, List<KeyValuePair<string, string>> metadata)
        {
            if (blockBlob == null)
            {
                throw new ArgumentNullException(cloudBlockBlobCannotBeNull);
            }

            if (metadata == null)
            {
                throw new ArgumentNullException(metadataCannotBeNull);
            }

            if (metadata.Count == 0)
            {
                throw new ArgumentOutOfRangeException(metadataCannotBeNull);
            }

            foreach (KeyValuePair<string, string> datum in metadata)
            {
                blockBlob.Metadata.Add(datum);
            }

            await blockBlob.SetMetadataAsync();
        }

        /// <summary>
        /// Sets the Container Context in which the Blob is stored.
        /// </summary>
        /// <param name="containerName">The Container Name in which the blob is stored</param>
        public async Task SetContainerContextAsync(string containerName)
        {
            if (!string.IsNullOrWhiteSpace(containerName))
            {
                _blobContainer = _blobClient.GetContainerReference(containerName);
                await _blobContainer.CreateIfNotExistsAsync();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Shared constructor initialization code
        /// </summary>
        private void Initialize()
        {
            try
            {
                _storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=remotelink;AccountKey=ykKtbMsrZJI8DvikFLhWgy7EpGheIfUJzKB87nTgoQm0hwLcYBYhnMxEJhcD+HIHMZ/bBvSf9kjHNg+4CnYd4w==");                
                // todo _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
                _blobClient = _storageAccount.CreateCloudBlobClient();
            }
            catch (StorageException storageException)
            {
                // TODO - GLobal Exception Handling/Propigation
                Console.WriteLine(storageException.Message);
                throw;
            }
        }

        #endregion 
    }
}