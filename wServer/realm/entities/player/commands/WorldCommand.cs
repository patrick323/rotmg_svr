using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.svrPackets;

namespace wServer.realm.entities.player.commands
{
    class TutorialCommand : ICommand
    {
        public string Command { get { return "tutorial"; } }
        public bool RequirePerm { get { return false; } }

        public void Execute(Player player, string[] args)
        {
            player.Client.Reconnect(new ReconnectPacket()
            {
                Host = "",
                Port = 2050,
                GameId = World.TUT_ID,
                Name = "Tutorial",
                Key = Empty<byte>.Array,
            });
        }
    }

    class WhoCommand : ICommand
    {
        public string Command { get { return "who"; } }
        public bool RequirePerm { get { return false; } }

        public void Execute(Player player, string[] args)
        {
            StringBuilder sb = new StringBuilder("Players online: ");
            var copy = player.Owner.Players.Values.ToArray();
            for (int i = 0; i < copy.Length; i++)
            {
                if (i != 0) sb.Append(", ");
                sb.Append(copy[i].Name);
            }

            player.SendInfo(sb.ToString());
        }
    }

    class ServerCommand : ICommand
    {
        public string Command { get { return "server"; } }
        public bool RequirePerm { get { return false; } }

        public void Execute(Player player, string[] args)
        {
            player.SendInfo(player.Owner.Name);
        }
    }

    class PauseCommand : ICommand
    {
        public string Command { get { return "pause"; } }
        public bool RequirePerm { get { return false; } }

        public void Execute(Player player, string[] args)
        {
            if (player.HasConditionEffect(ConditionEffects.Paused))
            {
                player.ApplyConditionEffect(new ConditionEffect()
                {
                    Effect = ConditionEffectIndex.Paused,
                    DurationMS = 0
                });
                player.SendInfo("Game resumed.");
            }
            else
            {
                if (player.Owner.EnemiesCollision.HitTest(player.X, player.Y, 8).OfType<Enemy>().Any())
                    player.SendError("Not safe to pause.");
                else
                {
                    player.ApplyConditionEffect(new ConditionEffect()
                    {
                        Effect = ConditionEffectIndex.Paused,
                        DurationMS = -1
                    });
                    player.SendInfo("Game paused.");
                }
            }
        }
    }

    /// <summary>
    /// This introduces a subtle bug, since the client UI is not notified when a /teleport is typed, it's cooldown does not reset.
    /// This leads to the unfortunate situation where the cooldown has been not been reached, but the UI doesn't know. The graphical TP will fail
    /// and cause it's timer to reset. NB: typing /teleport will workaround this timeout issue.
    /// </summary>
    class TeleportCommand : ICommand
    {
        public string Command { get { return "teleport"; } }
        public bool RequirePerm { get { return false; } }

        public void Execute(Player player, string[] args)
        {
                if (player.Name.ToLower() == args[0].ToLower())
                {
                    player.SendInfo("You are already at yourself, and always will be!");
                    return;
                }

                foreach (var i in player.Owner.Players)
                {
                    if (i.Value.Name.ToLower() == args[0].ToLower().Trim())
                    {
                        player.Teleport(new RealmTime(), new cliPackets.TeleportPacket()
                        {
                            ObjectId = i.Value.Id
                        });
                        return;
                    }
                }
                player.SendInfo(string.Format("Cannot teleport, {0} not found!", args[0].Trim()));
        }
    }

    class TellCommand : ICommand
    {
        public string Command { get { return "tell"; } }
        public bool RequirePerm { get { return false; } }

        public void Execute(Player player, string[] args)
        {
            if (!(player.NameChosen))
            {
                player.SendError("Choose a name!");
                return;
            }

            string playername = args[0].Trim();

            if (player.Name.ToLower() == playername.ToLower())
            {
                player.SendError("Quit telling yourself!");
                return;
            }

            string saytext = string.Join(" ", args, 1, args.Length - 1);

            foreach (var w in RealmManager.Worlds)
            {
                World world = w.Value;
                if (w.Key != 0) // 0 is limbo??
                {
                    foreach (var i in world.Players)
                    {
                        if (i.Value.Name.ToLower() == args[0].ToLower().Trim() && i.Value.NameChosen)
                        {
                            player.Client.SendPacket(new TextPacket() //echo to self
                            {
                                BubbleTime = 10,
                                Stars = player.Stars,
                                Name = player.Name,
                                Recipient = i.Value.Name,
                                Text = saytext
                            });

                            i.Value.Client.SendPacket(new TextPacket() //echo to /tell player
                            {
                                BubbleTime = 10,
                                Stars = player.Stars,
                                Recipient = i.Value.Name,
                                Name = player.Name,
                                Text = saytext
                            });
                            return;
                        }
                    }
                }
            }
            player.SendError("Cannot /tell, {0} not found!");
        }
    }
}
