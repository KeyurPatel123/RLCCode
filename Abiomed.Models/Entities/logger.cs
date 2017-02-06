using Abiomed.CLR.Models;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abiomed.Models
{   
    [BsonIgnoreExtraElements]
    public class log
    {
        public DateTime timestamp;
        public string level;
        public string thread;
        public string logx;
        public string message;
        public string mycustomproperty;
    }
}
