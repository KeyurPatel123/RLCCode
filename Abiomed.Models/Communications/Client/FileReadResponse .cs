﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abiomed.Models
{
    public class FileReadResponse : BaseMessage
    {
        #region Private
        private int _status;
        private int _userRef;
        private byte[] _data;
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

        public byte[] Data
        {
            get { return _data; }
            set { _data = value; }
        }
        #endregion

    }
}
