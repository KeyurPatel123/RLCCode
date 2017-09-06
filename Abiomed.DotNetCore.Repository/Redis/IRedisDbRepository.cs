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

namespace Abiomed.DotNetCore.Repository
{
    public interface IRedisDbRepository<T>
    {
        /// <summary>
        /// Get an object stored in redis by key
        /// </summary>
        /// <param name="key">The key used to stroe object</param>
        T GetHash(string key);

        /// <summary>
        /// Save an object in redis
        /// </summary>
        /// <param name="key">The key to stroe object against</param>
        /// <param name="obj">The object to store</param>
        void SaveHash(string key, T obj);

        /// <summary>
        /// Delete an object from redis using a key
        /// </summary>
        /// <param name="key">The key the object is stored using</param>
        void DeleteHash(string key);

        /// <summary>
        /// Get set
        /// </summary>
        /// <param name="key"></param>
        string[] GetSet(string key);

        /// <summary>
        /// Add to set
        /// </summary>
        /// <param name="key"></param>
        void AddToSet(string set, string value);

        /// <summary>
        /// Remove key from set
        /// </summary>
        /// <param name="key"></param>
        void RemoveFromSet(string set, string value);

        /// <summary>
        /// String Set
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void StringSet(string key, T value);

        /// <summary>
        /// String Get
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        T StringGet(string key);

        /// <summary>
        /// Delete String
        /// </summary>
        /// <param name="key"></param>
        void StringDelete(string key);

        /// <summary>
        /// Check if key exists
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool StringKeyExist(string key);
        /// <summary>
        /// Publish to a channel
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="msg"></param>
        void Publish(RedisChannel channel, RedisValue msg);

        /// <summary>
        /// Subscribe to a channel with callback
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="callback"></param>
        void Subscribe(RedisChannel channel, Action<RedisChannel, RedisValue> callback);

        /// <summary>
        /// Subscribe to multiple channels with the same callback
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="callback"></param>
        void Subscribe(List<RedisChannel> channel, Action<RedisChannel, RedisValue> callback);

    }
}