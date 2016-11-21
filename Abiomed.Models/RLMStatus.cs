using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abiomed.Models
{
    public class RLMStatus
    {
        private StatusEnum _status;

        public StatusEnum Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public enum StatusEnum
        {
            Unknown = -1,
            Success = 0,
            Failure = 1
        };
    }
}
