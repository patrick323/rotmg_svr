using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.cliPackets;
using wServer.svrPackets;
using wServer.realm.setpieces;
using wServer.realm.entities.player.commands;

namespace wServer.realm.entities
{
    partial class Player
    {
        public void PlayerText(RealmTime time, PlayerTextPacket pkt)
        {
            if (pkt.Text[0] == '/')
            {
                string[] x = pkt.Text.Trim().Split(' ');
                ProcessCmd(x[0].Trim('/'), x.Skip(1).ToArray());
            }
            else
                Owner.BroadcastPacket(new TextPacket()
                {
                    Name = (Client.Account.Admin ? "@" : "") + Name,
                    ObjectId = Id,
                    Stars = Stars,
                    BubbleTime = 5,
                    Recipient = "",
                    Text = pkt.Text,
                    CleanText = pkt.Text
                }, null);
        }

        public void SendInfo(string text)
        {
            psr.SendPacket(new TextPacket()
            {
                BubbleTime = 0,
                Stars = -1,
                Name = "",
                Text = text
            });
        }
        public void SendError(string text)
        {
            psr.SendPacket(new TextPacket()
            {
                BubbleTime = 0,
                Stars = -1,
                Name = "*Error*",
                Text = text
            });
        }
        public void SendClientText(string text)
        {
            psr.SendPacket(new TextPacket()
            {
                BubbleTime = 0,
                Stars = -1,
                Name = "*Client*",
                Text = text
            });
        }
        public void SendHelp(string text)
        {
            psr.SendPacket(new TextPacket()
            {
                BubbleTime = 0,
                Stars = -1,
                Name = "*Help*",
                Text = text
            });
        }
        public void SendEnemy(string name, string text)
        {
            psr.SendPacket(new TextPacket()
            {
                BubbleTime = 0,
                Stars = -1,
                Name = "#" + name,
                Text = text
            });
        }
        public void SendText(string sender, string text)
        {
            psr.SendPacket(new TextPacket()
            {
                BubbleTime = 0,
                Stars = -1,
                Name = sender,
                Text = text
            });
        }


        static Dictionary<string, ICommand> cmds;

        bool CmdReqAdmin()
        {
            if (!psr.Account.Admin)
            {
                psr.SendPacket(new TextPacket()
                {
                    BubbleTime = 0,
                    Stars = -1,
                    Name = "*Error*",
                    Text = "No permission!"
                });
                return false;
            }
            else
                return true;
        }
        void ProcessCmd(string cmd, string[] args)
        {
            if (cmds == null)
            {
                cmds = new Dictionary<string, ICommand>();
                var t = typeof(ICommand);
                foreach (var i in t.Assembly.GetTypes())
                    if (t.IsAssignableFrom(i) && i != t)
                    {
                        var instance = (ICommand)Activator.CreateInstance(i);
                        cmds.Add(instance.Command, instance);
                    }
            }

            ICommand command;
            if (!cmds.TryGetValue(cmd, out command))
            {
                psr.SendPacket(new TextPacket()
                    {
                        BubbleTime = 0,
                        Stars = -1,
                        Name = "*Error*",
                        Text = "Unknown Command!"
                    });
                return;
            }
            try
            {
                if (command.RequirePerm)
                {
                    if (this.CmdReqAdmin())
                    {
                        command.Execute(this, args);
                    }
                }
                else
                    command.Execute(this, args);

            }
            catch
            {
                psr.SendPacket(new TextPacket()
                {
                    BubbleTime = 0,
                    Stars = -1,
                    Name = "*Error*",
                    Text = "Error when executing the command!"
                });
            }
        }
    }
}
