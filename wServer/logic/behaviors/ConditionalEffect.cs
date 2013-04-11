using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.realm;

namespace wServer.logic.behaviors
{
    class ConditionalEffect : Behavior
    {
        //State storage: none

        ConditionEffectIndex effect;
        public ConditionalEffect(ConditionEffectIndex effect)
        {
            this.effect = effect;
        }

        protected override void OnStateEntry(Entity host, RealmTime time, ref object state)
        {
            host.ApplyConditionEffect(new ConditionEffect()
            {
                Effect = effect,
                DurationMS = -1
            });
        }

        protected override void OnStateExit(Entity host, RealmTime time, ref object state)
        {
            host.ApplyConditionEffect(new ConditionEffect()
            {
                Effect = effect,
                DurationMS = 0
            });
        }

        protected override void TickCore(Entity host, RealmTime time, ref object state)
        { }
    }
}
