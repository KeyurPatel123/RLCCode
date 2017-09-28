using ElCamino.AspNetCore.Identity.AzureTable;
using ElCamino.AspNetCore.Identity.AzureTable.Model;

namespace Abiomed.DotNetCore.Storage
{

    public class ApplicationDbContext : IdentityCloudContext
    {
        public ApplicationDbContext() : base() { }

        public ApplicationDbContext(IdentityConfiguration config) : base(config) { }
    }
}
