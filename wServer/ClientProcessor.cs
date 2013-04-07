using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using wServer.svrPackets;
using wServer.cliPackets;
using System.Xml;
using db;
using wServer.realm;
using wServer.realm.entities;

namespace wServer
{
    public enum ProtocalStage
    {
        Connected,
        Handshaked,
        Ready,
        Disconnected
    }
    public class ClientProcessor
    {
        Socket skt;
        Thread wkrThread;
        public RC4 ReceiveKey { get; private set; }
        public RC4 SendKey { get; private set; }

        public Socket Socket { get { return skt; } }

        public ClientProcessor(Socket skt)
        {
            this.skt = skt;
            ReceiveKey = new RC4(new byte[] { 0x31, 0x1f, 0x80, 0x69, 0x14, 0x51, 0xc7, 0x1b, 0x09, 0xa1, 0x3a, 0x2a, 0x6e });
            SendKey = new RC4(new byte[] { 0x72, 0xc5, 0x58, 0x3c, 0xaf, 0xb6, 0x81, 0x89, 0x95, 0xcb, 0xd7, 0x4b, 0x80 });
        }

        NetworkHandler handler;
        public void BeginProcess()
        {
            handler = new NetworkHandler(this, skt);
            handler.BeginHandling();
        }

        public void SendPacket(Packet pkt)
        {
            handler.SendPacket(pkt);
        }
        public void SendPackets(IEnumerable<Packet> pkts)
        {
            handler.SendPackets(pkts);
        }

        internal bool ProcessPacket(Packet pkt)
        {
            if (stage == ProtocalStage.Disconnected)
                return false;
            if (stage == ProtocalStage.Ready && (entity == null || entity != null && entity.Owner == null))
                return false;
            try
            {
                if (pkt.ID == PacketID.Hello)
                    ProcessHelloPacket(pkt as HelloPacket);
                else if (pkt.ID == PacketID.Create)
                    ProcessCreatePacket(pkt as CreatePacket);
                else if (pkt.ID == PacketID.Load)
                    ProcessLoadPacket(pkt as LoadPacket);
                else if (pkt.ID == PacketID.Pong)
                    entity.Pong(pkt as PongPacket);
                else if (pkt.ID == PacketID.Move)
                    RealmManager.Logic.AddPendingAction(t => entity.Move(t, pkt as MovePacket));
                else if (pkt.ID == PacketID.PlayerShoot)
                    RealmManager.Logic.AddPendingAction(t => entity.PlayerShoot(t, pkt as PlayerShootPacket));
                else if (pkt.ID == PacketID.EnemyHit)
                    RealmManager.Logic.AddPendingAction(t => entity.EnemyHit(t, pkt as EnemyHitPacket));
                else if (pkt.ID == PacketID.OtherHit)
                    RealmManager.Logic.AddPendingAction(t => entity.OtherHit(t, pkt as OtherHitPacket));
                else if (pkt.ID == PacketID.SquareHit)
                    RealmManager.Logic.AddPendingAction(t => entity.SquareHit(t, pkt as SquareHitPacket));
                else if (pkt.ID == PacketID.PlayerHit)
                    RealmManager.Logic.AddPendingAction(t => entity.PlayerHit(t, pkt as PlayerHitPacket));
                else if (pkt.ID == PacketID.ShootAck)   //TODO: this one is spammed when lots of bullet, special handle?
                    RealmManager.Network.AddPendingAction(this, t => entity.ShootAck(t, pkt as ShootAckPacket));
                else if (pkt.ID == PacketID.InvSwap)
                    RealmManager.Logic.AddPendingAction(t => entity.InventorySwap(t, pkt as InvSwapPacket));
                else if (pkt.ID == PacketID.InvDrop)
                    RealmManager.Logic.AddPendingAction(t => entity.InventoryDrop(t, pkt as InvDropPacket));
                else if (pkt.ID == PacketID.UseItem)
                    RealmManager.Logic.AddPendingAction(t => entity.UseItem(t, pkt as UseItemPacket));
                else if (pkt.ID == PacketID.UsePortal)
                    RealmManager.Network.AddPendingAction(this, t => entity.UsePortal(t, pkt as UsePortalPacket));
                else if (pkt.ID == PacketID.PlayerText)
                    RealmManager.Logic.AddPendingAction(t => entity.PlayerText(t, pkt as PlayerTextPacket));
                else if (pkt.ID == PacketID.ChooseName)
                    ProcessChooseNamePacket(pkt as ChooseNamePacket);
                else if (pkt.ID == PacketID.Escape)
                    ProcessEscapePacket(pkt as EscapePacket);
                else if (pkt.ID == PacketID.Teleport)
                    RealmManager.Logic.AddPendingAction(t => entity.Teleport(t, pkt as TeleportPacket));
                else if (pkt.ID == PacketID.GotoAck)
                    RealmManager.Logic.AddPendingAction(t => entity.GotoAck(t, pkt as GotoAckPacket));
                else if (pkt.ID == PacketID.EditAccountList)
                    entity.EditAccountList(pkt as EditAccountListPacket);
                else if (pkt.ID == PacketID.Buy)
                    entity.Buy(pkt as BuyPacket);
                else if (pkt.ID == PacketID.RequestTrade)
                    RealmManager.Logic.AddPendingAction(t => entity.RequestTrade(t, pkt as RequestTradePacket));
                else if (pkt.ID == PacketID.ChangeTrade)
                    RealmManager.Logic.AddPendingAction(t => entity.ChangeTrade(t, pkt as ChangeTradePacket));
                else if (pkt.ID == PacketID.AcceptTrade)
                    RealmManager.Logic.AddPendingAction(t => entity.AcceptTrade(t, pkt as AcceptTradePacket));
                else if (pkt.ID == PacketID.CancelTrade)
                    RealmManager.Logic.AddPendingAction(t => entity.CancelTrade(t, pkt as CancelTradePacket));
                else if (pkt.ID == PacketID.AOEAck)
                    RealmManager.Logic.AddPendingAction(t => entity.AOEAck(t, pkt as AOEAckPacket));
                else if (pkt.ID == PacketID.GroundDamage)
                    RealmManager.Logic.AddPendingAction(t => entity.GroundDamage(t, pkt as GroundDamagePacket));
                else if (pkt.ID == PacketID.CheckCredits)
                    entity.CheckCredits(pkt as CheckCreditsPacket);
                else if (pkt.ID != PacketID.Packet)
                {
                    Console.WriteLine("Unhandled packet: " + pkt.ToString());
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public void Disconnect()
        {
            if (stage == ProtocalStage.Disconnected) return;
            var original = stage;
            stage = ProtocalStage.Disconnected;
            if (account != null)
                DisconnectFromRealm();
            if (db != null && original != ProtocalStage.Ready)
            {
                db.Dispose();
                db = null;
            }
            skt.Close();
        }
        public void Save()
        {
            if (db != null)
            {
                if (character != null)
                {
                    entity.SaveToCharacter();
                    db.SaveCharacter(account, character);
                }
                db.Dispose();
                db = null;
            }
        }

        Database db;
        Account account;
        Char character;
        Player entity;
        bool isGuest = false;
        ProtocalStage stage;

        public Database Database { get { return db; } }
        public Char Character { get { return character; } }
        public Account Account { get { return account; } }
        public ProtocalStage Stage { get { return stage; } }
        public Player Player { get { return entity; } }
        public wRandom Random { get; private set; }

        int targetWorld = -1;
        void ProcessHelloPacket(HelloPacket pkt)
        {
            if (!skt.RemoteEndPoint.ToString().StartsWith("127.0.0.1"))
            {
                SendPacket(new svrPackets.TextPacket()
                {
                    BubbleTime = 0,
                    Stars = -1,
                    Name = "*Error*",
                    Text = "You should not be here!"
                });
                Disconnect();
                return;
            }

            db = new Database();
            if ((account = db.Verify(pkt.GUID, pkt.Password)) == null)
            {
                account = Database.Register(pkt.GUID, pkt.Password, true);
                if (account == null)
                {
                    SendPacket(new svrPackets.FailurePacket()
                    {
                        Message = "Invalid account."
                    });
                    Disconnect();
                    return;
                }
            }
            if (!RealmManager.TryConnect(this))
            {
                account = null;
                SendPacket(new svrPackets.FailurePacket()
                {
                    Message = "Failed to connect."
                });
                Disconnect();
            }
            else
            {
                World world = RealmManager.GetWorld(pkt.GameId);
                if (world == null)
                {
                    SendPacket(new svrPackets.FailurePacket()
                    {
                        Message = "Invalid world."
                    });
                    Disconnect();
                }
                else
                {
                    if (world.Id == -6) //Test World
                        (world as realm.worlds.Test).LoadJson(pkt.MapInfo);
                    else if (world.IsLimbo)
                        world = world.GetInstance(this);

                    var seed = (uint)((long)Environment.TickCount * pkt.GUID.GetHashCode()) % uint.MaxValue;
                    Random = new wRandom(seed);
                    targetWorld = world.Id;
                    SendPacket(new MapInfoPacket()
                    {
                        Width = world.Map.Width,
                        Height = world.Map.Height,
                        Name = world.Name,
                        Seed = seed,
                        Background = world.Background,
                        AllowTeleport = world.AllowTeleport,
                        ShowDisplays = world.ShowDisplays,
                        ClientXML = world.ClientXML,
                        ExtraXML = world.ExtraXML
                    });
                    stage = ProtocalStage.Handshaked;
                }
            }
        }

        void ProcessCreatePacket(CreatePacket pkt)
        {
            int nextCharId = 1;
            nextCharId = db.GetNextCharID(account);
            var cmd = db.CreateQuery();
            cmd.CommandText = "SELECT maxCharSlot FROM accounts WHERE id=@accId;";
            cmd.Parameters.AddWithValue("@accId", account.AccountId);
            int maxChar = (int)cmd.ExecuteScalar();

            cmd = db.CreateQuery();
            cmd.CommandText = "SELECT COUNT(id) FROM characters WHERE accId=@accId AND dead = FALSE;";
            cmd.Parameters.AddWithValue("@accId", account.AccountId);
            int currChar = (int)(long)cmd.ExecuteScalar();

            if (currChar >= maxChar)
            {
                Disconnect();
                return;
            }

            character = Database.CreateCharacter(pkt.ObjectType, nextCharId);

            int[] stats = new int[]
            {
                character.MaxHitPoints,
                character.MaxMagicPoints,
                character.Attack,
                character.Defense,
                character.Speed,
                character.Dexterity,
                character.HpRegen,
                character.MpRegen,
            };

            bool ok = true;
            cmd = db.CreateQuery();
            cmd.CommandText = @"INSERT INTO characters(accId, charId, charType, level, exp, fame, items, hp, mp, stats, dead, pet)
 VALUES(@accId, @charId, @charType, 1, 0, 0, @items, 100, 100, @stats, FALSE, -1);";
            cmd.Parameters.AddWithValue("@accId", account.AccountId);
            cmd.Parameters.AddWithValue("@charId", nextCharId);
            cmd.Parameters.AddWithValue("@charType", pkt.ObjectType);
            cmd.Parameters.AddWithValue("@items", character._Equipment);
            cmd.Parameters.AddWithValue("@stats", Utils.GetCommaSepString(stats));
            int v = cmd.ExecuteNonQuery();
            ok = v > 0;

            if (ok)
            {
                SendPacket(new CreateSuccessPacket()
                {
                    CharacterID = character.CharacterId,
                    ObjectID = RealmManager.Worlds[targetWorld].EnterWorld(entity = new Player(this))
                });
                stage = ProtocalStage.Ready;
            }
            else
                SendPacket(new svrPackets.FailurePacket()
                {
                    Message = "Failed to Load character."
                });
        }

        void ProcessLoadPacket(LoadPacket pkt)
        {
            character = db.LoadCharacter(account, pkt.CharacterId);
            if (character != null)
            {
                if (character.Dead)
                    SendPacket(new svrPackets.FailurePacket()
                    {
                        Message = "Character is dead."
                    });
                else
                {
                    SendPacket(new CreateSuccessPacket()
                    {
                        CharacterID = character.CharacterId,
                        ObjectID = RealmManager.Worlds[targetWorld].EnterWorld(entity = new Player(this))
                    });
                    stage = ProtocalStage.Ready;
                }
            }
            else
                SendPacket(new svrPackets.FailurePacket()
                {
                    Message = "Failed to Load character."
                });
        }

        void ProcessChooseNamePacket(ChooseNamePacket pkt)
        {
            if (string.IsNullOrEmpty(pkt.Name) ||
                pkt.Name.Length > 10)
            {
                SendPacket(new NameResultPacket()
                {
                    Success = false,
                    Message = "Invalid name"
                });
            }

            var cmd = db.CreateQuery();
            cmd.CommandText = "SELECT COUNT(name) FROM accounts WHERE name=@name;";
            cmd.Parameters.AddWithValue("@name", pkt.Name);
            if ((int)(long)cmd.ExecuteScalar() > 0)
                SendPacket(new NameResultPacket()
                {
                    Success = false,
                    Message = "Duplicated name"
                });
            else
            {
                db.ReadStats(account);
                if (account.NameChosen && account.Credits < 1000)
                    SendPacket(new NameResultPacket()
                    {
                        Success = false,
                        Message = "Not enough credits"
                    });
                else
                {
                    cmd = db.CreateQuery();
                    cmd.CommandText = "UPDATE accounts SET name=@name, namechosen=TRUE WHERE id=@accId;";
                    cmd.Parameters.AddWithValue("@accId", account.AccountId);
                    cmd.Parameters.AddWithValue("@name", pkt.Name);
                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        entity.Credits = db.UpdateCredit(account, -1000);
                        entity.Name = pkt.Name;
                        entity.NameChosen = true;
                        entity.UpdateCount++;
                        SendPacket(new NameResultPacket()
                        {
                            Success = true,
                            Message = ""
                        });
                    }
                    else
                        SendPacket(new NameResultPacket()
                        {
                            Success = false,
                            Message = "Internal Error"
                        });
                }
            }
        }

        void ProcessEscapePacket(EscapePacket pkt)
        {
            Reconnect(new ReconnectPacket()
            {
                Host = "",
                Port = 2050,
                GameId = World.NEXUS_ID,
                Name = "Nexus",
                Key = Empty<byte>.Array,
            });
        }

        //Following must execute, network loop will discard disconnected client, so logic loop
        void DisconnectFromRealm()
        {
            RealmManager.Logic.AddPendingAction(t =>
            {
                if (Player != null)
                    Player.SaveToCharacter();
                Save();
                RealmManager.Disconnect(this);
            }, PendingPriority.Destruction);
        }
        public void Reconnect(ReconnectPacket pkt)
        {
            RealmManager.Logic.AddPendingAction(t =>
            {
                if (Player != null)
                    Player.SaveToCharacter();
                Save();
                RealmManager.Disconnect(this);
                SendPacket(pkt);
            }, PendingPriority.Destruction);
        }
    }
}
