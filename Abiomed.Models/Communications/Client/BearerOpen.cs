/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * BearerOpen.cs: Client Bearer Open Msg Struct Model
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

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
