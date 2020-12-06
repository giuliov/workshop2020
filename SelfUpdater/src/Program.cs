using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SelfUpdater
{
    class Program
    {
        static int Main(string[] args)
        {
                SelfUpdater.SelfUpdateIfRequired(args);

                Console.WriteLine("Hello World!");

                // every now and then
                SelfUpdater.CheckForNewerVersion();

                return 0;//success
        }
    }
}
