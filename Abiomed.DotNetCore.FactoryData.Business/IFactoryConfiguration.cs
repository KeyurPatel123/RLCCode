using System;
using System.Threading.Tasks;

namespace Abiomed.DotNetCore.FactoryData
{
    public interface IFactoryConfiguration
    {
        Task SetFactoryData(bool isCloud = false);
    }
}
