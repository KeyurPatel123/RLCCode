
using Abiomed.RLR.Communications;
using Abiomed.Business;

using Autofac;
using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Abiomed.Repository;
using Abiomed.Models;


namespace Abiomed.DependencyInjection 
{
    public class AutofacContainer
    {
        public static IContainer Container { get; set; }

        public void Build()
        {            
            log4net.Config.XmlConfigurator.Configure();

            var builder = new ContainerBuilder();
                  
            builder.RegisterType<MongoRepository>().As<IMongoDbRepository>();

            builder.RegisterInstance(LogManager.GetLogger(@"Logger")).As<ILog>();

            builder.RegisterType<TCPServer>().As<ITCPServer>();
            builder.RegisterType<RLMCommunication>().As<IRLMCommunication>();
            builder.RegisterType<RLMDeviceList>();

            Container = builder.Build();

            ILog logger = Container.Resolve<ILog>();
            logger.Info(@"Starting Service");
        }     
    }
}
