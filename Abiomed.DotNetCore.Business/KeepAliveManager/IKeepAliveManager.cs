/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * IKeepAliveManager.cs: Interface Keep Alive Manager
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/
namespace Abiomed.DotNetCore.Business
{
    public interface IKeepAliveManager
    {
        void Add(string deviceIpAddress);
        void Remove(string deviceIpAddress);
        void Ping(string deviceIpAddress);

        void ImageTimerAdd(string deviceIpAddress);

        void ImageTimerDelete(string deviceIpAddress);
    }
}
