using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.realm;
using wServer.realm.entities;
using wServer.svrPackets;

namespace wServer.logic.behaviors
{
    class Order : Behavior
    {
        //State storage: none
        //Note the target state must be defined before this is used

        double range;
        short children;
        State targetState;

        public Order(double range, string children, string targetState)
        {
            this.range = range;
            this.children = XmlDatas.IdToType[children];
            this.targetState = FindState(BehaviorDb.Definitions[this.children].Item1, targetState);
        }

        static State FindState(State state, string name)
        {
            if (state.Name == name) return state;
            State ret;
            foreach (var i in state.States)
            {
                if ((ret = FindState(i, name)) != null)
                    return ret;
            }
            return null;
        }

        protected override void OnStateEntry(Entity host, RealmTime time, ref object state)
        {
            foreach (var i in host.GetNearestEntities(range, children))
                i.SwitchTo(targetState);
        }

        protected override void TickCore(Entity host, RealmTime time, ref object state) { }
    }
}
