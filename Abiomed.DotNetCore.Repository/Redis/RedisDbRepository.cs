/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * RedisDbRepository.cs: Redis Repo
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using Abiomed.DotNetCore.Models;
using Abiomed.DotNetCore.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Abiomed.DotNetCore.Repository
{
    public class RedisDbRepository<T> : IRedisDbRepository<T>
    {
        private readonly IDatabase _db;
        private readonly IServer _server;
        private readonly ISubscriber _subscriber;
        private IConfigurationCache _configurationCache;
        RedisDbContext redisDbContext;

        public RedisDbRepository(IConfigurationCache configurationCache)
        {
            _configurationCache = configurationCache;            

            string redisConnect = _configurationCache.GetConfigurationItem("connectionmanager", "redisconnect");

            redisDbContext = new RedisDbContext(_configurationCache);

            var endPoints = redisDbContext.Connection.GetEndPoints();
            _server = redisDbContext.Connection.GetServer(endPoints[0]);            
            _db = redisDbContext.Connection.GetDatabase();
            _subscriber = redisDbContext.Connection.GetSubscriber();           
        }

        #region Get Save Delete HASH
        public async Task<T> GetHashAsync(string key)
        {
            key = GenerateKey(key);
            var hash = await _db.HashGetAllAsync(key);

            return MapFromHash(hash);
        }

        public T GetHash(string key)
        {
            key = GenerateKey(key);
            var hash = _db.HashGetAll(key);

            return MapFromHash(hash);
        }

        public async Task SaveHashAsync(string key, T obj)
        {
            if (obj != null)
            {
                var hash = GenerateRedisHash(obj);
                key = GenerateKey(key);

                await _db.HashSetAsync(key, hash);
            }
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

        public async Task DeleteHashAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("invalid key");

            key = GenerateKey(key);
            await _db.KeyDeleteAsync(key);
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
        public async Task<string[]> GetSetAsync(string key)
        {
            var members = await _db.SetMembersAsync(key);            
            return members.ToStringArray();
        }

        public string[] GetSet(string key)
        {
            var members = _db.SetMembers(key).ToStringArray();            
            return members;
        }

        public async Task AddToSetAsync(string set, string value)
        {
            await _db.SetAddAsync(set, value);
        }

        public void AddToSet(string set, string value)
        {
            _db.SetAdd(set, value);            
        }

        public async Task RemoveFromSetAsync(string set, string value)
        {
            await _db.SetRemoveAsync(set, value);
        }

        public void RemoveFromSet(string set, string value)
        {
            _db.SetRemove(set, value);
        }

        #endregion

        #region String

        public async Task StringSetAsync(string key, T data)
        {
            key = GenerateKey(key);

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(stream, data);
                bytes = stream.ToArray();
            }

            await _db.StringSetAsync(key, bytes);
        }

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

        public async Task StringSetAsync(string key, string JSON)
        {
            key = GenerateKey(key);
            await _db.StringSetAsync(key, JSON);
        }

        public void StringSet(string key, string JSON)
        {
            key = GenerateKey(key);
            _db.StringSet(key, JSON);
        }

        public async Task<T> StringGetAsync(string key)
        {
            T returnObject = default(T);
            key = GenerateKey(key);
            byte[] bytes = await _db.StringGetAsync(key);

            if (bytes != null)
            {
                using (var stream = new MemoryStream(bytes))
                {
                    returnObject = (T)new BinaryFormatter().Deserialize(stream);
                }
            }

            return returnObject;
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

        public async Task StringDeleteAsync(string key)
        {
            key = GenerateKey(key);
            // todo figure out!
        }

        public void StringDelete(string key)
        {
            key = GenerateKey(key);
            // todo figure out!
        }

        public async Task<bool> StringKeyExistAsync(string key)
        {
            var membersAsync = await _db.SetMembersAsync(Definitions.RLMDeviceSet);            
            var members = membersAsync.ToStringArray();

            bool keyExist = false;
            if (members.Contains(key))
            {
                keyExist = true;
            }
            return keyExist;
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

        public async Task PublishAsync(RedisChannel channel, RedisValue msg)
        {
            await _subscriber.PublishAsync(channel, msg);            
        }

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

        public async Task SubscribeAsync(RedisChannel channel, Action<RedisChannel, RedisValue> callback)
        {
            await _subscriber.SubscribeAsync(channel, callback);
        }

        public void Subscribe(RedisChannel channel, Action<RedisChannel, RedisValue> callback)
        {
            _subscriber.Subscribe(channel, callback);
        }

        public async Task SubscribeAsync(List<RedisChannel> channel, Action<RedisChannel, RedisValue> callback)
        {
            foreach (var c in channel)
            {
                await _subscriber.SubscribeAsync(c, callback);
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