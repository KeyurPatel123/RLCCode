/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * RedisDbContext.cs: Redis DB Context REPO
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using Abiomed.DotNetCore.Models;
using System;
using StackExchange.Redis;

namespace Abiomed.DotNetCore.Repository
{
    
    public class RedisDbContext
    {
        private Configuration _configuration;
        
        private string _connectionString = "";

        public RedisDbContext(Configuration configuration)
        {
            _connectionString = configuration.RedisConnect;

            lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                return ConnectionMultiplexer.Connect(_connectionString);
            });
        }

        private Lazy<ConnectionMultiplexer> lazyConnection; 

        public ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }
    }
}
