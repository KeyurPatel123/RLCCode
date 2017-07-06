/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * LimitRequest.cs: [Software Unit Description]
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/
using System;

namespace Abiomed.Models
{
    [Serializable]
    public class LimitRequest
    {
        private Definitions.Bearer _bearer = Definitions.Bearer.Unknown;
        private int _bytes = 0;
        private int _percent = 0;

        public Definitions.Bearer Bearer
        {
            get { return _bearer; }
            set { _bearer = value; }
        }

        public int Bytes
        {
            get { return _bytes; }
            set { _bytes = value; }
        }

        public int Percent
        {
            get { return _percent; }
            set { _percent = value; }
        }        
    }
}
