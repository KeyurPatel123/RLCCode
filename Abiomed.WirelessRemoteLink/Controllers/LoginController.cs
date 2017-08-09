using Abiomed.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Abiomed_WirelessRemoteLink.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class LoginController : Controller
    {       
        [HttpPost]
        [Route("UserLogin")]
        [AllowAnonymous]
        public bool Post([FromBody]Credentials credentials)
        {
            var status = true;
            
            return status;
        }
        
    }
}
