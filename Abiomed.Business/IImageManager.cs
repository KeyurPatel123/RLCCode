
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace Abiomed.Business
{
    public interface IImageManager
    {
        /// <summary>
        /// This method will upload the Image to Azure Blob Storage
        /// </summary>
        /// <param name="deviceName">The Device Name sending the Image</param>
        /// <param name="image">The Actual Image to Store</param>
        /// <param name="imageFormat">The Format of the Image (i.e. Jpg, Png, Gif, Etc.)</param>
        /// <param name="metadata">Matadata to associate with the Image</param>
        /// <param name="containerName">The Name of the Storage Container the Image is to be stored</param>
        Task UploadImage(string deviceName, Image image, System.Drawing.Imaging.ImageFormat imageFormat, List<KeyValuePair<string, string>> metadata, string containerName = null);

        /// <summary>
        /// This method will upload an image from a File Path/Name to Azure Blob Storage
        /// </summary>
        /// <param name="deviceName">The Device Name sending the Image</param>
        /// <param name="imagePath">The Path and fle name of the image to upload</param>
        /// <param name="imageFormat">The Format of the Image (i.e. Jpg, Png, Gif, Etc.)</param>
        /// <param name="metadata">Matadata to associate with the Image</param>
        /// <param name="containerName">The Name of the Storage Container the Image is to be stored</param>
        Task UploadImageFromFile(string deviceName, string imagePath, System.Drawing.Imaging.ImageFormat imageFormat, List<KeyValuePair<string, string>> metadata, string containerName = null);
    }
}
