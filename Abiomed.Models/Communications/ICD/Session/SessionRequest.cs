/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * SessionRequest.cs: Client Session Open Msg Struct Model
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/


using Newtonsoft.Json;

namespace Abiomed.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SessionRequest : BaseMessage
    {
        #region Private
        private int _ifaceVer;
        private string _serialNo;
        private Definitions.Bearer _bearer = Definitions.Bearer.Unknown;
        private string _text;
        #endregion

        #region Public
        [JsonProperty]
        public int IfaceVer
        {
            get { return _ifaceVer; }
            set { _ifaceVer = value; }
        }

        [JsonProperty]
        public string SerialNo
        {
            get { return _serialNo; }
            set { _serialNo = value; }
        }

        [JsonProperty]
        public Definitions.Bearer Bearer
        {
            get { return _bearer; }
            set { _bearer = value; }
        }

        [JsonProperty]
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }
        #endregion
    }
}
