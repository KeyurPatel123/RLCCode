/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * Logger.cs: Logger for MONGODB & Log4Net
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using MongoDB.Bson.Serialization.Attributes;
using System;

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
