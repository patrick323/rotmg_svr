using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.realm;
using wServer.realm.entities;
using wServer.logic.loot;

namespace wServer.logic
{
    partial class BehaviorDb
    {
        public static void ResolveBehavior(Entity entity)
        {
            Tuple<State, Loot> def;
            if (Definitions.TryGetValue(entity.ObjectType, out def))
                entity.SwitchTo(def.Item1);
        }

        public static void ResolveLoot(Enemy enemy)
        {
            Tuple<State, Loot> def;
            if (Definitions.TryGetValue(enemy.ObjectType, out def) && def.Item2 != null)
            {
                enemy.DamageCounter.Dead += (sender, e) =>
                {
                    def.Item2.Handle(enemy, e.Time);
                };
            }
        }

        struct _
        {
            public _ Init(string objType, State rootState, params ILootDef[] defs)
            {
                rootState.Resolve(new Dictionary<string, State>());
                if (defs.Length > 0)
                {
                    var loot = new Loot();
                    foreach (var i in defs)
                        i.Populate(loot, loot);
                    Definitions.Add(XmlDatas.IdToType[objType], new Tuple<State, Loot>(rootState, loot));
                }
                else
                    Definitions.Add(XmlDatas.IdToType[objType], new Tuple<State, Loot>(rootState, null));
                return this;
            }
        }

        static _ Behav()
        {
            if (Definitions == null)
                Definitions = new Dictionary<short, Tuple<State, Loot>>();
            return new _();
        }

        public static Dictionary<short, Tuple<State, Loot>> Definitions;
    }
}
