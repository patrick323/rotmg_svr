using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using wServer.networking;

namespace wServer.realm
{
    using Work = Tuple<Client, Packet>;
    public class NetworkTicker //Sync network processing
    {
        public RealmManager Manager { get; private set; }
        public NetworkTicker(RealmManager manager)
        {
            this.Manager = manager;
        }

        public void AddPendingPacket(Client client, Packet packet)
        {
            pendings.Add(new Tuple<Client, Packet>(client, packet));
        }
        static BlockingCollection<Work> pendings =
           new BlockingCollection<Work>(new ConcurrentQueue<Work>());


        public void TickLoop()
        {
            Tuple<Client, Packet> work;
            while (true)
            {
                work = pendings.Take();
                if (pendings.Count > 0)
                    Console.WriteLine(pendings.Count);
                if (work.Item1.Stage == ProtocalStage.Disconnected)
                {
                    Client client;
                    Manager.Clients.TryRemove(work.Item1.Account.AccountId, out client);
                    continue;
                }
                try
                {
                    work.Item1.ProcessPacket(work.Item2);
                }
                catch { }
            }
        }
    }
}
