using Abiomed.DotNetCore.Business;
using Abiomed.DotNetCore.Models;
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

            Container = builder.Build();
        }

    }
}
