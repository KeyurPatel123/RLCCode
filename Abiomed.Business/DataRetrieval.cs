using Abiomed.Models;
using Abiomed.Repository;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abiomed.Business
{
    public class DataRetrieval : IDataRetrieval
    {        
        private ILog _log;
        private IMongoDbRepository _mongoDbRepository;

        public DataRetrieval(ILog logger, IMongoDbRepository mongoDbRepository)
        {
            _log = logger;
            _mongoDbRepository = mongoDbRepository;
        }

        public GetManyResult<log> GetLogs(int limit = -1)
        {
            GetManyResult<log> results = new GetManyResult<log>();
            try
            {
                Task<GetManyResult<log>> task = Task.Run(() => GetLogData(limit));
                task.Wait();
                results = task.Result;
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Error retrieving log limit {0}, Exception {1}", limit, e.ToString());
            }

            return results;
        }

        private GetManyResult<log> GetLogData(int Limit)
        {
            GetManyResult<log> x = _mongoDbRepository.GetAll<log>(Limit).Result;
            return x;
        }
    }
}
