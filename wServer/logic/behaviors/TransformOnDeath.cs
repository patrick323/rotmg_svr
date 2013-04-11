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
        float probability;
        public TransformOnDeath(string target, double probability = 1)
        {
            this.target = XmlDatas.IdToType[target];
            this.probability = (float)probability;
        }

        protected internal override void Resolve(State parent)
        {
            parent.Death += (sender, e) =>
            {
                if (Random.NextDouble() < probability)
                {
                    Entity entity = Entity.Resolve(target);

                    entity.Move(e.Host.X, e.Host.Y);
                    e.Host.Owner.EnterWorld(entity);
                }
            };
        }
        protected override void TickCore(Entity host, RealmTime time, ref object state)
        { }
    }
}
