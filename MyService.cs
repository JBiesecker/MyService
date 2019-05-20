// https://docs.microsoft.com/en-us/dotnet/framework/windows-services/walkthrough-creating-a-windows-service-application-in-the-component-designer
//using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;
using System.Runtime.InteropServices;
using Newtonsoft.Json;






namespace MyService
{

    
public partial class MyService : ServiceBase
    {
        public enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        static string iotHubUri = "TelemetryManagementIOTHub.azure-devices.net";
        static DeviceClient iotHubDeviceClient;
        static string iotHubDeviceName = "SimulatedDevice_1";
        static string iotHubDeviceKey = "EWGizAIIh6E3wybumuWhPFpbXmDNNw5Mq0ySNWS24vQ=";
        static double baseValue = 20;

        [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus
        {
            public int dwServiceType;
            public ServiceState dwCurrentState;
            public int dwControlsAccepted;
            public int dwWin32ExitCode;
            public int dwServiceSpecificExitCode;
            public int dwCheckPoint;
            public int dwWaitHint;
        };
        private int eventId = 1;

        private Timer timer;

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);

        public MyService()
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

        protected override void OnContinue()
        {
            timer.Start();
            eventLog1.WriteEntry("In OnContinue.", EventLogEntryType.Information, eventId++);
        }
        protected override void OnPause()
        {
            timer.Stop();
            ServiceStatus serviceStatus = new ServiceStatus();
            eventLog1.WriteEntry("In OnPause.", EventLogEntryType.Information, eventId++);
            serviceStatus.dwCurrentState = ServiceState.SERVICE_PAUSED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

        }
        protected override void OnStart(string[] args)
        {
            ServiceStatus serviceStatus = new ServiceStatus();
            eventLog1.WriteEntry("In OnStart.", EventLogEntryType.Information, eventId++);
            timer = new Timer();
            timer.Interval = 150000; // 30 seconds
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Start();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        protected override void OnStop()
        {
            ServiceStatus serviceStatus = new ServiceStatus();
            eventLog1.WriteEntry("In OnStop.", EventLogEntryType.Information, eventId++);
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

        }

        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            // TODO: Insert monitoring activities here.
            eventLog1.WriteEntry("Sending Telemetry", EventLogEntryType.Information, eventId++);
        }

        static void SendTelemetry()
        {
            var random = new Random();
            var history = new List<string>();
            var message = JsonConvert.SerializeObject(new {
                time = DateTime.UtcNow,
                id = "beb02410-78ba-496e-a26a-59cef9b4d609",
                readings = new[] {
                    new { value = Math.Round(baseValue + random.NextDouble() * 10, 3) }
                }
        });         
             //   history.Add(message);
             //   PrintHistory(history);

                iotHubDeviceClient.SendEventAsync(new Message(Encoding.ASCII.GetBytes(message)));

             //   await Task.Delay(5000);
            }
        }

    }
}
