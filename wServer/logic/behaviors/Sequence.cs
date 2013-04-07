using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.realm;

namespace wServer.logic.behaviors
{
    //replacement for simple sequential state transition
    class Sequence : Behavior
    {
        //State storage: index

        Behavior[] children;
        public Sequence(params Behavior[] children)
        {
            this.children = children;
        }

        protected override bool? TickCore(Entity host, RealmTime time, ref object state)
        {
            int index;
            if (state == null) index = 0;
            else index = (int)state;

            if (children[index].Tick(host, time) ?? false)
            {
                index++;
                if (index == children.Length) index = 0;
            }

            state = index;
            return null;
        }
    }
}
