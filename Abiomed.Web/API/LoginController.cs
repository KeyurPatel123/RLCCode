using Abiomed.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Abiomed.Web.API
{
    public class LoginController : ApiController
    {
        [HttpPost]
        public bool Post(Credentials credentials)
        {
            var status = false;
            if (credentials.Username.ToLower() == @"abiomedadmin" && credentials.Password== @"Str3@m")
            {
                status = true;
            }
            return status;
        }
    }
}
