using System.Threading.Tasks;
using System.Collections.Generic;

namespace Abiomed.DotNetCore.Business
{
    public interface IMediaManager
    {
        Task<List<string>> GetLiveStreamsAsync();
        Task<string> GetImageTextAsync(string serialNumber);
    }
}