using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
