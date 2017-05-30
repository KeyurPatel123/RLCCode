/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * ScreenCaptureResponse.cs: Client Screen Capture Response Msg Struct Model
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

namespace Abiomed.Models
{
    public class StreamingVideoControlResponse : BaseMessage
    {
        #region Private
        private int _status;
        private int _userRef;       
        #endregion

        #region Public
        public int Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public int UserRef
        {
            get { return _userRef; }
            set { _userRef = value; }
        }
        #endregion

    }
}
