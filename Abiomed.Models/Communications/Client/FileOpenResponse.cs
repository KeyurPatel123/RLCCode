/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * FileOpenResponse.cs: Client File Open Response Msg Struct Model
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using System;

namespace Abiomed.Models
{
    public class FileOpenResponse : BaseMessage
    {
        #region Private
        private int _status;
        private int _userRef;
        private int _size;
        private DateTime _time;
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

        public int Size
        {
            get { return _size; }
            set { _size = value; }
        }

        public DateTime Time
        {
            get { return _time; }
            set { _time = value; }
        }
        #endregion

    }
}
