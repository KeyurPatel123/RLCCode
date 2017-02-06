using Abiomed.Models.Communications;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
            catch(Exception e)
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
