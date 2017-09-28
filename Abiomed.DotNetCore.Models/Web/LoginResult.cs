
namespace Abiomed.DotNetCore.Models
{
    public enum LoginResult
    {
        Unknown = -1,
        BadUsernamePasswordCombination = 0,
        Succeeded = 1,
        IsLockedOut = 2,
        EmailNotValidated = 3,
        NotActivated = 4,
        RequiresTwoFactorAuthentication = 5,
        UserNotAcceptedTermsAndConditions = 6
    }
}
