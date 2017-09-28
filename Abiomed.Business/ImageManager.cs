using Abiomed.Storage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Abiomed.Business
{
    /// <summary>
    /// Class to Manage the Image Store and Retrieve operations
    /// </summary>
    public class ImageManager : IImageManager
    {
        #region Private Member Variables

        private IBlobStorage _iBlobImageStorage;

        #endregion

        #region Constructors

        public ImageManager()
        {
            Initialize();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This method will upload the Image to Azure Blob Storage
        /// </summary>
        /// <param name="deviceName">The Device Name sending the Image</param>
        /// <param name="image">The Actual Image to Store</param>
        /// <param name="imageFormat">The Format of the Image (i.e. Jpg, Png, Gif, Etc.)</param>
        /// <param name="metadata">Matadata to associate with the Image</param>
        /// <param name="containerName">The Name of the Storage Container the Image is to be stored</param>
        public async Task UploadImageAsync(string deviceName, Image image, System.Drawing.Imaging.ImageFormat imageFormat, List<KeyValuePair<string, string>> metadata, string containerName = null)
        {
            if (string.IsNullOrWhiteSpace(deviceName))
            {
                throw new ArgumentNullException("Device Name cannot be null, empty, or whitespace.");
            }

            if (image == null)
            {
                throw new ArgumentNullException("Image cannot be null.");
            }

            await _iBlobImageStorage.UploadAsync(ConvertImageToByteArray(image, imageFormat), CreateBlobNameForImage(deviceName, DateTime.UtcNow), metadata, containerName);
        }

        /// <summary>
        /// This method will upload an image from a File Path/Name to Azure Blob Storage
        /// </summary>
        /// <param name="deviceName">The Device Name sending the Image</param>
        /// <param name="imagePath">The Path and fle name of the image to upload</param>
        /// <param name="imageFormat">The Format of the Image (i.e. Jpg, Png, Gif, Etc.)</param>
        /// <param name="metadata">Matadata to associate with the Image</param>
        /// <param name="containerName">The Name of the Storage Container the Image is to be stored</param>
        public async Task UploadImageFromFile(string deviceName, string imagePath, System.Drawing.Imaging.ImageFormat imageFormat, List<KeyValuePair<string, string>> metadata, string containerName = null)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                throw new ArgumentNullException("Image Path cannot be null, empty, or whitespace.");
            }
            await UploadImageAsync(deviceName, Image.FromFile(imagePath), imageFormat, metadata, containerName);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Converts an Image type to a Binary Array
        /// </summary>
        /// <param name="image">The Image to Convert</param>
        /// <param name="imageFormat">he Format of the Image (i.e. Jpg, Png, Gif, Etc.)</param>
        /// <returns>The Image Converted to a Byte Array</returns>
        private byte[] ConvertImageToByteArray(Image image, System.Drawing.Imaging.ImageFormat imageFormat)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                image.Save(memoryStream, imageFormat);
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Centralized logic to be called by the constructor.  Core functionality goes in this method and any specialized constructor
        /// code for tat overloaded constructor can go in that instance
        /// </summary>
        private void Initialize()
        {
            _iBlobImageStorage = new BlobStorage();
        }

        /// <summary>
        /// Creates a Blob Image Name/Path for Azure Storage.
        /// </summary>
        /// <param name="deviceName">The Device Name</param>
        /// <param name="currentDateTime">The Current Date TIme in which a path is constructed</param>
        /// <returns>storage Name/path</returns>
        private string CreateBlobNameForImage(string deviceName, DateTime currentDateTime)
        {
            StringBuilder blobName = new StringBuilder(deviceName);

            blobName.Append("/" + currentDateTime.Year.ToString("0000"));
            blobName.Append("/" + currentDateTime.Month.ToString("00"));
            blobName.Append("/" + currentDateTime.Day.ToString("00"));
            blobName.Append("/" + currentDateTime.Hour.ToString("00"));
            blobName.Append("/" + currentDateTime.Minute.ToString("00"));
            return blobName.Append("m" + currentDateTime.Second.ToString("00") + "s").ToString();
        }

        #endregion
    }
}