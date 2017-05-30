using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abiomed.Models;
using Abiomed.Repository;
using Microsoft.Azure.Documents;

namespace Abiomed.Business
{
    public class LogManager : ILogManager
    {
        private DocumentDBRepository<Resource> _documentDBRepository;

        public LogManager(DocumentDBRepository<Resource>  documentDBRepository)
        {
            _documentDBRepository = documentDBRepository;
        }

        public async Task Create<T>(string deviceIpAddress, string rlmSerial, T message, Definitions.LogMessageType logMessageType)
        {
            Log<T> log = new Log<T>
            {
                DeviceIpAddress = deviceIpAddress,
                RLMSerial = rlmSerial,
                Message = message,
                LogMessageType = logMessageType
            };

            await _documentDBRepository.Create(log);
        }
    }
}
