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
using Autofac.Integration.SignalR;
using Microsoft.AspNet.SignalR;
using System.Reflection;

namespace Abiomed.SignalRSelfHost
{

    public class AutofacContainer
    {
        public static IContainer Container { get; set; }


        public void Build()
        {

            var builder = new ContainerBuilder();

            // Register your SignalR hubs.
            builder.RegisterHubs(Assembly.GetExecutingAssembly()).SingleInstance();

            builder.RegisterType<Startup>().SingleInstance();
            builder.RegisterType<LogManager>().As<ILogManager>().SingleInstance();
            builder.RegisterGeneric(typeof(DocumentDBRepository<>)).SingleInstance();

            builder.RegisterGeneric(typeof(RedisDbRepository<>)).As(typeof(IRedisDbRepository<>)).SingleInstance();
            builder.RegisterType<Configuration>().SingleInstance();

            // Set the dependency resolver to be Autofac.
            Container = builder.Build();
            GlobalHost.DependencyResolver = new AutofacDependencyResolver(Container);

        }
    }
}