/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * BearerStatistics.cs: [Software Unit Description]
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/
using System;

namespace Abiomed.Models
{
    [Serializable]
    public class BearerStatistics
    {
        private UInt64 _bytes;
        private int _frames;
        private int _seq;
        private int _count;
        
        public UInt64 Bytes
        {
            get { return _bytes; }
            set { _bytes = value; }
        }

        public int Frames
        {
            get { return _frames; }
            set { _frames = value; }
        }

        public int Seq
        {
            get { return _seq; }
            set { _seq = value; }
        }

        public int Count
        {
            get { return _count; }
            set { _count = value; }
        }
    }
}
