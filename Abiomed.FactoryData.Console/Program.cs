using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Abiomed.DependencyInjection;
using System.Diagnostics;
using Abiomed.FactoryData;

namespace Abiomed.FactoryData
{
    class Program
    {
        private static AutofacContainer autofac;

        static void Main(string[] args)
        {
            autofac = new AutofacContainer();
            autofac.Build();

            FactoryConfiguration factoryData = AutofacContainer.Container.Resolve<FactoryConfiguration>();
            factoryData.SetFactoryData().Wait();
        }
    }
}