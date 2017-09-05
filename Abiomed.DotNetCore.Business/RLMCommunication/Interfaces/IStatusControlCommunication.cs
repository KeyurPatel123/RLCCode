/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * IStatusControl.cs: Interface Status Control
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using Abiomed.DotNetCore.Models;

namespace Abiomed.DotNetCore.Business
{
    public interface IStatusControlCommunication
    {
        #region Receiving
        byte[] StatusResponse(string deviceIpAddress, byte[] message, out RLMStatus status);
        byte[] BearerAuthenticationUpdateResponse(string deviceIpAddress, byte[] message, out RLMStatus status);
        byte[] BearerAuthenticationReadResponse(string deviceIpAddress, byte[] message, out RLMStatus status);
        byte[] BearerPriorityConfirm(string deviceIpAddress, byte[] message, out RLMStatus status);
        byte[] LimitWarningRequest(string deviceIpAddress, byte[] message, out RLMStatus status);
        byte[] LimitCriticalRequest(string deviceIpAddress, byte[] message, out RLMStatus status);
        #endregion

        #region Sending
        byte[] StatusIndication(string deviceIpAddress);
        byte[] BearerAuthenticationUpdateIndication(string deviceIpAddress, WifiCredentials wifiCredentials);
        byte[] BearerSlotDelete(string deviceIpAddress, WifiCredentials wifiCredentials);

        byte[] BearerPriorityIndication(string deviceIpAddress, BearerPriority bearerPriority);
        byte[] BearerAuthenticationReadIndication(string deviceIpAddress);
        #endregion
    }
}
