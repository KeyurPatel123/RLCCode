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
        public async Task<UserResponse> Post([FromBody]Credentials credentials)
        {
            string resultMessage = "Invalid Username/password combination";
            bool isLoginSuccess = false;
            UserResponse userResponse = new UserResponse();

            try
            {
                var result = await _signInManager.PasswordSignInAsync(credentials.Username, credentials.Password, false, lockoutOnFailure: false);
                var remoteLinkUser = new RemoteLinkUser();
                bool isUserActivated = false;
                bool hasUserAcceptedTermsAndConditions = false;
                long accessFailedCount = 0;


                remoteLinkUser = await _userManager.FindByNameAsync(credentials.Username);

                if (remoteLinkUser != null)
                {
                    isUserActivated = remoteLinkUser.Activated;
                    hasUserAcceptedTermsAndConditions = remoteLinkUser.AcceptedTermsAndConditions;
                    userResponse.FirstName = remoteLinkUser.FirstName;
                    userResponse.LastName = remoteLinkUser.LastName;
                    userResponse.ViewedTermsAndConditions = remoteLinkUser.AcceptedTermsAndConditions;
                    accessFailedCount = remoteLinkUser.AccessFailedCount;
                    userResponse.FullName = userResponse.LastName + ", " + userResponse.FirstName;

                    foreach(var role in remoteLinkUser.Roles)
                    {
                        userResponse.Role = role.RoleName;
                        break;
                    }
                }

                switch(DetermineLoginResult(result, isUserActivated, hasUserAcceptedTermsAndConditions))
                {
                    case LoginResult.Succeeded:
                        resultMessage = "Success";
                        isLoginSuccess = true;
                        if (accessFailedCount > 0)
                        {
                            await ResetUserAccountAsync(remoteLinkUser);
                        }
                        break;
                    case LoginResult.EmailNotValidated:
                        resultMessage = "Email Address not verified";
                        break;
                    case LoginResult.IsLockedOut:
                        resultMessage = "Account locked";
                        break;
                    case LoginResult.NotActivated:
                        resultMessage = "Account not activated by Abiomed";
                        break;
                    case LoginResult.RequiresTwoFactorAuthentication:
                        resultMessage = "Not authorized: Multi factor authentication";
                        break;
                    case LoginResult.UserNotAcceptedTermsAndConditions:
                        isLoginSuccess = true;
                        resultMessage = "User has not accepted Terms and Conditions";
                        break;
                    case LoginResult.BadUsernamePasswordCombination:
                    case LoginResult.Unknown:
                    default:
                        if (remoteLinkUser != null)
                        {
                            await AccessFailedAsync(remoteLinkUser);
                        }
                        break;
                }
            }
            catch
            {
                // ToDo: Log Error
                // Determine UI Error Handling
            }

            userResponse.IsSuccess = isLoginSuccess;
            userResponse.Response = resultMessage;
            await _iauditLogManager.AuditAsync(credentials.Username, DateTime.UtcNow, Request.HttpContext.Connection.RemoteIpAddress.ToString(), "Login", resultMessage);

            return userResponse;
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
                    InstitutionName = userRegistration.InstitutionName,
                    InstitutionLocationProvince = userRegistration.InstitutionLocationProvince,
                    UserName = userRegistration.UserName,
                    Email = userRegistration.Email,
                    EmailConfirmed = bool.Parse(userRegistration.EmailConfirmed),
                    Activated = bool.Parse(userRegistration.Activated),
                    AcceptedTermsAndConditions = bool.Parse(userRegistration.AcceptedTermsAndConditions),
                    AcceptedTermsAndConditionsDate = userRegistration.AcceptedTermsAndConditionsDate
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

        private LoginResult DetermineLoginResult(Microsoft.AspNetCore.Identity.SignInResult signInResult, bool isUserActivated, bool hasUserAcceptedTermsAndConditions)
        {
            if (signInResult.Succeeded)
            {
                if (isUserActivated)
                {
                    if (hasUserAcceptedTermsAndConditions)
                    {
                        return LoginResult.Succeeded;
                    }
                    else
                    {
                        return LoginResult.UserNotAcceptedTermsAndConditions;
                    }
                }
                else
                {
                    return LoginResult.NotActivated;
                }
            }

            if (signInResult.IsLockedOut)
            {
                return LoginResult.IsLockedOut;
            }

            if (signInResult.IsNotAllowed)
            {
                return LoginResult.EmailNotValidated;
            }

            if (signInResult.RequiresTwoFactor)
            {
                return LoginResult.RequiresTwoFactorAuthentication;
            }

            return LoginResult.BadUsernamePasswordCombination;
        }

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
