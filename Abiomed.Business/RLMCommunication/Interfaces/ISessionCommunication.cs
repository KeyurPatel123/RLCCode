/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * ISessionCommunication.cs: Interface for Session
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/
using Abiomed.Models;

namespace Abiomed.Business
{
    public interface ISessionCommunication
    {
        #region Receiving
        byte[] SessionRequest(string deviceIpAddress, byte[] message, out RLMStatus status);

        byte[] BearerRequest(string deviceIpAddress, byte[] message, out RLMStatus status);

        byte[] SessionCloseResponse(string deviceIpAddress, byte[] message, out RLMStatus status);

        byte[] CloseBearerRequest(string deviceIpAddress, byte[] message, out RLMStatus status);

        byte[] KeepAliveRequest(string deviceIpAddress, byte[] message, out RLMStatus status);

        byte[] SessionCloseRequest(string deviceIpAddress, byte[] message, out RLMStatus status);
        #endregion

        #region Sending
        byte[] BearerChangeIndication(string deviceIpAddress, Definitions.Bearer bearer);
        byte[] KeepAliveIndication(string deviceIpAddress);
        byte[] SessionCloseIndication(string deviceIpAddress);
        #endregion
        
    }
}
