/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * AutofacContainer.cs: Autofac Container for RLR
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/
using Abiomed.Business;
using Abiomed.Models;
using Abiomed.Repository;
using Autofac;
using Autofac.Integration.WebApi;
using System.Reflection;
using System.Web.Http;

namespace Abiomed.Web
{

    public class AutofacContainer
    {
        public IContainer Container { get; set; }

        public AutofacContainer()
        {

            var builder = new ContainerBuilder();

            builder.RegisterType<EventManager>().As<IEventManager>().AutoActivate();
            builder.RegisterType<LogManager>().As<ILogManager>().SingleInstance();
            builder.RegisterGeneric(typeof(DocumentDBRepository<>)).SingleInstance();
            builder.RegisterGeneric(typeof(RedisDbRepository<>)).As(typeof(IRedisDbRepository<>)).SingleInstance();
            builder.RegisterType<Configuration>();
            builder.RegisterType<DeviceStatusManager>().As<IDeviceStatusManager>().SingleInstance();

            // Get your HttpConfiguration.
            var config = GlobalConfiguration.Configuration;

            // Register your Web API controllers.
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            // OPTIONAL: Register the Autofac filter provider.
            builder.RegisterWebApiFilterProvider(config);

            // Set the dependency resolver to be Autofac.
            Container = builder.Build();

            config.DependencyResolver = new AutofacWebApiDependencyResolver(Container);            
        }
    }
}