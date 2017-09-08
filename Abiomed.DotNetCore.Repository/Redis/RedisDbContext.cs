/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * RedisDbContext.cs: Redis DB Context REPO
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using Abiomed.DotNetCore.Configuration;
using System;
using StackExchange.Redis;

namespace Abiomed.DotNetCore.Repository
{
    
    public class RedisDbContext
    {
        private IConfigurationCache _configurationCache;
        private string _connectionString = string.Empty;

        public RedisDbContext(IConfigurationCache configurationCache)
        {
            string redisConnect = string.Empty;
            _configurationCache.GetConfigurationItem("connectionmanager", "redisconnect", out _connectionString);

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
