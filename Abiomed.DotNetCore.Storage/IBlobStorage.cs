using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abiomed.DotNetCore.Storage
{
    public interface IBlobStorage
    {
        /// <summary>
        /// Async Task to upload an Object to Azure Blob Storage.
        /// </summary>
        /// <param name="objectToUpload">The Blob to upload</param>
        /// <param name="blobName">The Name of the Blob</param>
        /// <param name="metadata">metadata to associate with the Blob Obce Stored</param>
        /// <param name="containerName">The Name of the Container the Blob should be stored in - Optional</param>
        Task UploadAsync(Object objectToUpload, string blobName, List<KeyValuePair<string, string>> metadata, string containerName = null);

        /// <summary>
        /// Uploads the Byte Array to Blob Storage.
        /// </summary>
        /// <param name="payload">The Payload to store</param>
        /// <param name="blobName">The Name of the Blob</param>
        /// <param name="metadata">List of Metadata to store with the Blob</param>
        /// <param name="containerName">Optional: Changes the Container Context to the specified Container Name.</param>
        Task UploadAsync(byte[] payload, string blobName, List<KeyValuePair<string, string>> metadata, string containerName = null);

        /// <summary>
        /// Adds Metadata to a Blob.
        /// </summary>
        /// <param name="blockBlob">The Blob in which the metadata is to bre added.</param>
        /// <param name="metadata">The metadata to add to a blob</param>
        Task AddMetadataToBlob(CloudBlockBlob blockBlob, List<KeyValuePair<string, string>> metadata);

        /// <summary>
        /// Sets the Container Context in which the Blob is stored.
        /// </summary>
        /// <param name="containerName">The Container Name in which the blob is stored</param>
        Task SetContainerContextAsync(string containerName);
    }
}
