using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.realm;

namespace wServer.logic.transitions
{
    class TimedTransition : Transition
    {
        //State storage: cooldown timer

        int time;

        public TimedTransition(int time, string targetState)
            : base(targetState)
        {
            this.time = time;
        }

        protected override bool TickCore(Entity host, RealmTime time, ref object state)
        {
            int cool;
            if (state == null) cool = this.time;
            else cool = (int)state;

            bool ret = false;
            if (cool <= 0)
            {
                ret = true;
                cool = this.time;
            }
            else
                cool -= time.thisTickTimes;

            state = cool;
            return ret;
        }
    }
}
