﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.networking.svrPackets;
using System.Xml;
using System.Xml.Linq;

namespace wServer.realm.entities
{
    public partial class Player
    {
        static int GetExpGoal(int level)
        {
            return 50 + (level - 1) * 100;
        }
        static int GetLevelExp(int level)
        {
            if (level == 1) return 0;
            return 50 * (level - 1) + (level - 2) * (level - 1) * 50;
        }
        static int GetFameGoal(int fame)
        {
            if (fame >= 2000) return 0;
            else if (fame >= 800) return 2000;
            else if (fame >= 400) return 800;
            else if (fame >= 150) return 400;
            else if (fame >= 20) return 150;
            else return 20;
        }

        public int GetStars()
        {
            int ret = 0;
            foreach (var i in client.Account.Stats.ClassStates)
            {
                if (i.BestFame >= 2000) ret += 5;
                else if (i.BestFame >= 800) ret += 4;
                else if (i.BestFame >= 400) ret += 3;
                else if (i.BestFame >= 150) ret += 2;
                else if (i.BestFame >= 20) ret += 1;
            }
            return ret;
        }

        static readonly Dictionary<string, Tuple<int, int, int>> QuestDat =
            new Dictionary<string, Tuple<int, int, int>>()  //Priority, Min, Max
        {
            { "Scorpion Queen",             Tuple.Create(1, 1, 6) },
            { "Bandit Leader",              Tuple.Create(1, 1, 6) },
            { "Hobbit Mage",                Tuple.Create(3, 3, 8) },
            { "Undead Hobbit Mage",         Tuple.Create(3, 3, 8) },
            { "Giant Crab",                 Tuple.Create(3, 3, 8) },
            { "Desert Werewolf",            Tuple.Create(3, 3, 8) },
            { "Sandsman King",              Tuple.Create(4, 4, 9) },
            { "Goblin Mage",                Tuple.Create(4, 4, 9) },
            { "Elf Wizard",                 Tuple.Create(4, 4, 9) },
            { "Dwarf King",                 Tuple.Create(5, 5, 10) },
            { "Swarm",                      Tuple.Create(6, 6, 11) },
            { "Shambling Sludge",           Tuple.Create(6, 6, 11) },
            { "Great Lizard",               Tuple.Create(7, 7, 12) },
            { "Wasp Queen",                 Tuple.Create(8, 7, 20) },
            { "Horned Drake",               Tuple.Create(8, 7, 20) },

            { "Deathmage",                  Tuple.Create(5, 6, 11) },
            { "Great Coil Snake",           Tuple.Create(6, 6, 12) },
            { "Lich",                       Tuple.Create(8, 6, 20) },
            { "Actual Lich",                Tuple.Create(8, 7, 20) },
            { "Ent Ancient",                Tuple.Create(9, 7, 20) },
            { "Actual Ent Ancient",         Tuple.Create(9, 7, 20) },
            { "Oasis Giant",                Tuple.Create(10, 8, 20) },
            { "Phoenix Lord",               Tuple.Create(10, 9, 20) },
            { "Ghost King",                 Tuple.Create(11,10, 20) },
            { "Actual Ghost King",          Tuple.Create(11,10, 20) },
            { "Cyclops God",                Tuple.Create(12,10, 20) },
            { "Red Demon",                  Tuple.Create(14,15, 20) },

            { "Skull Shrine",               Tuple.Create(13,15, 20) },
            { "Pentaract",                  Tuple.Create(13,15, 20) },
            { "Cube God",                   Tuple.Create(13,15, 20) },
            { "Grand Sphinx",               Tuple.Create(13,15, 20) },
            { "Lord of the Lost Lands",     Tuple.Create(13,15, 20) },
            { "Hermit God",                 Tuple.Create(13,15, 20) },
            { "Ghost Ship",                 Tuple.Create(13,15, 20) },

            { "Evil Chicken God",           Tuple.Create(15,1, 20) },
            { "Bonegrind the Butcher",      Tuple.Create(15,1, 20) },
            { "Dreadstump the Pirate King", Tuple.Create(15,1, 20) },
            { "Arachna the Spider Queen",   Tuple.Create(15,1, 20) },
            { "Stheno the Snake Queen",     Tuple.Create(15,1, 20) },
            { "Mixcoatl the Masked God",    Tuple.Create(15,1, 20) },
            { "Limon the Sprite God",       Tuple.Create(15,1, 20) },
            { "Septavius the Ghost God",    Tuple.Create(15,1, 20) },
            { "Davy Jones",                 Tuple.Create(15,1, 20) },
            { "Lord Ruthven",               Tuple.Create(15,1, 20) },
            { "Archdemon Malphas",          Tuple.Create(15,1, 20) },
            { "Thessal the Mermaid Goddess",Tuple.Create(15,1, 20) },
            { "Dr. Terrible",               Tuple.Create(15,1, 20) },
            { "Horrific Creation",          Tuple.Create(15,1, 20) },
            { "Masked Party God",           Tuple.Create(15,1, 20) },
            { "Stone Guardian Left",        Tuple.Create(15,1, 20) },
            { "Stone Guardian Right",       Tuple.Create(15,1, 20) },
            { "Oryx the Mad God 1",         Tuple.Create(15,1, 20) },
            { "Oryx the Mad God 2",         Tuple.Create(15,1, 20) },
        };

        float Dist(Entity a, Entity b)
        {
            var dx = a.X - b.X;
            var dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
        Entity FindQuest()
        {
            Entity ret = null;
            float bestScore = 0;
            foreach (var i in Owner.Quests.Values
                .OrderBy(quest => DistanceSquared(quest.X, quest.Y, X, Y)))
            {
                if (i.ObjectDesc == null || !i.ObjectDesc.Quest) continue;

                Tuple<int, int, int> x;
                if (!QuestDat.TryGetValue(i.ObjectDesc.ObjectId, out x)) continue;

                if ((Level >= x.Item2 && Level <= x.Item3))
                {
                    var score = (20 - Math.Abs((i.ObjectDesc.Level ?? 0) - Level)) * x.Item1 -   //priority * level diff
                            Dist(this, i) / 100;    //minus 1 for every 100 tile distance
                    if (score > bestScore)
                    {
                        bestScore = score;
                        ret = i;
                    }
                }
            }
            return ret;
        }

        Entity questEntity;
        public Entity Quest { get { return questEntity; } }
        void HandleQuest(RealmTime time)
        {
            if (time.tickCount % 500 == 0 || questEntity == null || questEntity.Owner == null)
            {
                var newQuest = FindQuest();
                if (newQuest != null && newQuest != questEntity)
                {
                    Owner.Timers.Add(new WorldTimer(100, (w, t) =>
                    {
                        client.SendPacket(new QuestObjIdPacket()
                        {
                            ObjectID = newQuest.Id
                        });
                    }));
                    questEntity = newQuest;
                }
            }
        }

        void CalculateFame()
        {
            int newFame = 0;
            if (Experience < 200 * 1000) newFame = Experience / 1000;
            else newFame = 200 + (Experience - 200 * 1000) / 1000;
            if (newFame != Fame)
            {
                Fame = newFame;
                int newGoal;
                var state = client.Account.Stats.ClassStates.SingleOrDefault(_ => _.ObjectType == ObjectType);
                if (state != null && state.BestFame > Fame)
                    newGoal = GetFameGoal(state.BestFame);
                else
                    newGoal = GetFameGoal(Fame);
                if (newGoal > FameGoal)
                {
                    Owner.BroadcastPacket(new NotificationPacket()
                    {
                        ObjectId = Id,
                        Color = new ARGB(0xFF00FF00),
                        Text = "Class Quest Complete!"
                    }, null);
                    Stars = GetStars();
                }
                FameGoal = newGoal;
                UpdateCount++;
            }
        }

        bool CheckLevelUp()
        {
            if (Experience - GetLevelExp(Level) >= ExperienceGoal && Level < 20)
            {
                Level++;
                ExperienceGoal = GetExpGoal(Level);
                foreach (XElement i in XmlDatas.TypeToElement[ObjectType].Elements("LevelIncrease"))
                {
                    Random rand = new System.Random();
                    int min = int.Parse(i.Attribute("min").Value);
                    int max = int.Parse(i.Attribute("max").Value) + 1;
                    int limit = int.Parse(XmlDatas.TypeToElement[ObjectType].Element(i.Value).Attribute("max").Value);
                    int idx = StatsManager.StatsNameToIndex(i.Value);
                    Stats[idx] += rand.Next(min, max);
                    if (Stats[idx] > limit) Stats[idx] = limit;
                }
                HP = Stats[0] + Boost[0];
                MP = Stats[1] + Boost[1];

                UpdateCount++;

                if (Level == 20)
                    foreach (var i in Owner.Players.Values)
                        i.SendInfo(Name + " achieved level 20");
                questEntity = null;
                return true;
            }
            CalculateFame();
            return false;
        }

        public bool EnemyKilled(Enemy enemy, int exp, bool killer)
        {
            if (enemy == questEntity)
                Owner.BroadcastPacket(new NotificationPacket()
                {
                    ObjectId = Id,
                    Color = new ARGB(0xFF00FF00),
                    Text = "Quest Complete!"
                }, null);
            if (exp != 0)
            {
                Experience += exp;
                UpdateCount++;
            }
            fames.Killed(enemy, killer);
            return CheckLevelUp();
        }
    }
}
