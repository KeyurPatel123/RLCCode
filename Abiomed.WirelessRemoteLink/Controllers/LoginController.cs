using Microsoft.AspNetCore.Mvc;

namespace Abiomed_WirelessRemoteLink.Controllers
{
    [Route("api/[controller]")]
    public class LoginController : Controller
    {
       
        [HttpPost("[action]")]
        public bool Login(string credentials)
        {
            var status = true;
            
            return status;
        }
        
    }
}
