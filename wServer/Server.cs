using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using wServer.realm;
using wServer.networking;

namespace wServer
{
    class Server
    {
        public Socket Socket { get; private set; }
        public RealmManager Manager { get; private set; }
        public Server(RealmManager manager, int port)
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Manager = manager;
        }

        public void Start()
        {
            Socket.Bind(new IPEndPoint(IPAddress.Any, 2050));
            Socket.Listen(0xff);
            Socket.BeginAccept(Listen, null);
        }

        void Listen(IAsyncResult ar)
        {
            var cliSkt = Socket.EndAccept(ar);
            Socket.BeginAccept(Listen, null);
            if (cliSkt != null)
            {
                var client = new Client(Manager, cliSkt);
                client.BeginProcess();
            }
        }

        public void Stop()
        {
            Console.WriteLine("Terminating...");
            Socket.Shutdown(SocketShutdown.Both);
            foreach (var i in Manager.Clients.Values.ToArray())
                i.Disconnect();
            Socket.Close();
        }
    }
}
