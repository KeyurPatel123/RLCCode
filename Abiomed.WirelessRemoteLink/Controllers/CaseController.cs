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
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Abiomed_WirelessRemoteLink.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CaseController : Controller
    {
        private readonly IAuditLogManager _auditLogManager;
        private readonly IDeviceManager _deviceManager;

        public CaseController(IAuditLogManager auditLogManager, IDeviceManager deviceManager)
        {
            _auditLogManager = auditLogManager;
            _deviceManager = deviceManager;
        }

        [HttpGet]
        [Route("GetCases")]
        [Authorize(Roles = "ADMIN, STAFF")]
        public KeyValuePair<string, OcrResponse>[] GetCases()
        {
            var devices = _deviceManager.GetRlmDevices();
            var deviceArr = devices.Devices.ToArray();
            return deviceArr;
        }

        [HttpGet]
        [Route("GetUpdatedCases")]
        [Authorize(Roles = "ADMIN, STAFF")]
        public KeyValuePair<string, OcrResponse>[] GetUpdatedCases()
        {
            // todo - gets Paul code here
            var devices = _deviceManager.GetRlmDevices();
            var deviceArr = devices.Devices.ToArray();
            return deviceArr;
        }

        [HttpGet]
        [Route("GetUpdatedCases")]
        [Authorize(Roles = "ADMIN, STAFF")]
        public KeyValuePair<string, OcrResponse>[] GetCase()
        {
            // todo - gets Paul code here
            var devices = _deviceManager.GetRlmDevices();
            var deviceArr = devices.Devices.ToArray();
            return deviceArr;
        }

    }
}
