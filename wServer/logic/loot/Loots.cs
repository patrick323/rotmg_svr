using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.realm.entities;
using wServer.realm;

namespace wServer.logic.loot
{
    struct LootDef
    {
        public LootDef(Item item, double probabilty)
        {
            this.Item = item;
            this.Probabilty = probabilty;
        }
        public readonly Item Item;
        public readonly double Probabilty;
    }
    class ConsiderEventArgs : EventArgs
    {
        public ConsiderEventArgs(Enemy enemy, Tuple<Player, int> dat,
            Random rand, IList<LootDef> lootDefs)
        {
            this.Enemy = enemy;
            this.PlayerData = dat;
            this.Random = rand;
            this.LootDefs = lootDefs;
        }
        public Enemy Enemy { get; private set; }
        public Tuple<Player, int> PlayerData { get; private set; }
        public Random Random { get; private set; }
        public IList<LootDef> LootDefs { get; private set; }
    }
    class Loot : List<LootDef>
    {
        public Loot() { }
        public Loot(params ILootDef[] lootDefs)   //For independent loots(e.g. chests)
        {
            foreach (var i in lootDefs)
                i.Populate(this, this);
        }

        public event EventHandler<ConsiderEventArgs> LootConsidering;

        static Random rand = new Random();

        public IEnumerable<Item> GetLoots(int min, int max)   //For independent loots(e.g. chests)
        {
            int retCount = rand.Next(min, max);
            foreach (var i in this)
            {
                if (rand.NextDouble() < i.Probabilty)
                {
                    yield return i.Item;
                    retCount--;
                }
                if (retCount == 0)
                    yield break;
            }
        }

        public void Handle(Enemy enemy, RealmTime time)
        {
            List<Item> sharedLoots = new List<Item>();
            foreach (var i in this)
            {
                if (rand.NextDouble() < i.Probabilty)
                    sharedLoots.Add(i.Item);
            }

            var dats = enemy.DamageCounter.GetPlayerData();
            Dictionary<Player, IList<Item>> loots = enemy.DamageCounter.GetPlayerData().ToDictionary(
                d => d.Item1, d => (IList<Item>)new List<Item>());

            foreach (var loot in sharedLoots.Where(item => item.Soulbound))
                loots[dats[rand.Next(dats.Length)].Item1].Add(loot);

            var consideration = new List<LootDef>();
            foreach (var dat in dats)
            {
                consideration.Clear();
                ConsiderEventArgs e = new ConsiderEventArgs(enemy, dat, rand, consideration);
                if (LootConsidering != null)
                    LootConsidering(this, e);

                IList<Item> playerLoot = loots[dat.Item1];
                foreach (var i in consideration)
                {
                    if (rand.NextDouble() < i.Probabilty)
                        playerLoot.Add(i.Item);
                }
            }

            AddBagsToWorld(enemy, sharedLoots, loots);
        }

        void AddBagsToWorld(Enemy enemy,IList<Item> shared, IDictionary<Player, IList<Item>> soulbound)
        {
            List<Player> pub = new List<Player>();  //only people not getting soulbound
            foreach (var i in soulbound)
            {
                if (i.Value.Count > 0)
                    ShowBags(enemy, i.Value, i.Key);
                else
                    pub.Add(i.Key);
            }
            if (pub.Count > 0 && shared.Count > 0)
                ShowBags(enemy, shared, pub.ToArray());
        }

        void ShowBags(Enemy enemy, IEnumerable<Item> loots, params Player[] owners)
        {
            int bagType = 0;
            Item[] items = new Item[8];
            int idx = 0;

            short bag;
            Container container;
            foreach (var i in loots)
            {
                if (i.BagType > bagType) bagType = i.BagType;
                items[idx] = i;
                idx++;

                if (idx == 8)
                {
                    bag = 0x0500;
                    switch (bagType)
                    {
                        case 0: bag = 0x0500; break;
                        case 1: bag = 0x0503; break;
                        case 2: bag = 0x0507; break;
                        case 3: bag = 0x0508; break;
                        case 4: bag = 0x0509; break;
                    }
                    container = new Container(bag, 1000 * 60, true);
                    for (int j = 0; j < 8; j++)
                        container.Inventory[j] = items[j];
                    container.BagOwners = owners.Select(x => x.AccountId).ToArray();
                    container.Move(
                        enemy.X + (float)((rand.NextDouble() * 2 - 1) * 0.5),
                        enemy.Y + (float)((rand.NextDouble() * 2 - 1) * 0.5));
                    container.Size = 80;
                    enemy.Owner.EnterWorld(container);

                    bagType = 0;
                    items = new Item[8];
                    idx = 0;
                }
            }

            if (idx > 0)
            {
                bag = 0x0500;
                switch (bagType)
                {
                    case 0: bag = 0x0500; break;
                    case 1: bag = 0x0503; break;
                    case 2: bag = 0x0507; break;
                    case 3: bag = 0x0508; break;
                    case 4: bag = 0x0509; break;
                }
                container = new Container(bag, 1000 * 60, true);
                for (int j = 0; j < idx; j++)
                    container.Inventory[j] = items[j];
                container.BagOwners = owners.Select(x => x.AccountId).ToArray();
                container.Move(
                    enemy.X + (float)((rand.NextDouble() * 2 - 1) * 0.5),
                    enemy.Y + (float)((rand.NextDouble() * 2 - 1) * 0.5));
                container.Size = 80;
                enemy.Owner.EnterWorld(container);
            }
        }
    }
}
