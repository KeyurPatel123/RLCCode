/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * IEventManager.cs:
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/
using Abiomed.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abiomed.Web
{
    public interface IEventManager
    {
        #region RLR to RLM Events
        void KeepAliveIndication(string serialNumber);
        void BearerChangeIndication(string serialNumber, string bearer);
        void StatusIndication(string serialNumber);
        void BearerAuthenticationReadIndication(string serialNumber);
        void BearerAuthenticationUpdateIndication(Authorization authorization, bool delete);
        void StreamingVideoControlIndication(string serialNumber);
        void ScreenCaptureIndication(string serialNumber);
        void VideoStop(string serialNumber);
        void ImageStop(string serialNumber);
        void OpenRLMLogFileIndication(string serialNumber);
        void CloseSessionIndication(string serialNumber);
        #endregion        

    }
}
