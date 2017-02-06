using Abiomed.Models;
using Abiomed.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abiomed.Business
{
    public interface IDataRetrieval
    {
        GetManyResult<log> GetLogs(int limit = -1);
    }
}
