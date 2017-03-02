/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * ImageController.cs: Image Controller
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/
using Abiomed.Models.Communications;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web.Http;

namespace Abiomed.Web.API
{
    public class ImageController : ApiController
    {
        [HttpPost]
        public bool Post(RLMImage rLMImage)
        {
            try
            {
                using (Image image = Image.FromStream(new MemoryStream(rLMImage.Data)))
                {                    
                    image.Save(@"C:\Development\MKS.png", ImageFormat.Png);  // Or Png
                }
            }
            catch(Exception)
            {

            }
            return true;
        }

        [HttpGet]
        public bool Get()
        {
            return true;
        }
    }
}
