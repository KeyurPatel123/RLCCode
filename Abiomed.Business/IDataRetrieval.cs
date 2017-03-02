/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * IDataRetrieval.cs: Interface for Data Retrieval
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/
using Abiomed.Models;
using Abiomed.Repository;

namespace Abiomed.Business
{
    public interface IDataRetrieval
    {
        GetManyResult<log> GetLogs(int limit = -1);
    }
}
