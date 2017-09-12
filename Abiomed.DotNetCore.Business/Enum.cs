using System;
using System.Collections.Generic;
using System.Text;

namespace Abiomed.DotNetCore.Business
{
    public enum EmailServiceActor
    {
        Unknown = -1,
        Broadcaster = 0,
        Listener = 1
    }

    public enum EmailServiceType
    {
        Unknown = -1,
        ServiceBus = 0,
        Queue = 1
    }

}
