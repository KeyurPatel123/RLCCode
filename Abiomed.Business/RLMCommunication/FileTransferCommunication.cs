/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * FileTransferCommunication.cs: File Transfer Communication Business
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/
using System;
using Abiomed.Models;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Abiomed.Repository;
using System.IO;
using System.Text;

namespace Abiomed.Business
{
    public class FileTransferCommunication : IFileTransferCommunication
    {
        private ILogManager _logManager;
        private RLMDeviceList _rlmDeviceList;
        private Configuration _configuration;
        private IRedisDbRepository<RLMImage> _redisDbRepository;
        private IKeepAliveManager _keepAliveManager;


        public FileTransferCommunication(ILogManager logManager, RLMDeviceList rlmDeviceList, Configuration configuration, IRedisDbRepository<RLMImage> redisDbRepository, IKeepAliveManager keepAliveManager)
        {
            _logManager = logManager;
            _rlmDeviceList = rlmDeviceList;
            _configuration = configuration;
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

            byte[] returnMessage = new byte[0];
            try
            {
                fileOpenResponse.Status = BitConverter.ToUInt16(message.Skip(6).Take(2).Reverse().ToArray(), 0);
                fileOpenResponse.UserRef = BitConverter.ToUInt16(message.Skip(8).Take(2).Reverse().ToArray(), 0);
                fileOpenResponse.Size = BitConverter.ToUInt32(message.Skip(10).Take(4).Reverse().ToArray(), 0);
                var secondTimeDifference = BitConverter.ToUInt64(message.Skip(14).Take(8).Reverse().ToArray(), 0);

                RLMDevice rlmDevice;
                _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);

                // Error checking
                if (fileOpenResponse.Status != Definitions.SuccessStats || fileOpenResponse.UserRef != Definitions.UserRef)
                {
                    status.Status = RLMStatus.StatusEnum.Failure;
                    Trace.TraceInformation(@"Open File Response Failure {0}", rlmDevice.SerialNo);
                }
                else
                {
                    Trace.TraceInformation(@"Open File Response {0}", rlmDevice.SerialNo);

                    //Last modification time of file, in seconds, since the UNIX epoch which will be zero if an error occurs
                    var epochTime = new DateTime(1970, 1, 1);                    
                    fileOpenResponse.Time = epochTime.AddSeconds(secondTimeDifference);

                    _logManager.Create(deviceIpAddress, rlmDevice.SerialNo, fileOpenResponse, Definitions.LogMessageType.FileOpenResponse);

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
            catch (Exception)
            {
               // Trace.TraceInformation(@"File Open Response Failure {0} Exception {1}", _RLMDeviceList.RLMDevices[deviceIpAddress].SerialNo, e.ToString());
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
            
            try
            {
                RLMDevice rlmDevice;
                _rlmDeviceList.RLMDevices.TryGetValue(deviceIpAddress, out rlmDevice);

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
                        Trace.TraceInformation(@"Data Read Response - Final - Failure {0}", deviceIpAddress);
                        status = new RLMStatus() { Status = RLMStatus.StatusEnum.Failure };
                    }
                }
                else // copy data and request next block
                {
                    var copyStatus = CopyDataFromTransfer(rlmDevice, message);
                    if(copyStatus)
                    {
                        returnMessage = GenerateBlock(rlmDevice);
                    }
                    else
                    {
                        Trace.TraceInformation(@"Data Read Response Failure {0}", deviceIpAddress);
                        status = new RLMStatus() { Status = RLMStatus.StatusEnum.Failure };
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceInformation(@"Data Read Response Failure {0} Exception {1}", deviceIpAddress, e.ToString());
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
               
                // Error checking
                if (clearFileResponse.Status != Definitions.SuccessStats || clearFileResponse.UserRef != Definitions.UserRef)
                {
                    status.Status = RLMStatus.StatusEnum.Failure;
                    Trace.TraceInformation(@"Clear File Response Failure {0}", rlmDevice.SerialNo);
                }
                else
                {
                    Trace.TraceInformation(@"Clear File Response {0}", rlmDevice.SerialNo);
                    _logManager.Create(deviceIpAddress, rlmDevice.SerialNo, clearFileResponse, Definitions.LogMessageType.ScreenCaptureResponse);
                }
            }
            catch (Exception e)
            {
                Trace.TraceInformation(@"Clear File Response Failure {0} Exception {1}", deviceIpAddress, e.ToString());
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

        public byte[] CloseFileInfication(string deviceIpAddress)
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
                Trace.TraceInformation(@"Data Transfer Failure {0} - Status {1}", rlmDevice.SerialNo, dataReadResponse.Status);
            }
            else
            {
                Trace.TraceInformation(@"Successful Data Transfer {0}", rlmDevice.SerialNo);
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
            catch(Exception)
            {

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
            fileName.Append(".txt");

            try
            {
                byte[] data = rlmDevice.DataTransfer.ToArray();
                using (var fs = new FileStream(fileName.ToString(), FileMode.Create, FileAccess.Write))
                {
                    fs.Write(data, 0, data.Length);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in process: {0}", ex);
                return false;
            }
        }
    }
}