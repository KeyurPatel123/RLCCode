using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abiomed.Repository
{
    public class MongoDbContext
    {
        public const string CONNECTION_STRING_NAME = "MongoDbConnectionString";
        public const string DATABASE_NAME = "log4net"; //todo make in app settings for different db's logging, devices, users, etc.

        private static readonly IMongoClient _client;
        private static IMongoDatabase _database;

        static MongoDbContext()
        {
            var connectionString = ConfigurationManager.ConnectionStrings[CONNECTION_STRING_NAME].ConnectionString;            
            _client = new MongoClient();            
            _database = _client.GetDatabase(DATABASE_NAME); // Set default DB           
        }

        /// <summary>
        /// The private GetCollection method
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public IMongoCollection<TEntity> GetCollection<TEntity>()
        {                            
            return _database.GetCollection<TEntity>(typeof(TEntity).Name.ToLower() + "s");            
        }

        /// <summary>
        /// Sets/Updates working Database
        /// </summary>
        /// <param name="database"></param>
        public void SetCurrentDatabase(string database)
        {
            _database = _client.GetDatabase(database);
        }

        /// <summary>
        /// Returns current Database in use
        /// </summary>
        /// <returns></returns>
        public string GetCurrentDatabase()
        {
            return _database.DatabaseNamespace.DatabaseName;
        }
    }
}
