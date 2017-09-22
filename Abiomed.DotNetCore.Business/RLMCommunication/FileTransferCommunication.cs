/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * FileTransferCommunication.cs: File Transfer Communication Business
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/
using System;
using Abiomed.DotNetCore.Models;
using System.Linq;
using System.Collections.Generic;
using Abiomed.DotNetCore.Repository;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Abiomed.DotNetCore.Business
{
    public class FileTransferCommunication : IFileTransferCommunication
    {
        private ILogger<IFileTransferCommunication> _logger;
        private RLMDeviceList _rlmDeviceList;
        private IRedisDbRepository<RLMImage> _redisDbRepository;
        private IKeepAliveManager _keepAliveManager;


        public FileTransferCommunication(ILogger<IFileTransferCommunication> logger, RLMDeviceList rlmDeviceList, IRedisDbRepository<RLMImage> redisDbRepository, IKeepAliveManager keepAliveManager)
        {
            _logger = logger;
            _rlmDeviceList = rlmDeviceList;
            _redisDbRepository = redisDbRepository;
            _keepAliveManager = keepAliveManager;
        }

        #region Receiving
        public byte[] OpenFileRequest(string deviceIpAddress, byte[] message, out RLMStatus status)
        {
            throw new NotImplementedException();
        }

        public byte[] OpenFileResponse(string deviceIpAddress, byte[] message, out RLMStatus status)
        {
            status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };
            FileOpenResponse fileOpenResponse = new FileOpenResponse();
            string deviceSerialNumber = string.Empty;
            byte[] returnMessage = new byte[0];

            try
            {
                fileOpenResponse.Status = BitConverter.ToUInt16(message.Skip(6).Take(2).Reverse().ToArray(), 0);
                fileOpenResponse.UserRef = BitConverter.ToUInt16(message.Skip(8).Take(2).Reverse().ToArray(), 0);
                fileOpenResponse.Size = BitConverter.ToUInt32(message.Skip(10).Take(4).Reverse().ToArray(), 0);
                var secondTimeDifference = BitConverter.ToUInt64(message.Skip(14).Take(8).Reverse().ToArray(), 0);

                RLMDevice rlmDevice;
                _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);
                deviceSerialNumber = rlmDevice.SerialNo;

                // Error checking
                if (fileOpenResponse.Status != Definitions.SuccessStats || fileOpenResponse.UserRef != Definitions.UserRef)
                {
                    status.Status = RLMStatus.StatusEnum.Failure;
                    _logger.LogInformation("Open File Response Failure {0}", rlmDevice.SerialNo);
                }
                else
                {
                    //Last modification time of file, in seconds, since the UNIX epoch which will be zero if an error occurs
                    var epochTime = new DateTime(1970, 1, 1);
                    fileOpenResponse.Time = epochTime.AddSeconds(secondTimeDifference);

                    _logger.LogInformation("Open File Response {0}", rlmDevice.SerialNo);

                    // Calculate amount of blocks, size in bytes
                    // number_of_blocks = (int)(file_size + 999) / 1000; 
                    rlmDevice.TotalBlocks = (int)((fileOpenResponse.Size + 999) / 1000);
                    rlmDevice.Block = 0;

                    // Create new List
                    rlmDevice.DataTransfer = new List<byte>();
                    rlmDevice.FileTransferSize = fileOpenResponse.Size;

                    returnMessage = GenerateBlock(rlmDevice);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("File Open Response Failure {0} Exception {1}", deviceSerialNumber, e.ToString());
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Failure };
            }
            return returnMessage;
        }

        public byte[] DataReadRequest(string deviceIpAddress, byte[] message, out RLMStatus status)
        {
            throw new NotImplementedException();
        }

        public byte[] DataReadResponse(string deviceIpAddress, byte[] message, out RLMStatus status)
        {
            byte[] returnMessage = new byte[0];
            status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };
            string deviceSerialNumber = string.Empty;

            try
            {
                RLMDevice rlmDevice;
                _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);
                deviceSerialNumber = rlmDevice.SerialNo;

                // Check if full if so send messages to close file and clear
                if (rlmDevice.Block == rlmDevice.TotalBlocks)
                {
                    // Get last piece of data
                    var validCopy = CopyDataFromTransfer(rlmDevice, message);

                    if (validCopy)
                    {
                        // Clean up
                        rlmDevice.TotalBlocks = 0;
                        rlmDevice.Block = 0;

                        byte[] clearFileMessage = new byte[0];

                        // Depending on file transfer take different actions
                        if (rlmDevice.FileTransferType == Definitions.RLMFileTransfer.ScreenCapture0)
                        {
                            // Send Publish Message of new Image available, and save it off somewhere!
                            CreateImage(rlmDevice.DataTransfer.ToArray(), rlmDevice.SerialNo);

                            clearFileMessage = General.GenerateRequest(Definitions.ClearScreenFileIndication, rlmDevice);

                            // Start Timer for next Image
                            _keepAliveManager.ImageTimerAdd(deviceIpAddress);
                        }
                        else // Must be log
                        {
                            clearFileMessage = General.GenerateRequest(Definitions.ClearRLMLogFileIndication, rlmDevice);
                            ByteArrayToFile(rlmDevice);
                        }

                        // Send Close Indication and clear 
                        var closeFileMessage = General.GenerateRequest(Definitions.CloseFileIndication, rlmDevice);

                        // Append to current Byte[]
                        returnMessage = new byte[closeFileMessage.Length + clearFileMessage.Length];
                        closeFileMessage.CopyTo(returnMessage, 0);
                        clearFileMessage.CopyTo(returnMessage, closeFileMessage.Length);
                    }
                    else
                    {
                        _logger.LogInformation("Data Read Response - Final - Failure {0}", deviceIpAddress);
                        status = new RLMStatus() { Status = RLMStatus.StatusEnum.Failure };
                    }
                }
                else // copy data and request next block
                {
                    var copyStatus = CopyDataFromTransfer(rlmDevice, message);
                    if (copyStatus)
                    {
                        returnMessage = GenerateBlock(rlmDevice);
                    }
                    else
                    {
                        _logger.LogInformation("Data Read Response - Failure {0}", deviceIpAddress);
                        status = new RLMStatus() { Status = RLMStatus.StatusEnum.Failure };
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Data Read Response Failure {0} Exception {1}", deviceIpAddress, e.ToString());
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Failure };
            }

            return returnMessage;
        }

        public byte[] ClearFileResponse(string deviceIpAddress, byte[] message, out RLMStatus status)
        {
            status = new RLMStatus() { Status = RLMStatus.StatusEnum.Success };

            RLMDevice rlmDevice;
            _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);


            byte[] returnMessage = new byte[0];
            try
            {
                ClearFileResponse clearFileResponse = new ClearFileResponse();

                clearFileResponse.Status = BitConverter.ToUInt16(message.Skip(6).Take(2).Reverse().ToArray(), 0);
                clearFileResponse.UserRef = BitConverter.ToUInt16(message.Skip(8).Take(2).Reverse().ToArray(), 0);

                string traceMessage = string.Empty;
                // Error checking
                if (clearFileResponse.Status != Definitions.SuccessStats || clearFileResponse.UserRef != Definitions.UserRef)
                {
                    status.Status = RLMStatus.StatusEnum.Failure;
                    traceMessage = string.Format(@"Clear File Response Failure {0}", rlmDevice.SerialNo);
                }
                else
                {
                    traceMessage = string.Format(@"Clear File Response {0}", rlmDevice.SerialNo);
                }
                _logger.LogInformation(traceMessage);
            }
            catch (Exception e)
            {
                _logger.LogError("Clear File Response Failure {0} Exception {1}", deviceIpAddress, e.ToString());
                status = new RLMStatus() { Status = RLMStatus.StatusEnum.Failure };
            }
            return returnMessage;

            throw new NotImplementedException();
        }
        #endregion

        #region Sending
        public byte[] OpenFileConfirm(string deviceIpAddress)
        {
            throw new NotImplementedException();
        }

        public byte[] OpenRLMLogFileIndication(string deviceIpAddress)
        {
            RLMDevice rlmDevice;
            _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);

            byte[] returnMessage = new byte[0];

            returnMessage = General.GenerateRequest(Definitions.OpenRLMLogFileIndication, rlmDevice);
            rlmDevice.FileTransferType = Definitions.RLMFileTransfer.RLMEventLog;

            return returnMessage;
        }

        public byte[] DataReadConfirm(string deviceIpAddress)
        {
            throw new NotImplementedException();
        }

        public byte[] DataReadIndication(string deviceIpAddress)
        {
            throw new NotImplementedException();
        }

        public byte[] CloseFileIndication(string deviceIpAddress)
        {
            throw new NotImplementedException();
        }

        public byte[] ClearFileIndication(string deviceIpAddress)
        {
            throw new NotImplementedException();
        }
        #endregion

        private byte[] GenerateBlock(RLMDevice rlmDevice)
        {
            var returnMessage = General.GenerateRequest(Definitions.DataReadIndication, rlmDevice);

            // Calculate # and adjust byte[] bytes 8 and 9
            byte[] currentBlock = BitConverter.GetBytes(rlmDevice.Block++); // Add to next block;

            // Update Message
            returnMessage[8] = currentBlock[3];
            returnMessage[9] = currentBlock[2];
            returnMessage[10] = currentBlock[1];
            returnMessage[11] = currentBlock[0];

            return returnMessage;
        }

        private bool CopyDataFromTransfer(RLMDevice rlmDevice, byte[] message)
        {
            bool status = true;

            DataReadResponse dataReadResponse = new DataReadResponse();
            dataReadResponse.Status = BitConverter.ToUInt16(message.Skip(6).Take(2).Reverse().ToArray(), 0);
            dataReadResponse.UserRef = BitConverter.ToUInt16(message.Skip(8).Take(2).Reverse().ToArray(), 0);
            dataReadResponse.Data = message.Skip(10).Take(message.Length - 10).ToArray();

            if (dataReadResponse.Status != Definitions.SuccessStats || dataReadResponse.UserRef != Definitions.UserRefFileTransfer)
            {
                status = false;
                _logger.LogInformation("Data Transfer Failure {0} - Status {1}", rlmDevice.SerialNo, dataReadResponse.Status);
            }
            else
            {
                _logger.LogInformation("Successful Data Transfer {0}", rlmDevice.SerialNo);
                rlmDevice.DataTransfer.AddRange(dataReadResponse.Data);
            }

            return status;
        }

        private void CreateImage(byte[] imageData, string serialNumber)
        {
            try
            {
                RLMImage rlmImage = new RLMImage { Data = imageData, SerialNumber = serialNumber };                
                _redisDbRepository.StringSet(serialNumber, rlmImage);                
                _redisDbRepository.Publish(Definitions.ImageCapture, serialNumber);                                
            }
            catch (Exception e)
            {
                _logger.LogError("Create Image Exception Exception {0}", e.ToString());
            }
        }

        private bool ByteArrayToFile(RLMDevice rlmDevice)
        {
            // Create Name : RLXXXXX_UTCTime
            StringBuilder fileName = new StringBuilder();
            fileName.Append(@"C:\\RLMLogs\");
            fileName.Append(rlmDevice.SerialNo);
            fileName.Append("-");
            fileName.Append(rlmDevice.ConnectionTime.ToString("yyyyMMdd_hhmmss"));
            fileName.Append(".gz");

            try
            {
                byte[] data = rlmDevice.DataTransfer.ToArray();
                using (var fs = new FileStream(fileName.ToString(), FileMode.Create, FileAccess.Write))
                {
                    fs.Write(data, 0, data.Length);
                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Exception Caught in Byte Array to File {0}", e.ToString());
                return false;
            }
        }
    }
}