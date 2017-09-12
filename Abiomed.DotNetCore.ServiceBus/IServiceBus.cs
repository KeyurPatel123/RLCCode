using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace Abiomed.DotNetCore.ServiceBus
{
    public interface IServiceBus
    {
        Task CloseAsync();
        IQueueClient GetQueueClient();
        Task SendMessageAsync<T>(T objectToAdd);
    }
}
