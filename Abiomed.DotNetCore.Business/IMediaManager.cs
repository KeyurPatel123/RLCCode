using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Abiomed.DotNetCore.Models;

namespace Abiomed.DotNetCore.Business
{
    public interface IMediaManager
    {
        Task<List<string>> GetLiveStreamsAsync();
        Task<OcrResponse> GetImageTextAsync(string serialNumber, DateTime batchStartTimeUtc, bool applyMaskToImage = true);
    }
}