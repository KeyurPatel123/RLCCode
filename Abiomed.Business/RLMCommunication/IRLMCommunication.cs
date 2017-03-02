/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * IRLMCommunication.cs: Interface of RLM Communications 
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/
using Abiomed.Models;
using System;
using System.Collections.Generic;

namespace Abiomed.Business
{
    public interface IRLMCommunication
    {
        event EventHandler SendMessage;

        byte[] ProcessMessage(string deviceId, byte[] dataMessage,out RLMStatus status);

        List<byte[]> SeperateMessages(byte[] dataMessage);

        bool StartVideo(string serialNumber);

        void UpdateSubscribedServers();
    }
}
