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

        bool Contains(State self, State target)
        {
            if (self == target) return true;
            else if (self.Parent != null) return Contains(self.Parent, target);
            else return false;
        }

        protected override void TickCore(Entity host, RealmTime time, ref object state)
        {
            foreach (var i in host.GetNearestEntities(range, children))
                if (!Contains(i.CurrentState, targetState))
                    i.SwitchTo(targetState);
        }
    }
}
