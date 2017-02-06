using Abiomed.Business;
using Abiomed.RLR.Communications;
using Abiomed.Models;
using Abiomed.Repository;
using Autofac;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Abiomed.Communications.Service
{
    public partial class CommunicationsService : ServiceBase
    {
       // private System.ComponentModel.IContainer components;
        private System.Diagnostics.EventLog eventLog1;
        private int eventId = 0;

        public static Autofac.IContainer AutoFacContainer { get; set; }

        public CommunicationsService()
        {
            InitializeComponent();
            eventLog1 = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("MySource"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "MySource", "MyNewLog");
            }
            eventLog1.Source = "MySource";
            eventLog1.Log = "MyNewLog";
        }

        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("In OnStart");
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 600000; // 600 seconds
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            timer.Start();

            Start();
        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("In onStop.");            
        }

        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            // TODO: Insert monitoring activities here.
            eventLog1.WriteEntry("Monitoring the System", EventLogEntryType.Information, eventId++);
        }

        private void Start()
        {
            log4net.Config.XmlConfigurator.Configure();

            var builder = new ContainerBuilder();

            builder.RegisterInstance(LogManager.GetLogger(@"Logger")).As<ILog>();

            builder.RegisterType<RLMCommunication>().As<IRLMCommunication>();
            builder.RegisterType<TCPServer>().As<ITCPServer>();
            builder.RegisterType<RLMDeviceList>();

            // Set the dependency resolver to be Autofac.
            AutoFacContainer = builder.Build();

            ITCPServer _tcpServer = AutoFacContainer.Resolve<ITCPServer>();
            _tcpServer.Run();
        }
    }
}
