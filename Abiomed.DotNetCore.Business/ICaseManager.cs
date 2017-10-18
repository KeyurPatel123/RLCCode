using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Abiomed.DotNetCore.Models;
using Abiomed.DotNetCore.Repository;

namespace Abiomed.DotNetCore.Business
{
    public interface ICaseManager
    {
        //Task CleanupActiveCasesAsync(DateTime expiredTimeUtc);
        Case GetCase(string pumpSerialNumber);
        RemoteLinkCases GetAll();
        RemoteLinkCases GetUpdated();
        RemoteLinkCases GetUpdated(DateTime lastTimeCheckedUtc);
    }
}