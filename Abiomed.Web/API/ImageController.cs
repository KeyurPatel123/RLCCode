/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * ImageController.cs: Image Controller
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;

namespace Abiomed.Web.API
{
    public class ImageController : ApiController
    {
        [HttpGet]
        [Route("api/Image/GetImageNames/{rlmSerial}")]
        public List<WebImage> GetImageNames([FromUri]string rlmSerial)
        {
            // Search for all images with serial number
            DirectoryInfo hdDirectoryInWhichToSearch = new DirectoryInfo(@"c:\\RLMImages");
            FileInfo[] filesInDir = hdDirectoryInWhichToSearch.GetFiles(rlmSerial + "*");

            List<WebImage> files = new List<WebImage>();
            int count = 0;

            foreach (FileInfo foundFile in filesInDir)
            {
                string fullName = foundFile.FullName;

                var imageIn = Image.FromFile(fullName,true);
                using (var ms = new MemoryStream())
                {
                    imageIn.Save(ms, ImageFormat.Png);

                    WebImage webImage = new WebImage()
                    {
                        id = count++,
                        fileName = fullName,
                        data = ms.ToArray()
                    };
                    
                    files.Add(webImage);
                }                                               
            }
            return files;
        }
    }

    public class WebImage
    {
        public string fileName = "";
        public byte[] data;
        public int id = 0;
    }
}
