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
        private readonly IInstitutionManager _institutionManager;
        
        public InstitutionController(IAuditLogManager auditLogManager, IInstitutionManager institutionManager)
        {
            _auditLogManager = auditLogManager;
            _institutionManager = institutionManager;
        }

        [HttpGet]
        [Route("GetInstitutions")]
        [Authorize(Roles = "ADMIN")]
        public List<Institution> GetInstitutions()
        {
            return _institutionManager.GetInstitutions();
        }       
    }
}
