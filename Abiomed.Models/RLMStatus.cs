/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * RLMStatus.cs: RLMStatus Model
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

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
