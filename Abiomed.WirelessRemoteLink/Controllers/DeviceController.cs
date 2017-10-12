using Abiomed.DotNetCore.Models;
using Abiomed.DotNetCore.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using System.Linq;
using System.Net;
using Abiomed.WirelessRemoteLink;
using System.Collections.Generic;

namespace Abiomed_WirelessRemoteLink.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class DeviceController : Controller
    {
        private readonly IAuditLogManager _auditLogManager;
        private readonly IDeviceManager _deviceManager;

        public DeviceController(IAuditLogManager auditLogManager, IDeviceManager deviceManager)
        {
            _auditLogManager = auditLogManager;
            _deviceManager = deviceManager;
        }

        [HttpGet]
        [Route("GetDevices")]
        [AllowAnonymous]
        public KeyValuePair<string, OcrResponse>[] GetDevices()
        {
            var devices = _deviceManager.GetRlmDevices();
            var deviceArr = devices.Devices.ToArray();
            return deviceArr;
        }       
    }
}
