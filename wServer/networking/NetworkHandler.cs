using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Collections.Concurrent;
using System.Threading;
using System.Net;
using wServer.realm;

namespace wServer.networking
{
    //hackish code
    class NetworkHandler
    {
        enum ReceiveState
        {
            Awaiting,
            ReceivingHdr,
            ReceivingBody,
            Processing
        }
        class ReceiveToken
        {
            public int Length;
            public Packet Packet;
        }
        enum SendState
        {
            Awaiting,
            Ready,
            Sending
        }
        class SendToken
        {
            public Packet Packet;
        }

        public const int BUFFER_SIZE = 0x10000;

        SocketAsyncEventArgs receive;
        ReceiveState receiveState = ReceiveState.Awaiting;
        byte[] receiveBuff;

        SocketAsyncEventArgs send;
        SendState sendState = SendState.Awaiting;
        byte[] sendBuff;

        Socket skt;
        Client parent;
        public NetworkHandler(Client parent, Socket skt)
        {
            this.parent = parent;
            this.skt = skt;
        }

        public void BeginHandling()
        {
            Console.WriteLine("{0} connected.", skt.RemoteEndPoint);

            skt.NoDelay = true;
            skt.UseOnlyOverlappedIO = true;

            send = new SocketAsyncEventArgs();
            send.Completed += SendCompleted;
            send.UserToken = new SendToken();
            send.SetBuffer(sendBuff = new byte[BUFFER_SIZE], 0, BUFFER_SIZE);

            var receive = new SocketAsyncEventArgs();
            receive.Completed += ReceiveCompleted;
            receive.UserToken = new ReceiveToken();
            receive.SetBuffer(receiveBuff = new byte[BUFFER_SIZE], 0, BUFFER_SIZE);

            receiveState = ReceiveState.ReceivingHdr;
            receive.SetBuffer(0, 5);
            if (!skt.ReceiveAsync(receive))
                ReceiveCompleted(this, receive);
        }

        void ProcessPolicyFile()    //WUT.
        {
            var s = new NetworkStream(skt);
            NWriter wtr = new NWriter(s);
            wtr.WriteNullTerminatedString(@"<cross-domain-policy>
     <allow-access-from domain=""*"" to-ports=""*"" />
</cross-domain-policy>");
            wtr.Write((byte)'\r');
            wtr.Write((byte)'\n');
            parent.Disconnect();
        }

        void ReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                bool repeat;
                do
                {
                    repeat = false;

                    if (e.SocketError != SocketError.Success)
                        throw new SocketException((int)e.SocketError);

                    switch (receiveState)
                    {
                        case ReceiveState.ReceivingHdr:
                            if (e.BytesTransferred < 5)
                            {
                                parent.Disconnect();
                                return;
                            }

                            if (e.Buffer[0] == 0x3c && e.Buffer[1] == 0x70 &&
                                e.Buffer[2] == 0x6f && e.Buffer[3] == 0x6c && e.Buffer[4] == 0x69)
                            {
                                ProcessPolicyFile();
                                return;
                            }

                            var len = (e.UserToken as ReceiveToken).Length =
                                IPAddress.NetworkToHostOrder(BitConverter.ToInt32(e.Buffer, 0)) - 5;
                            if (len < 0 || len > BUFFER_SIZE)
                                throw new InternalBufferOverflowException();
                            (e.UserToken as ReceiveToken).Packet = Packet.Packets[(PacketID)e.Buffer[4]].CreateInstance();

                            receiveState = ReceiveState.ReceivingBody;
                            e.SetBuffer(0, len);
                            if (!skt.ReceiveAsync(e))
                            {
                                repeat = true;
                                continue;
                            }
                            break;
                        case ReceiveState.ReceivingBody:
                            if (e.BytesTransferred < (e.UserToken as ReceiveToken).Length)
                            {
                                parent.Disconnect();
                                return;
                            }

                            var pkt = (e.UserToken as ReceiveToken).Packet;
                            pkt.Read(parent, e.Buffer, 0, (e.UserToken as ReceiveToken).Length);

                            receiveState = ReceiveState.Processing;
                            bool cont = OnPacketReceived(pkt);

                            if (cont && skt.Connected)
                            {
                                receiveState = ReceiveState.ReceivingHdr;
                                e.SetBuffer(0, 5);
                                if (!skt.ReceiveAsync(e))
                                {
                                    repeat = true;
                                    continue;
                                }
                            }
                            break;
                        default:
                            throw new InvalidOperationException(e.LastOperation.ToString());
                    }
                } while (repeat);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        void SendCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                bool repeat;
                do
                {
                    repeat = false;

                    if (e.SocketError != SocketError.Success)
                        throw new SocketException((int)e.SocketError);

                    switch (sendState)
                    {
                        case SendState.Ready:
                            var len = (e.UserToken as SendToken).Packet.Write(parent, sendBuff, 0);

                            sendState = SendState.Sending;
                            e.SetBuffer(sendBuff, 0, len);
                            if (!skt.SendAsync(e))
                            {
                                repeat = true;
                                continue;
                            }
                            break;
                        case SendState.Sending:
                            (e.UserToken as SendToken).Packet = null;

                            if (CanSendPacket(e, true))
                            {
                                repeat = true;
                                continue;
                            }
                            break;
                        default:
                            throw new InvalidOperationException(e.LastOperation.ToString());
                    }
                } while (repeat);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }


        void OnError(Exception ex)
        {
            parent.Disconnect();
        }
        bool OnPacketReceived(Packet pkt)
        {
            if (parent.IsReady())
            {
                parent.Manager.Network.AddPendingPacket(parent, pkt);
                return true;
            }
            else
                return false;
        }
        ConcurrentQueue<Packet> pendingPackets = new ConcurrentQueue<Packet>();
        bool CanSendPacket(SocketAsyncEventArgs e, bool ignoreSending)
        {
            lock (sendLock)
            {
                if (sendState == SendState.Ready ||
                    (!ignoreSending && sendState == SendState.Sending))
                    return false;
                Packet packet;
                if (pendingPackets.TryDequeue(out packet))
                {
                    (e.UserToken as SendToken).Packet = packet;
                    sendState = SendState.Ready;
                    return true;
                }
                else
                {
                    sendState = SendState.Awaiting;
                    return false;
                }
            }
        }

        object sendLock = new object();
        public void SendPacket(Packet pkt)
        {
            pendingPackets.Enqueue(pkt);
            if (CanSendPacket(send, false))
            {
                var len = (send.UserToken as SendToken).Packet.Write(parent, sendBuff, 0);

                sendState = SendState.Sending;
                send.SetBuffer(sendBuff, 0, len);
                if (!skt.SendAsync(send))
                    SendCompleted(this, send);
            }
        }
        public void SendPackets(IEnumerable<Packet> pkts)
        {
            foreach (var i in pkts)
                pendingPackets.Enqueue(i);
            if (CanSendPacket(send, false))
            {
                var len = (send.UserToken as SendToken).Packet.Write(parent, sendBuff, 0);

                sendState = SendState.Sending;
                send.SetBuffer(sendBuff, 0, len);
                if (!skt.SendAsync(send))
                    SendCompleted(this, send);
            }
        }
    }
}
