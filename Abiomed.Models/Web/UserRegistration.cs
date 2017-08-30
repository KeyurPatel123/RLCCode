using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abiomed.Models
{
    public class UserRegistration
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public List<string> Roles { get; set; }
        public string Role { get; set; } = string.Empty;
        public string InstitutionName { get; set; } = string.Empty;
        public string InstitutionLocationProvince { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string EmailConfirmed { get; set; } = string.Empty;
        public string Activated { get; set; } = string.Empty;
        public string AcceptedTermsAndConditions { get; set; } = string.Empty;
        public string AcceptedTermsAndConditionsDate { get; set; } = string.Empty;
    }
}
