using Abiomed.Business;
using Abiomed.Models;
using Abiomed.Repository;
using System;

using System.Threading.Tasks;
using System.Web.Http;

namespace Abiomed.RLR.API.API
{   
    public class LogController : ApiController
    {
        IDataRetrieval _dataRetrieval;

        public LogController(IDataRetrieval dataRetrieval)
        {
            _dataRetrieval = dataRetrieval;
        }

        [HttpGet]
        // GET api/<controller>
        public GetManyResult<log> Get()
        {
            GetManyResult<log> results = _dataRetrieval.GetLogs();
            
            return results;
        }

        [HttpGet]
        // GET api/log?limit=5
        public GetManyResult<log> Get([FromUri] int limit)
        {
            GetManyResult<log> results = _dataRetrieval.GetLogs(limit);            
            return results;
        }
        
        // POST api/<controller>
        public void Post([FromBody]string value)
        {

        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}