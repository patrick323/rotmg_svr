using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.realm;

namespace wServer.logic.behaviors
{
    class TransformOnDeath : Behavior
    {
        short target;
        int min;
        int max;
        float probability;
        public TransformOnDeath(string target, int min = 1, int max = 1, double probability = 1)
        {
            this.target = XmlDatas.IdToType[target];
            this.min = min;
            this.max = max;
            this.probability = (float)probability;
        }

        protected internal override void Resolve(State parent)
        {
            parent.Death += (sender, e) =>
            {
                if (Random.NextDouble() < probability)
                {
                    int count = Random.Next(min, max + 1);
                    for (int i = 0; i < count; i++)
                    {
                        Entity entity = Entity.Resolve(target);

                        entity.Move(e.Host.X, e.Host.Y);
                        e.Host.Owner.EnterWorld(entity);
                    }
                }
            };
        }
        protected override void TickCore(Entity host, RealmTime time, ref object state)
        { }
    }
}
