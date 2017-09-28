using ElCamino.AspNetCore.Identity.AzureTable.Model;

namespace Abiomed.DotNetCore.Models
{
    public class RemoteLinkUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string InstitutionName { get; set; } = string.Empty;
        public string InstitutionLocationProvince { get; set; } = string.Empty;
        public bool Activated { get; set; } = false;
        public string ActivationDate { get; set; } = string.Empty;
        public string ActivatedBy { get; set; } = string.Empty;
        public string Territory { get; set; } = string.Empty;
        public bool AcceptedTermsAndConditions { get; set; } = false;
        public string AcceptedTermsAndConditionsDate { get; set; } = string.Empty;
    }
}
