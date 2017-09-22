using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abiomed.DotNetCore.Models
{
    [Serializable]
    public class RegisterResponse
    {
        public enum CreationStatus
        {
            Unknown = -1,
            Success = 0,
            AlreadyExist = 1,
            GeneralFailure = 2
        };        

        public string CreationResult { get; set; } = CreationStatus.Unknown.ToString();        
        public string Response { get; set; } = string.Empty;         
    }
}