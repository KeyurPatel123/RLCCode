using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace Abiomed.DotNetCore.Communication
{
    public interface IServiceBus
    {
        Task CloseAsync();
        IQueueClient GetQueueClient();
        Task SendMessageAsync<T>(T objectToAdd);
    }
}
