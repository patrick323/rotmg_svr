using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wServer.logic.loot
{
    interface ILootDef
    {
        void Populate(Loot loot, IList<LootDef> defs);
    }

    class ItemLoot : ILootDef
    {
        string item;
        double probability;
        public ItemLoot(string item, double probability)
        {
            this.item = item;
            this.probability = probability;
        }

        public void Populate(Loot loot, IList<LootDef> defs)
        {
            defs.Add(new LootDef(XmlDatas.ItemDescs[XmlDatas.IdToType[item]], probability));
        }
    }

    enum ItemType
    {
        Weapon,
        Ability,
        Armor,
        Ring,
        Potion
    }
    class TierLoot : ILootDef
    {
        public static readonly int[] WeaponsT = new int[] { 1, 2, 3, 8, 17, };
        public static readonly int[] AbilityT = new int[] { 4, 5, 11, 12, 13, 15, 16, 18, 19, 20, 21, 22, 23, };
        public static readonly int[] ArmorsT = new int[] { 6, 7, 14, };
        public static readonly int[] RingT = new int[] { 9 };
        public static readonly int[] PotionT = new int[] { 10 };

        public static readonly Dictionary<int, Item[]> WeaponItems;
        public static readonly Dictionary<int, Item[]> AbilityItems;
        public static readonly Dictionary<int, Item[]> ArmorItems;
        public static readonly Dictionary<int, Item[]> RingItems;
        public static readonly Dictionary<int, Item[]> PotionItems;

        static TierLoot()
        {
            WeaponItems = new Dictionary<int, Item[]>();
            for (int tier = 1; tier < 20; tier++)
            {
                List<Item> items = new List<Item>();
                foreach (var i in WeaponsT)
                    items.AddRange(XmlDatas.ItemDescs.Select(_ => _.Value).Where(_ => _.Tier == tier && _.SlotType == i));
                if (items.Count == 0)
                    break;
                else
                    WeaponItems[tier] = items.ToArray();
            }
            AbilityItems = new Dictionary<int, Item[]>();
            for (int tier = 1; tier < 20; tier++)
            {
                List<Item> items = new List<Item>();
                foreach (var i in AbilityT)
                    items.AddRange(XmlDatas.ItemDescs.Select(_ => _.Value).Where(_ => _.Tier == tier && _.SlotType == i));
                if (items.Count == 0)
                    break;
                else
                    AbilityItems[tier] = items.ToArray();
            }
            ArmorItems = new Dictionary<int, Item[]>();
            for (int tier = 1; tier < 20; tier++)
            {
                List<Item> items = new List<Item>();
                foreach (var i in ArmorsT)
                    items.AddRange(XmlDatas.ItemDescs.Select(_ => _.Value).Where(_ => _.Tier == tier && _.SlotType == i));
                if (items.Count == 0)
                    break;
                else
                    ArmorItems[tier] = items.ToArray();
            }
            RingItems = new Dictionary<int, Item[]>();
            for (int tier = 1; tier < 20; tier++)
            {
                List<Item> items = new List<Item>();
                foreach (var i in RingT)
                    items.AddRange(XmlDatas.ItemDescs.Select(_ => _.Value).Where(_ => _.Tier == tier && _.SlotType == i));
                if (items.Count == 0)
                    break;
                else
                    RingItems[tier] = items.ToArray();
            }
            PotionItems = new Dictionary<int, Item[]>();
            for (int tier = 1; tier < 20; tier++)
            {
                List<Item> items = new List<Item>();
                foreach (var i in PotionT)
                    items.AddRange(XmlDatas.ItemDescs.Select(_ => _.Value).Where(_ => _.Tier == tier && _.SlotType == i));
                if (items.Count == 0)
                    break;
                else
                    PotionItems[tier] = items.ToArray();
            }
        }

        byte tier;
        ItemType type;
        double probability;
        public TierLoot(byte tier, ItemType type, double probability)
        {
            this.tier = tier;
            this.type = type;
            this.probability = probability;
        }

        public void Populate(Loot loot, IList<LootDef> defs)
        {
            Item[] candidates;
            switch (type)
            {
                case ItemType.Weapon:
                    candidates = WeaponItems[tier];
                    break;
                case ItemType.Ability:
                    candidates = AbilityItems[tier];
                    break;
                case ItemType.Armor:
                    candidates = ArmorItems[tier];
                    break;
                case ItemType.Ring:
                    candidates = RingItems[tier];
                    break;
                case ItemType.Potion:
                    candidates = RingItems[tier];
                    break;
                default:
                    throw new NotSupportedException(type.ToString());
            }
            foreach (var i in candidates)
                defs.Add(new LootDef(i, probability / candidates.Length));
        }
    }

    class Threshold : ILootDef
    {
        double threshold;
        ILootDef[] children;
        public Threshold(double threshold, params ILootDef[] children)
        {
            this.threshold = threshold;
            this.children = children;
        }

        public void Populate(Loot loot, IList<LootDef> defs)
        {
            defs = new List<LootDef>();
            foreach (var i in children)
                i.Populate(loot, defs);
            loot.LootConsidering += (sender, e) =>
            {
                if (e.PlayerData.Item2 / (double)e.Enemy.ObjectDesc.MaxHP >= threshold)
                    foreach (var i in defs)
                        e.LootDefs.Add(i);
            };
        }
    }
}
