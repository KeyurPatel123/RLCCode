using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abiomed.Models
{
    public class BearerOpen : BaseMessage
    {
        #region Private        
        private byte[] _serialNo;
        private byte[] _bearer;        
        #endregion

        #region Public
        public byte[] SerialNo
        {
            get { return _serialNo; }
            set { _serialNo = value; }
        }

        public byte[] Bearer
        {
            get { return _bearer; }
            set { _bearer = value; }
        }
        #endregion

    }
}
