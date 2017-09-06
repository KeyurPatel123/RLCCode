using Abiomed.Models;
using Abiomed.DotNetCore.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using ElCamino.AspNetCore.Identity.AzureTable.Model;

namespace Abiomed_WirelessRemoteLink.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class AuthenticationController : Controller
    {
        private readonly UserManager<RemoteLinkUser> _userManager;
        private readonly SignInManager<RemoteLinkUser> _signInManager;
        private readonly IAuditLogManager _auditLogManager;
        private readonly IEmailManager _emailManager;

        public AuthenticationController(UserManager<RemoteLinkUser> userManager, SignInManager<RemoteLinkUser> signInManager, IAuditLogManager auditLogManager, IEmailManager emailManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _auditLogManager = auditLogManager;
            _emailManager = emailManager;
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
                var result = await PasswordSignInAsync(credentials.Username, credentials.Password);
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
            catch(Exception EX)
            {
                var xxx = EX.Message;
                // ToDo: Log Error
                // Determine UI Error Handling
            }

            userResponse.IsSuccess = isLoginSuccess;
            userResponse.Response = resultMessage;
            await _auditLogManager.AuditAsync(credentials.Username, DateTime.UtcNow, Request.HttpContext.Connection.RemoteIpAddress.ToString(), "Login", resultMessage);

            return userResponse;
        }

        [HttpPost]
        [Route("AcceptTAC")]
        [AllowAnonymous]
        public async Task<bool> Post()
        {
            bool result = false;
            string auditMessage = "Accepted Terms and Conditions";

            ClaimsPrincipal currentUser = User;

            if (currentUser.Identity.IsAuthenticated)
            {
                result = await SetTermsAndConditionsAsync(true);
                if (!result)
                {
                    auditMessage = "Error attempting to set Terms and Conditions";
                }
            }
            else
            {
                auditMessage = "Error attmepting to set Terms and Conditions.  User is not authenticated.";
            }

            await _auditLogManager.AuditAsync(User.Identity.Name, DateTime.UtcNow, Request.HttpContext.Connection.RemoteIpAddress.ToString(), "TermsAndConditions", auditMessage);
            return result;
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
                        await _emailManager.BroadcastToQueueStorage(userRegistration.Email, "Remote Link Cloud Account Creation", "Your Remote Link Cloud Account has been created ... Some More Text here - Instructions/welcome message is an open task.", userRegistration.FirstName + " " + userRegistration.LastName);
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

                await _auditLogManager.AuditAsync(userRegistration.Email, DateTime.UtcNow, Request.HttpContext.Connection.RemoteIpAddress.ToString(), "Registration", "New User Registration");
            }
            catch 
            {
                // ToDo Handle/Log Error
            }
            return status;

        }

        #region Private Helper Methods
        private async Task<Microsoft.AspNetCore.Identity.SignInResult> PasswordSignInAsync(string userName, string password)
        {
            var result = new Microsoft.AspNetCore.Identity.SignInResult();

            try
            {
                result = await _signInManager.PasswordSignInAsync(userName, password, false, lockoutOnFailure: false);
            }
            catch
            {
                result = null;
            }

            return result;
        }


        private async Task<bool> SetTermsAndConditionsAsync(bool termsAndConditions)
        {
            bool result = false;
            try
            {
                var remoteLinkUser = await _userManager.GetUserAsync(User);
                remoteLinkUser.AcceptedTermsAndConditions = termsAndConditions;
                if (termsAndConditions)
                {
                    remoteLinkUser.AcceptedTermsAndConditionsDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
                }
                else
                {
                    remoteLinkUser.AcceptedTermsAndConditionsDate = string.Empty;
                }

                var identityResult = await _userManager.UpdateAsync(remoteLinkUser);
                if (identityResult.Succeeded)
                {
                    result = true;
                }
            } catch (Exception EX)
            {
                // TODO Handle Exception
                string message = EX.Message;
            }

            return result;
        }
        private LoginResult DetermineLoginResult(Microsoft.AspNetCore.Identity.SignInResult signInResult, bool isUserActivated, bool hasUserAcceptedTermsAndConditions)
        { 
            if (signInResult == null || signInResult.Succeeded)
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
