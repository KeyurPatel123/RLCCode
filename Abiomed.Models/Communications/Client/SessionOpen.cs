using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abiomed.Models
{
    public class SessionOpen : BaseMessage
    {
        #region Private
        private int _ifaceVer;
        private string _serialNo;
        private Definitions.Bearer _bearer = Definitions.Bearer.Unknown;
        private string _text;
        #endregion

        #region Public
        public int IfaceVer
        {
            get { return _ifaceVer; }
            set { _ifaceVer = value; }
        }

        public string SerialNo
        {
            get { return _serialNo; }
            set { _serialNo = value; }
        }

        public Definitions.Bearer Bearer
        {
            get { return _bearer; }
            set { _bearer = value; }
        }

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }
        #endregion

    }
}
