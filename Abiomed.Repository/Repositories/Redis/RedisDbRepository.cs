/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * RedisDbRepository.cs: Redis Repo
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Abiomed.CLR.Models;
using StackExchange.Redis;
using System.Reflection;
using Abiomed.Models;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Abiomed.Repository
{
    public class RedisDbRepository<T> : IRedisDbRepository<T>
    {
        private readonly IDatabase _db;
        private readonly IServer _server;
        private readonly ISubscriber _subscriber;
        private ConnectionMultiplexer _connectionMultiplexer;
        private Configuration _configuration;

        public RedisDbRepository(Configuration configuration)
        {            
            _connectionMultiplexer = ConnectionMultiplexer.Connect(configuration.RedisConnect);

            var endPoints = _connectionMultiplexer.GetEndPoints();
            _server = _connectionMultiplexer.GetServer(endPoints[0]);            
            _db = _connectionMultiplexer.GetDatabase();
            _subscriber = _connectionMultiplexer.GetSubscriber();
        }

        #region Get Save Delete HASH
        public T GetHash(string key)
        {
            key = GenerateKey(key);
            var hash = _db.HashGetAll(key);

            return MapFromHash(hash);
        }

        public void SaveHash(string key, T obj)
        {
            if (obj != null)
            {
                var hash = GenerateRedisHash(obj);
                key = GenerateKey(key);

                _db.HashSet(key, hash);               
            }            
        }

        public void DeleteHash(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("invalid key");

            key = GenerateKey(key);
            _db.KeyDelete(key);
        }
        #endregion

        #region Get Save Delete SET
        public string[] GetSet(string key)
        {
            var members = _db.SetMembers(key).ToStringArray();
            
            return members;
        }

        public void AddToSet(string set, string value)
        {
            _db.SetAdd(set, value);            
        }

        public void RemoveFromSet(string set, string value)
        {
            _db.SetRemove(set, value);
        }

        #endregion

        #region String

        public void StringSet(string key, T data)
        {
            key = GenerateKey(key);

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(stream, data);
                bytes = stream.ToArray();
            }

            _db.StringSet(key, bytes);
        }

        public T StringGet(string key)
        {
            T returnObject = default(T);
            key = GenerateKey(key);
            byte[] bytes = (byte[])_db.StringGet(key);

            if (bytes != null)
            {
                using (var stream = new MemoryStream(bytes))
                {
                    returnObject = (T)new BinaryFormatter().Deserialize(stream);
                }
            }
            
            return returnObject;
        }

        public void StringDelete(string key)
        {
            key = GenerateKey(key);
        }

        public bool StringKeyExist(string key)
        {
            var members = _db.SetMembers(Definitions.RLMDeviceSet).ToStringArray();

            bool keyExist = false;
            if (members.Contains(key))
            {
                keyExist = true;
            }           
            return keyExist;
        }

        #endregion

        #region Publish Subscribe

        public void Publish(RedisChannel channel, RedisValue msg)
        {
            try
            {
                _subscriber.Publish(channel, msg);
            } catch (Exception EX)
            {
                _subscriber.Publish(channel, msg);
            }
        }

        public void Subscribe(RedisChannel channel, Action<RedisChannel, RedisValue> callback)
        {
            try
            {
                _subscriber.Subscribe(channel, callback);
            } catch (Exception EX)
            {
                // Retry
                _subscriber.Subscribe(channel, callback);
            }
        }

        public void Subscribe(List<RedisChannel> channel, Action<RedisChannel, RedisValue> callback)
        {
            foreach(var c in channel)
            {
                _subscriber.Subscribe(c, callback);
            }            
        }

        #endregion
        #region Helpers

        //generate a key from a given key and the class name of the object we are storing
        string GenerateKey(string key) =>
            string.Concat(key.ToLower(), ":", NameOfT.ToLower());

        //create a hash entry array from object using reflection
        HashEntry[] GenerateRedisHash(T obj)
        {
            var props = PropertiesOfT;
            var hash = new HashEntry[props.Count()];

            for (int i = 0; i < props.Count(); i++)
                hash[i] = new HashEntry(props[i].Name, props[i].GetValue(obj).ToString());

            return hash;
        }

        //build object from hash entry array using reflection
        T MapFromHash(HashEntry[] hash)
        {
            var obj = (T)Activator.CreateInstance(TypeOfT);//new instance of T
            var props = PropertiesOfT;

            for (int i = 0; i < props.Count(); i++)
                for (int j = 0; j < hash.Count(); j++)
                    if (props[i].Name == hash[j].Name)
                    {
                        var val = hash[j].Value;
                        var type = props[i].PropertyType;

                        if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                            if (string.IsNullOrEmpty(val))
                                props[i].SetValue(obj, null);

                        if (type.IsEnum)
                        {                            
                            props[i].SetValue(obj, Enum.Parse(type, val));
                        }
                        else if(type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(List<>)))
                        {
                            props[i].SetValue(obj, Convert.ChangeType(val, type));
                        }
                        else
                        {
                            props[i].SetValue(obj, Convert.ChangeType(val, type));
                        }
                        
                    }

            return obj;
        }
       
        Type TypeOfT { get { return typeof(T); } }

        string NameOfT { get { return TypeOfT.FullName; } }

        PropertyInfo[] PropertiesOfT { get { return TypeOfT.GetProperties(); } }

        #endregion

    }
}