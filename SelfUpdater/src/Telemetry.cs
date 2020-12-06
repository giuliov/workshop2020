using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SelfUpdater
{
    public static class Telemetry
    {
        public static bool Enabled { get; private set; }
        private static TelemetryClient telemetryClient;

        private static TelemetryClient Current
        {
            get
            {
                if (telemetryClient == null)
                {
                    Initialize();
                }
                return telemetryClient;
            }
        }

        public static void Initialize()
        {
            // this variable helps while developing and air-gapped scenarios
            Enabled = Environment.GetEnvironmentVariable("SELFUPDATE_DISABLED") != "1";

            if (Enabled)
            {
                var configuration = new TelemetryConfiguration
                {
                    ConnectionString = BuildConstants.AppInsightsConnectionString
                };
                telemetryClient = new TelemetryClient(configuration);
                // WARNING: this is not recommended as leaks identifiable information
                telemetryClient.Context.User.Id = Environment.UserName;
                telemetryClient.Context.Device.Id = Environment.MachineName;
                telemetryClient.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
                telemetryClient.Context.Component.Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                telemetryClient.Context.Session.Id = Guid.NewGuid().ToString();
                Trace.WriteLine(string.Format("SessionID: {0}", telemetryClient.Context.Session.Id));
            }
        }


        public static void TrackEvent(EventTelemetry ev)
        {
            if (Enabled)
            {
                Current.TrackEvent(ev);
            }
        }

        public static void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            if (Enabled)
            {
                Current.TrackEvent(eventName, properties, metrics);
            }
        }

        public static void TrackException(Exception ex)
        {
            if (Enabled)
            {
                Current.TrackException(ex);
            }
        }

        public static void Shutdown()
        {
            if (Enabled)
            {
                // before exit, flush the remaining data
                telemetryClient.Flush();
                // flush is not blocking when not using InMemoryChannel so wait a bit. There is an active issue regarding the need for `Sleep`/`Delay`
                // which is tracked here: https://github.com/microsoft/ApplicationInsights-dotnet/issues/407
                if (true)
                {
                    System.Threading.Tasks.Task.Delay(5000).Wait();
                }
            }
        }
    }
}
