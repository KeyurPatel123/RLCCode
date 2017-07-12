using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abiomed.Models
{
    public class BearerPriorityUpdate
    {
        private BearerPriority _bearerPriority = new BearerPriority();
        private string serialNumber = string.Empty ;

        public string SerialNumber
        {
            get { return serialNumber ; }
            set { serialNumber  = value; }
        }

        public BearerPriority BearerPriority
        {
            get { return _bearerPriority; }
            set { _bearerPriority = value; }
        }

    }
}
