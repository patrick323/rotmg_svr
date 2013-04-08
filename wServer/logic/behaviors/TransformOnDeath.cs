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
        public TransformOnDeath(string target)
        {
            this.target = XmlDatas.IdToType[target];
        }

        protected internal override void Resolve(State parent)
        {
            parent.Death += (sender, e) =>
            {
                Entity entity = Entity.Resolve(target);

                entity.Move(e.Host.X, e.Host.Y);
                e.Host.Owner.EnterWorld(entity);
            };
        }
        protected override void TickCore(Entity host, RealmTime time, ref object state)
        { }
    }
}
