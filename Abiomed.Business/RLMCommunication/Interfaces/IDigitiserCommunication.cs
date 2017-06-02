/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * IStatusControl.cs: Interface Status Control
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using Abiomed.Models;

namespace Abiomed.Business
{
    public interface IDigitiserCommunication
    {
        #region Receiving
        byte[] StreamingVideoControlResponse(string deviceIpAddress, byte[] message, out RLMStatus status);
        byte[] BufferStatusRequest(string deviceIpAddress, byte[] message, out RLMStatus status);
        byte[] ScreenCaptureResponse(string deviceIpAddress, byte[] message, out RLMStatus status);
        #endregion

        #region Sending
        byte[] StreamingVideoControlIndication(string deviceIpAddress);
        byte[] ScreenCaptureIndication(string deviceIpAddress);
        byte[] ImageStop(string deviceIpAddress);
        byte[] VideoStop(string deviceIpAddress);
        
        #endregion
    }
}
