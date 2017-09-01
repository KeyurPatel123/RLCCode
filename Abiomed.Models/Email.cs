using System;

namespace Abiomed.Models 
{
    [Serializable]
    public class Email
    {
        public string To { get; set; } = string.Empty;
        public string ToFriendlyName { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string FromFriendlyName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }
}
