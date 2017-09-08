using Abiomed.DotNetCore.Business;
using Abiomed.DotNetCore.Models;
using Abiomed.DotNetCore.Storage;
using Abiomed.DotNetCore.RLR.Communications;
using Autofac;

namespace Abiomed.DotNetCore.DependencyInjection
{
    public class AutofacContainer
    {
        public static IContainer Container { get; set; }

        public void Build()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<InsecureTCPServer>();

            #region RLM Communications

            builder.RegisterType<RLMDeviceList>().SingleInstance();

            builder.RegisterType<DigitiserCommunication>().As<IDigitiserCommunication>();
            builder.RegisterType<FileTransferCommunication>().As<IFileTransferCommunication>();
            builder.RegisterType<SessionCommunication>().As<ISessionCommunication>();
            builder.RegisterType<StatusControlCommunication>().As<IStatusControlCommunication>();
            builder.RegisterType<RLMCommunication>().As<IRLMCommunication>();
            builder.RegisterType<KeepAliveManager>().As<IKeepAliveManager>().SingleInstance();

            #endregion

            builder.RegisterType<TableStorage>().As<ITableStorage>().SingleInstance();
            builder.RegisterType<BlobStorage>().As<IBlobStorage>().SingleInstance();
            builder.RegisterType<QueueStorage>().As<IQueueStorage>().SingleInstance();
            builder.RegisterType<ConfigurationManager>().As<IConfigurationManager>().SingleInstance();
            builder.RegisterType<LogManager>().As<ILogManager>().SingleInstance();

            Container = builder.Build();
        }
    }
}