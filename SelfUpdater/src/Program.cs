using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SelfUpdater
{
    class Program
    {
        static int Main(string[] args)
        {
            var mainTimer = new Stopwatch();
            mainTimer.Start();

            Telemetry.Initialize();

            Telemetry.TrackEvent("program-start",
                new Dictionary<string, string> {
                    { "arg", string.Join(' ', args) }
                });

            try
            {
                SelfUpdater.SelfUpdateIfRequired(args);

                Console.WriteLine("Hello World!");

                // every now and then
                SelfUpdater.CheckForNewerVersion();

                return 0;//success
            }
            finally
            {
                mainTimer.Stop();
                Telemetry.TrackEvent("program-end", null,
                    new Dictionary<string, double> {
                        { "RunDuration", mainTimer.ElapsedMilliseconds }
                    });

                Telemetry.Shutdown();
            }
        }
    }
}
