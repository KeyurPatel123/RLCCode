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
using Newtonsoft.Json.Linq;

namespace Abiomed_WirelessRemoteLink.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class InstitutionController : Controller
    {
        private readonly IAuditLogManager _auditLogManager;
        
        public InstitutionController(IAuditLogManager auditLogManager)
        {
            _auditLogManager = auditLogManager;
        }

        [HttpGet]
        [Route("GetInstitutions")]
        [Authorize(Roles = "ADMIN")]
        public JArray GetInstitutions()
        {
            JArray institutions = new JArray();

            institutions.Add(new JObject(
                            new JProperty("Id", 12340),
                            new JProperty("DisplayName", "Henry Ford Hospital")));

            institutions.Add(new JObject(
                new JProperty("Id", 12341),
                new JProperty("DisplayName", "Harper Hospital")));


            institutions.Add(new JObject(
                new JProperty("Id", 12342),
                new JProperty("DisplayName", "Mercy General Hospital")));


            institutions.Add(new JObject(
                new JProperty("Id", 12343),
                new JProperty("DisplayName", "Univ of Washington Medical Ctr")));


            institutions.Add(new JObject(
                new JProperty("Id", 12344),
                new JProperty("DisplayName", "Wellstar Kennestone Hospital")));


            return institutions;
        }       
    }
}
