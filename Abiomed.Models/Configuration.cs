/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * Configuration.cs: Configuration Reader
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using System;
using System.Configuration;
using System.Text;

namespace Abiomed.Models
{
    public class Configuration
    {
        // Default strings to localhost
        private const string localhost = @"localhost";
        private string _deviceStatus = @"http://localhost/api/DeviceStatus";
        private string _imageSend = @"http://localhost/RLR/api/Image";
        private int _keepAliveTimer = 5000;
       
        #region Constructor
        public Configuration()
        {
            // Configure Sections
            connectionManager();
            keepAliveTimerManager();
        }
        #endregion

        private void connectionManager()
        {
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

                _deviceStatus = str.ToString();

                str = new StringBuilder(_imageSend);
                str.Replace(localhost, WEB);

                _imageSend = str.ToString();
            }
        }

        private void keepAliveTimerManager()
        {
            var optionsManager = ConfigurationManager.GetSection("OptionsManager") as System.Collections.Specialized.NameValueCollection;
            _keepAliveTimer = Convert.ToInt32(optionsManager["KeepAliveTimer"].ToString());
        }

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

        public int KeepAliveTimer
        {
            get { return _keepAliveTimer; }
            set { _keepAliveTimer = value; }
        }
    }
}
