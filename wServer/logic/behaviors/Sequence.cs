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

        CycleBehavior[] children;
        public Sequence(params CycleBehavior[] children)
        {
            this.children = children;
        }

        protected override void TickCore(Entity host, RealmTime time, ref object state)
        {
            int index;
            if (state == null) index = 0;
            else index = (int)state;

            children[index].Tick(host, time);
            if (children[index].Status == CycleStatus.Completed)
            {
                index++;
                if (index == children.Length) index = 0;
            }

            state = index;
        }
    }
}
