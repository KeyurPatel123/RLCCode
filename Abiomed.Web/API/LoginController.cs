/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * LoginController.cs: Login Controller
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/
using Abiomed.Models;
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
