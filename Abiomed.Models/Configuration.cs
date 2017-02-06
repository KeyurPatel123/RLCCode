using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abiomed.Models
{
    public class Configuration
    {
        // Default strings to localhost
        private const string localhost = @"localhost";
        private string _deviceStatus = @"http://localhost/api/DeviceStatus";
        private string _imageSend = @"http://localhost/RLR/api/Image";

       
        #region Constructor
        public Configuration()
        {
            // Get Configuration Data
            var connectionManager = ConfigurationManager.GetSection("ConnectionManager") as System.Collections.Specialized.NameValueCollection;
            string type = connectionManager["RUN"].ToString();
            string WOWZA = connectionManager["WOWZA"].ToString();
            string WEB = connectionManager["WEB"].ToString();
            string RLR = connectionManager["RLR"].ToString();

            if (type != @"localhost")
            {
                // Update Strings
                StringBuilder str = new StringBuilder(_deviceStatus);
                str.Replace(localhost, WEB);

                str = new StringBuilder(_imageSend);
                str.Replace(localhost, WEB);
            }

        }
        #endregion

        public string DeviceStatus
        {
            get { return _deviceStatus; }
            set { _deviceStatus = value; }
        }

        public string ImageSend
        {
            get { return _imageSend; }
            set { _imageSend = value; }
        }
    }
}
