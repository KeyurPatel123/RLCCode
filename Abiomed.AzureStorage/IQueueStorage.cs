using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Abiomed.DotNetCore.Storage
{
    public interface IQueueStorage
    {
        Task AddMessageAsync<T>(T objectToAdd);
        Task<string> PeekMessageAsync();
        Task<List<string>> PeekMessagesAsync(int numberOfMessagesToPeek);
        Task RetrieveMessageAsync();
        Task DeleteRetrievedMessageAsync();
        string GetMessageContent();
        Task ChangeQueueAsync(string newQueueName);
        string GetMyQueueName();
    }
}
