/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * RLMStreamList.cs: RLM Stream List Model
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using System.Collections.Generic;

namespace Abiomed.Models
{
    public class RLMStreamList
    {
        private List<RLMStream> _rLMStreamList;

        public List<RLMStream> RLMStreams
        {
            get { return _rLMStreamList; }
            set { _rLMStreamList = value; }
        }

    }
}
