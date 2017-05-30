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
        byte[] ProcessMessage(string deviceIpAddress, byte[] dataMessage,out RLMStatus status);

        byte[] ProcessEvent(string deviceIpAddress, string message, string[] options);

        byte[] GenerateCloseSession(string deviceIpAddress);

        void RemoveRLMDeviceFromList(string deviceIpAddress);

        List<byte[]> SeperateMessages(string deviceIpAddress, byte[] dataMessage);
    }
}
