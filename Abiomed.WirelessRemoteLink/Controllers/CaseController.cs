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
        private readonly ICaseManager _caseManager;

        public CaseController(IAuditLogManager auditLogManager, ICaseManager caseManager)
        {
            _auditLogManager = auditLogManager;
            _caseManager = caseManager;
        }

        [HttpGet]
        [Route("GetCases")]
        [Authorize(Roles = "ADMIN, STAFF")]
        public KeyValuePair<string, Case>[] GetCases()
        {
            var cases = _caseManager.GetAll();
            var caseArray = cases.Cases.ToArray();
            return caseArray;
        }       
    }
}
