using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.realm;

namespace wServer.logic.behaviors
{
    class Prioritize : Behavior
    {
        //State storage: none

        Behavior[] children;
        public Prioritize(params Behavior[] children)
        {
            this.children = children;
        }

        protected override bool? TickCore(Entity host, RealmTime time, ref object state)
        {
            int index;
            if (state == null) index = -1;
            else index = (int)state;

            if (index == -1)    //select
            {
                for (int i = 0; i < children.Length; i++)
                    if (children[i].Tick(host, time) ?? false)
                    {
                        index = i;
                        return null;
                    }
            }
            else                //run a cycle
            {
                if (children[index].Tick(host, time) ?? false)
                    index = -1;
            }

            state = index;
            return null;


            return false;
        }
    }
}
