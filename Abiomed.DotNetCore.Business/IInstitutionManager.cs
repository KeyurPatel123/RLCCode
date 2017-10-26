using Abiomed.DotNetCore.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abiomed.DotNetCore.Business
{
    public interface IInstitutionManager
    {
        List<Institution> GetInstitutions();
        List<Institution> GetInstitution(string sapCustomerId);
    }
}
