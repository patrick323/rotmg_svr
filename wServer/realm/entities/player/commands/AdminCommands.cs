using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.cliPackets;
using wServer.svrPackets;
using wServer.realm.setpieces;
using wServer.realm.worlds;

namespace wServer.realm.entities.player.commands
{
    class SpawnCommand : ICommand
    {
        public string Command { get { return "spawn"; } }
        public bool RequirePerm { get { return true; } }

        public void Execute(Player player, string[] args)
        {
            int num;
            if (args.Length > 0 && int.TryParse(args[0], out num)) //multi
            {
                string name = string.Join(" ", args.Skip(1).ToArray());
                short objType;
                if (!XmlDatas.IdToType.TryGetValue(name, out objType) ||
                    !XmlDatas.ObjectDescs.ContainsKey(objType))
                {
                    player.SendError("Unknown entity!");
                }
                else
                {
                    for (int i = 0; i < num; i++)
                    {
                        var entity = Entity.Resolve(objType);
                        entity.Move(player.X, player.Y);
                        player.Owner.EnterWorld(entity);
                    }
                }
            }
            else
            {
                string name = string.Join(" ", args);
                short objType;
                if (!XmlDatas.IdToType.TryGetValue(name, out objType) ||
                    !XmlDatas.ObjectDescs.ContainsKey(objType))
                {
                    player.SendError("Unknown entity!");
                }
                else
                {
                    var entity = Entity.Resolve(objType);
                    entity.Move(player.X, player.Y);
                    player.Owner.EnterWorld(entity);
                }
            }
        }
    }

    class AddEffCommand : ICommand
    {
        public string Command { get { return "addeff"; } }
        public bool RequirePerm { get { return true; } }

        public void Execute(Player player, string[] args)
        {
            try
            {
                player.ApplyConditionEffect(new ConditionEffect()
                {
                    Effect = (ConditionEffectIndex)Enum.Parse(typeof(ConditionEffectIndex), args[0].Trim()),
                    DurationMS = -1
                });
            }
            catch
            {
                player.SendError("Invalid effect!");
            }
        }
    }

    class RemoveEffCommand : ICommand
    {
        public string Command { get { return "removeeff"; } }
        public bool RequirePerm { get { return true; } }

        public void Execute(Player player, string[] args)
        {
            try
            {
                player.ApplyConditionEffect(new ConditionEffect()
                {
                    Effect = (ConditionEffectIndex)Enum.Parse(typeof(ConditionEffectIndex), args[0].Trim()),
                    DurationMS = 0
                });
            }
            catch
            {
                player.SendError("Invalid effect!");
            }
        }
    }

    class GimmeCommand : ICommand
    {
        public string Command { get { return "gimme"; } }
        public bool RequirePerm { get { return true; } }

        public void Execute(Player player, string[] args)
        {
            string name = string.Join(" ", args.ToArray()).Trim();
            short objType;
            if (!XmlDatas.IdToType.TryGetValue(name, out objType))
            {
                player.SendError("Unknown item type!");
                return;
            }
            for (int i = 0; i < player.Inventory.Length; i++)
                if (player.Inventory[i] == null)
                {
                    player.Inventory[i] = XmlDatas.ItemDescs[objType];
                    player.UpdateCount++;
                    return;
                }
        }
    }

    class TpCommand : ICommand
    {
        public string Command { get { return "tp"; } }
        public bool RequirePerm { get { return true; } }

        public void Execute(Player player, string[] args)
        {
            int x, y;
            try
            {
                x = int.Parse(args[0]);
                y = int.Parse(args[1]);
            }
            catch
            {
                player.SendError("Invalid coordinates!");
                return;
            }
            player.Move(x + 0.5f, y + 0.5f);
            player.SetNewbiePeriod();
            player.UpdateCount++;
            player.Owner.BroadcastPacket(new GotoPacket()
            {
                ObjectId = player.Id,
                Position = new Position()
                {
                    X = player.X,
                    Y = player.Y
                }
            }, null);
        }
    }

    class SetpieceCommand : ICommand
    {
        public string Command { get { return "setpiece"; } }
        public bool RequirePerm { get { return true; } }

        public void Execute(Player player, string[] args)
        {
            try
            {
                ISetPiece piece = (ISetPiece)Activator.CreateInstance(Type.GetType(
                    "wServer.realm.setpieces." + args[0]));
                piece.RenderSetPiece(player.Owner, new IntPoint((int)player.X + 1, (int)player.Y + 1));
            }
            catch
            {
                player.SendError("Cannot apply setpiece!");
            }
        }
    }

    class DebugCommand : ICommand
    {
        public string Command { get { return "debug"; } }
        public bool RequirePerm { get { return true; } }

        public void Execute(Player player, string[] args)
        {
        }
    }

    class AllOnlineCommand : ICommand
    {
        public string Command { get { return "online"; } }
        public bool RequirePerm { get { return true; } }

        public void Execute(Player player, string[] args)
        {
            StringBuilder sb = new StringBuilder("Users online: \r\n");
            var copy = RealmManager.Clients.Values.ToArray();
            for (int i = 0; i < copy.Length; i++)
            {
                if (copy[i].Stage == ProtocalStage.Disconnected) continue;
                sb.AppendFormat("{0}@{1}\r\n", copy[i].Account.Name, copy[i].Socket.RemoteEndPoint.ToString());
            }

            player.SendInfo(sb.ToString());
        }
    }

    class KillAll : ICommand
    {
        public string Command { get { return "killall"; } }
        public bool RequirePerm { get { return true; } }

        public void Execute(Player player, string[] args)
        {
            foreach (var i in player.Owner.Enemies)
            {
                if ((i.Value.ObjectDesc != null) &&
                    (i.Value.ObjectDesc.ObjectId != null) &&
                    (i.Value.ObjectDesc.ObjectId.Contains(args[0])))
                {
                    // i.Value.Damage(player, new RealmTime(), 100 * 1000, true); //may not work for ents/liches
                    i.Value.Owner.LeaveWorld(i.Value);
                }
            }
        }
    }

    class KillAllX : ICommand //this version gives XP points, but does not work for enemies with evaluation/inv periods
    {
        public string Command { get { return "killallx"; } }
        public bool RequirePerm { get { return true; } }

        public void Execute(Player player, string[] args)
        {
            foreach (var i in player.Owner.Enemies)
            {
                if ((i.Value.ObjectDesc != null) &&
                    (i.Value.ObjectDesc.ObjectId != null) &&
                    (i.Value.ObjectDesc.ObjectId.Contains(args[0])))
                {
                    i.Value.Damage(player, new RealmTime(), 100 * 1000, true); //may not work for ents/liches, 
                    //i.Value.Owner.LeaveWorld(i.Value);
                }
            }
        }
    }

    class Kick : ICommand
    {
        public string Command { get { return "kick"; } }
        public bool RequirePerm { get { return true; } }

        public void Execute(Player player, string[] args)
        {
            World destworld = RealmManager.GetWorld(World.VAULT_ID);
            foreach (var w in RealmManager.Worlds)
            {
                World world = w.Value;
                if (w.Key != 0)
                {
                    foreach (var i in world.Players)
                    {
                        if (i.Value.Name.ToLower() == args[0].ToLower().Trim())
                        {
                            i.Value.Client.SendPacket(new ReconnectPacket()
                                  {
                                      Host = "",
                                      Port = 2050,
                                      GameId = destworld.Id,
                                      Name = destworld.Name,
                                      Key = Empty<byte>.Array,
                                  });
                        }
                    }
                }
            }
        }
    }

    class GetQuest : ICommand
    {
        public string Command { get { return "getquest"; } }
        public bool RequirePerm { get { return true; } }

        public void Execute(Player player, string[] args)
        {
            player.SendInfo("Loc: " + player.Quest.X + " " + player.Quest.Y);
        }
    }

    class OryxSay : ICommand
    {
        public string Command { get { return "oryxsay"; } }
        public bool RequirePerm { get { return true; } }

        public void Execute(Player player, string[] args)
        {
            string saytext = string.Join(" ", args);
            player.SendEnemy("Oryx the Mad God", saytext);
        }
    }

    class ListCommands : ICommand
    {
        public string Command { get { return "commands"; } }
        public bool RequirePerm { get { return true; } }

        public void Execute(Player player, string[] args)
        {
            Dictionary<string, ICommand> cmds = new Dictionary<string, ICommand>();
            var t = typeof(ICommand);
            foreach (var i in t.Assembly.GetTypes())
                if (t.IsAssignableFrom(i) && i != t)
                {
                    var instance = (ICommand)Activator.CreateInstance(i);
                    cmds.Add(instance.Command, instance);
                }

            StringBuilder sb = new StringBuilder("Commands: ");
            var copy = cmds.Values.ToArray();
            for (int i = 0; i < copy.Length; i++)
            {
                if (i != 0) sb.Append(", ");
                sb.Append(copy[i].Command);
            }

            player.SendInfo(sb.ToString());
        }
    }

    class AltitudeCommands : ICommand
    {
        public string Command { get { return "alt"; } }
        public bool RequirePerm { get { return true; } }

        public void Execute(Player player, string[] args)
        {
            var tile = player.Owner.Map[(int)player.X, (int)player.Y];
            player.SendInfo(tile.Elevation.ToString());
        }
    }

    class Announcement : ICommand
    {
        public string Command { get { return "announcement"; } } //msg all players in all worlds
        public bool RequirePerm { get { return true; } }

        public void Execute(Player player, string[] args)
        {
            string saytext = string.Join(" ", args);

            foreach (var w in RealmManager.Worlds)
            {
                World world = w.Value;
                if (w.Key != 0)
                {
                    player.SendText("@Announcement", saytext);
                }
            }
        }
    }

    class RTeleport : ICommand
    {
        public string Command { get { return "rteleport"; } }
        public bool RequirePerm { get { return true; } }

        public void Execute(Player player, string[] args)
        {
            foreach (var i in player.Owner.Players)
            {
                if (i.Value.Name.ToLower() == args[0].ToLower().Trim())
                {
                    i.Value.Teleport(new RealmTime(), new cliPackets.TeleportPacket()
                    {
                        ObjectId = player.Id
                    });
                    return;
                }
            }
            player.SendError(string.Format("Cannot rteleport, {0} not found!", args[0].Trim()));
        }
    }

    class KillCommand : ICommand
    {
        public string Command { get { return "kill"; } }
        public bool RequirePerm { get { return true; } }

        public void Execute(Player player, string[] args)
        {
            foreach (var w in RealmManager.Worlds)
            {
                World world = w.Value;
                if (w.Key != 0) // 0 is limbo??
                {
                    foreach (var i in world.Players)
                    {
                        //Unnamed becomes a problem: skip them
                        if (i.Value.Name.ToLower() == args[0].ToLower().Trim() && i.Value.NameChosen)
                        {
                            i.Value.Death("Moderator");
                            return;
                        }
                    }
                }
            }
            player.SendError(string.Format("Cannot /kill, {0} not found!", args[0].Trim()));
        }
    }

}
