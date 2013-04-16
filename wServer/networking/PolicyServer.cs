using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace wServer.networking
{
    class PolicyServer
    {
        TcpListener listener;
        public PolicyServer()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, 843);
            }
            catch { }
        }

        static void ServePolicyFile(IAsyncResult ar)
        {
            TcpClient cli = (ar.AsyncState as TcpListener).EndAcceptTcpClient(ar);
            (ar.AsyncState as TcpListener).BeginAcceptTcpClient(ServePolicyFile, ar.AsyncState);
            try
            {
                var s = cli.GetStream();
                NReader rdr = new NReader(s);
                NWriter wtr = new NWriter(s);
                if (rdr.ReadNullTerminatedString() == "<policy-file-request/>")
                {
                    wtr.WriteNullTerminatedString(@"<cross-domain-policy>
     <allow-access-from domain=""*"" to-ports=""*"" />
</cross-domain-policy>");
                    wtr.Write((byte)'\r');
                    wtr.Write((byte)'\n');
                }
                cli.Close();
            }
            catch { }
        }

        bool started = false;
        public void Start()
        {
            try
            {
                listener.Start();
                listener.BeginAcceptTcpClient(ServePolicyFile, listener);
                started = true;
            }
            catch
            {
                started = false;
            }
        }

        public void Stop()
        {
            if (started)
                listener.Stop();
        }
    }
}
