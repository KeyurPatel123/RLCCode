/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * AutofacContainer.cs: Autofac Container
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using Abiomed.RLR.Communications;
using Abiomed.Business;
using Autofac;
using Abiomed.Repository;
using Abiomed.Models;

namespace Abiomed.DependencyInjection 
{
    public class AutofacContainer
    {
        public static IContainer Container { get; set; }

        public void Build()
        {            
            var builder = new ContainerBuilder();

            builder.RegisterType<LogManager>().As<ILogManager>().SingleInstance();
            builder.RegisterGeneric(typeof(DocumentDBRepository<>)).SingleInstance();
            builder.RegisterGeneric(typeof(RedisDbRepository<>)).As(typeof(IRedisDbRepository<>)).SingleInstance();

            builder.RegisterType<TCPServer>().As<ITCPServer>();
            builder.RegisterType<InsecureTcpServer>();
            builder.RegisterType<Configuration>().SingleInstance();            

            #region RLM Communications
            builder.RegisterType<RLMDeviceList>().SingleInstance();

            builder.RegisterType<DigitiserCommunication>().As<IDigitiserCommunication>();
            builder.RegisterType<FileTransferCommunication>().As<IFileTransferCommunication>();
            builder.RegisterType<SessionCommunication>().As<ISessionCommunication>();
            builder.RegisterType<StatusControlCommunication>().As<IStatusControlCommunication>();
            builder.RegisterType<RLMCommunication>().As<IRLMCommunication>();

            builder.RegisterType<KeepAliveManager>().As<IKeepAliveManager>().SingleInstance();
            #endregion

            Container = builder.Build();            
        }     
    }
}
