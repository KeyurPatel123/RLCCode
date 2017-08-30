using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abiomed.Models
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