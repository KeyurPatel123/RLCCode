using System.Collections.Generic;

namespace Abiomed.DotNetCore.Models
{
    public class UserRegistration
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public List<string> Roles { get; set; }
        public string InstitutionName { get; set; } = string.Empty;
        public string InstitutionLocationProvince { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string EmailConfirmed { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string PhoneConfirmed { get; set; } = string.Empty;
        public string Activated { get; set; } = string.Empty;
        public string AcceptedTermsAndConditions { get; set; } = string.Empty;
        public string AcceptedTermsAndConditionsDate { get; set; } = string.Empty;
    }
}