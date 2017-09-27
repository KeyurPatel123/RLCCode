/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * IRedisDbRepository.cs: Interface of Redis Repo
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abiomed.DotNetCore.Repository
{
    public interface IRedisDbRepository<T>
    {
        /// <summary>
        /// Get an object stored in redis by key
        /// </summary>
        /// <param name="key">The key used to stroe object</param>
        Task<T> GetHashAsync(string key);
        T GetHash(string key);

        /// <summary>
        /// Save an object in redis
        /// </summary>
        /// <param name="key">The key to stroe object against</param>
        /// <param name="obj">The object to store</param>
        Task SaveHashAsync(string key, T obj);
        void SaveHash(string key, T obj);

        /// <summary>
        /// Delete an object from redis using a key
        /// </summary>
        /// <param name="key">The key the object is stored using</param>
        Task DeleteHashAsync(string key);
        void DeleteHash(string key);
        /// <summary>
        /// Get set
        /// </summary>
        /// <param name="key"></param>
        Task<string[]> GetSetAsync(string key);
        string[] GetSet(string key);

        /// <summary>
        /// Add to set
        /// </summary>
        /// <param name="key"></param>
        Task AddToSetAsync(string set, string value);
        void AddToSet(string set, string value);

        /// <summary>
        /// Remove key from set
        /// </summary>
        /// <param name="key"></param>
        Task RemoveFromSetAsync(string set, string value);
        void RemoveFromSet(string set, string value);

        /// <summary>
        /// String Set
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        Task StringSetAsync(string key, T value);
        Task StringSetAsync(string key, string value);
        void StringSet(string key, T value);
        void StringSet(string key, string value);


        /// <summary>
        /// String Get
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<T> StringGetAsync(string key);
        T StringGet(string key);

        /// <summary>
        /// Delete String
        /// </summary>
        /// <param name="key"></param>
        Task StringDeleteAsync(string key);

        void StringDelete(string key);

        /// <summary>
        /// Check if key exists
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<bool> StringKeyExistAsync(string key);
        bool StringKeyExist(string key);
        /// <summary>
        /// Publish to a channel
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="msg"></param>
        Task PublishAsync(RedisChannel channel, RedisValue msg);

        void Publish(RedisChannel channel, RedisValue msg);

        /// <summary>
        /// Subscribe to a channel with callback
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="callback"></param>
        Task SubscribeAsync(RedisChannel channel, Action<RedisChannel, RedisValue> callback);

        void Subscribe(RedisChannel channel, Action<RedisChannel, RedisValue> callback);

        /// <summary>
        /// Subscribe to multiple channels with the same callback
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="callback"></param>
        Task SubscribeAsync(List<RedisChannel> channel, Action<RedisChannel, RedisValue> callback);

        void Subscribe(List<RedisChannel> channel, Action<RedisChannel, RedisValue> callback);

    }
}