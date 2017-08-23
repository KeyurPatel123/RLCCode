using Abiomed.Models;
using Abiomed.DotNetCore.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System;
using ElCamino.AspNetCore.Identity.AzureTable.Model;

namespace Abiomed_WirelessRemoteLink.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class AuthenticationController : Controller
    {
        private readonly UserManager<RemoteLinkUser> _userManager;
        private readonly SignInManager<RemoteLinkUser> _signInManager;
        private readonly IAuditLogManager _iauditLogManager;

        public AuthenticationController(UserManager<RemoteLinkUser> userManager, SignInManager<RemoteLinkUser> signInManager, IAuditLogManager iauditLogManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _iauditLogManager = iauditLogManager;
        }

        [HttpPost]
        [Route("Login")]
        [AllowAnonymous]
        public async Task<bool> Post([FromBody]Credentials credentials)
        {
            var status = false;

            try
            {
                var result = await _signInManager.PasswordSignInAsync(credentials.Username, credentials.Password, false, lockoutOnFailure: false);
                var remoteLinkUser = await _userManager.FindByNameAsync(credentials.Username);

                if (remoteLinkUser == null)
                {
                    await _iauditLogManager.AuditAsync(credentials.Username, DateTime.UtcNow, Request.HttpContext.Connection.RemoteIpAddress.ToString(), "Login", "Login Failed: User Does not exist.");
                }
                else
                {
                    if (result.Succeeded == true)
                    {
                        await ResetUserAccountAsync(remoteLinkUser);
                        await _iauditLogManager.AuditAsync(credentials.Username, DateTime.UtcNow, Request.HttpContext.Connection.RemoteIpAddress.ToString(), "Login", "Login Successful");
                        status = true;
                    }
                    else
                    {
                        await AccessFailedAsync(remoteLinkUser);
                        //TODO (Descriptive error message logged).
                        await _iauditLogManager.AuditAsync(credentials.Username, DateTime.UtcNow, Request.HttpContext.Connection.RemoteIpAddress.ToString(), "Login", "Login Failed... Place Holder");

                    }
                }
            }
            catch (Exception EX)
            {

                var xx = EX.Message; // ToDo: Log Error/Audit
            }

            return status;
        }

        [HttpPost]
        [Route("Register")]
        [AllowAnonymous]
        public async Task<bool> Post([FromBody] UserRegistration userRegistration)
        {
            var status = false;

            try
            {
                var remoteLinkUser = new RemoteLinkUser
                {
                    FirstName = userRegistration.FirstName,
                    LastName = userRegistration.LastName,
                    MiddleInitial = userRegistration.MiddleInitial,
                    InstitutionName = userRegistration.InstitutionName,
                    InstitutionLocationProvince = userRegistration.InstitutionLocationProvince,
                    UserName = userRegistration.UserName,
                    Email = userRegistration.Email                  
                };

                var result = await _userManager.CreateAsync(remoteLinkUser, userRegistration.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRolesAsync(remoteLinkUser, userRegistration.Roles);
                    var updateActionResult = await _userManager.UpdateAsync(remoteLinkUser);

                    if (updateActionResult.Succeeded)
                    {
                        // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                        // Send an email with this link
                        //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                        //await _emailSender.SendEmailAsync(model.Email, "Confirm your account",
                        //    $"Please confirm your account by clicking this link: <a href='{callbackUrl}'>link</a>");
                        await _signInManager.SignInAsync(remoteLinkUser, isPersistent: false);
                        status = true;
                    }
                }

                // ToDo Add Auditing
            }
            catch 
            {
                // ToDo Handle/Log Error
            }
            return status;

        }

        #region Private Helper Methods

        private async Task ResetUserAccountAsync(RemoteLinkUser remoteLinkUser)
        {
            if (_userManager.SupportsUserLockout)
            {
                int accessFailedCount = await _userManager.GetAccessFailedCountAsync(remoteLinkUser);
                await _userManager.ResetAccessFailedCountAsync(remoteLinkUser);
            }
        }

        private async Task AccessFailedAsync(RemoteLinkUser remoteLinkUser)
        {
            if (_userManager.SupportsUserLockout)
            {
                var supportsLockout = await _userManager.AccessFailedAsync(remoteLinkUser);
            }
        }
        #endregion
    }
}
