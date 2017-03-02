/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * AutofacContainer.cs: Autofac Container for RLR
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/
using Abiomed.Business;
using Abiomed.RLR.Communications;
using Abiomed.Models;
using Abiomed.Repository;
using Autofac;
using Autofac.Integration.WebApi;
using log4net;
using System.Reflection;
using System.Web.Http;

namespace Abiomed.RLR.API
{

    public class AutofacContainer
    {
        public static IContainer Container { get; set; }

        public AutofacContainer()
        {
            log4net.Config.XmlConfigurator.Configure();

            var builder = new ContainerBuilder();

            builder.RegisterType<MongoRepository>().As<IMongoDbRepository>();

            builder.RegisterInstance(LogManager.GetLogger(@"Logger")).As<ILog>();

            builder.RegisterType<TCPServer>().As<ITCPServer>();
            builder.RegisterType<RLMCommunication>().As<IRLMCommunication>();
            builder.RegisterType<RLMDeviceList>();
            builder.RegisterType<DataRetrieval>().As<IDataRetrieval>();

            // Get your HttpConfiguration.
            var config = GlobalConfiguration.Configuration;

            // Register your Web API controllers.
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            // OPTIONAL: Register the Autofac filter provider.
            builder.RegisterWebApiFilterProvider(config);

            // Set the dependency resolver to be Autofac.
            Container = builder.Build();

            config.DependencyResolver = new AutofacWebApiDependencyResolver(Container);

            //ILog logger = Container.Resolve<ILog>();
            //logger.Info("Started RLR Web Service");
        }
    }
}