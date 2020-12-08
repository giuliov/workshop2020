using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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

            // hook to display what is happening
            SelfUpdater.Log = (message) =>
            {
                Console.Write("DBG: ");
                Console.WriteLine(message);
            };

            try
            {
                args = SelfUpdater.AutoUpdate(args);

                Console.WriteLine("Hello World!");
                for (int i = 0; i < args.Length; i++)
                {
                    Console.WriteLine($"Arg[{i + 1}] = '{args[i]}'");
                }

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
