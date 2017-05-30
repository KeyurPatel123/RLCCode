/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * BearerInformation.cs: [Software Unit Description]
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/
using System;

namespace Abiomed.Models
{
    public class BearerInformation
    {
        private int _result;
        private int _available;
        private int _rssi;
        private int _attempts;
        private int _failures;

        public int Result
        {
            get { return _result; }
            set { _result = value; }
        }

        public int Available
        {
            get { return _available; }
            set { _available = value; }
        }

        public int RSSI
        {
            get { return _rssi; }
            set { _rssi = value; }
        }

        public int Attempts
        {
            get { return _attempts; }
            set { _attempts = value; }
        }

        public int Failures
        {
            get { return _failures; }
            set { _failures = value; }
        }
    }
}
