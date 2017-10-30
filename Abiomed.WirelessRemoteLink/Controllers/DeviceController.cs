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
    public class DeviceController : Controller
    {
        private readonly IAuditLogManager _auditLogManager;        

        public DeviceController(IAuditLogManager auditLogManager)
        {
            _auditLogManager = auditLogManager;
        }

        [HttpPost]
        [Route("CreateDevice")]
        [Authorize(Roles = "ADMIN")]
        public RegisterResponse CreateDevice(RLMDeviceWeb device)
        {
            RegisterResponse response = new RegisterResponse();
            
            // Check if device exist

            // Add device

            return response;
        }

        [HttpPost]
        [Route("UpdateDevice")]
        [Authorize(Roles = "ADMIN")]
        public bool UpdateDevice()
        {
            return true;
        }

        [HttpGet]
        [Route("GetDevices")]
        [Authorize(Roles = "ADMIN")]
        public bool GetDevices()
        {
            return true;
        }
    }
}
