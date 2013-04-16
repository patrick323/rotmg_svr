using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using wServer.realm;
using System.Net.NetworkInformation;
using wServer.networking;
using System.Globalization;

namespace wServer
{
    static class Program
    {
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.Name = "Server Entry Point";

            RealmManager manager = new RealmManager();
            manager.Initialize();

            Server server = new Server(manager, 2050);
            PolicyServer policy = new PolicyServer();

            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine("Terminating...");
                server.Stop();
                policy.Stop();
                Environment.Exit(0);
            };

            policy.Start();
            server.Start();
            Console.WriteLine("Listening at port 2050...");
            Thread.CurrentThread.Join();
        }
    }
}
