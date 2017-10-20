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
        public string GetInstitutions()
        {
            string returnResult = string.Empty;
            returnResult = @"
            { 'Institutions' : [
                { 'Id': 12340, 'DisplayName' : 'Henry Ford Hospital'},
                { 'Id': 12341, 'DisplayName' : 'Harper Hospital'},
                { 'Id': 12342, 'DisplayName' : 'Mercy General Hospital'},
                { 'Id': 12343, 'DisplayName' : 'Univ of Washington Medical Ctr'},
                { 'Id': 12344, 'DisplayName' : 'Wellstar Kennestone Hospital'},
                { 'Id': 12345, 'DisplayName' : 'Ochsner Foundation Hospital'},
                { 'Id': 12346, 'DisplayName' : 'New York Presbyterian Columbia'},
                { 'Id': 12347, 'DisplayName' : 'Inova Fairfax Hospital'},
                { 'Id': 12348, 'DisplayName' : 'Banner University Medical Ctr'},
                { 'Id': 12349, 'DisplayName' : 'Cedars - Sinai Medical Center'}    
            ]
           }";            
            return returnResult;
        }       
    }
}
