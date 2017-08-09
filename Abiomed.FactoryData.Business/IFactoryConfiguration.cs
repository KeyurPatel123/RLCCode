using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abiomed.FactoryData
{
    public interface IFactoryConfiguration
    {
        Task SetFactoryData(bool isCloud = false);
    }
}
