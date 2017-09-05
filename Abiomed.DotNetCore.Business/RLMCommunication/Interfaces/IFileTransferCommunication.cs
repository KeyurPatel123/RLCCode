/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * IFileTransferCommunication.cs: File Transfer Communication Interface
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using Abiomed.DotNetCore.Models;

namespace Abiomed.DotNetCore.Business
{
    public interface IFileTransferCommunication
    {
        #region Receiving
        byte[] OpenFileRequest(string deviceIpAddress, byte[] message, out RLMStatus status);
        byte[] OpenFileResponse(string deviceIpAddress, byte[] message, out RLMStatus status);
        byte[] DataReadRequest(string deviceIpAddress, byte[] message, out RLMStatus status);
        byte[] DataReadResponse(string deviceIpAddress, byte[] message, out RLMStatus status);
        byte[] ClearFileResponse(string deviceIpAddress, byte[] message, out RLMStatus status);
        #endregion

        #region Sending
        byte[] OpenFileConfirm(string deviceIpAddress);
        byte[] OpenRLMLogFileIndication(string deviceIpAddress);
        byte[] DataReadConfirm(string deviceIpAddress);
        byte[] DataReadIndication(string deviceIpAddress);
        byte[] CloseFileIndication(string deviceIpAddress);
        byte[] ClearFileIndication(string deviceIpAddress);
        #endregion
    }
}
