using System;

namespace Abiomed.DotNetCore.Models
{
    [Serializable]
    public class UserResponse
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsSuccess { get; set; } = false;
        public bool ViewedTermsAndConditions { get; set; } = false;
        public string Response { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}