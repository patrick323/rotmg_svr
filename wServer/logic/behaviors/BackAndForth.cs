using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.realm;

namespace wServer.logic.behaviors
{
    class BackAndForth : CycleBehavior
    {
        //State storage: remaining distance

        float speed;
        public BackAndForth(double speed)
        {
            this.speed = (float)speed;
        }

        const int DISTANCE = 5;

        protected override void TickCore(Entity host, RealmTime time, ref object state)
        {
            float dist;
            if (state == null) dist = DISTANCE;
            else dist = (float)state;

            Status = CycleStatus.NotStarted;

            if (host.HasConditionEffect(ConditionEffects.Paralyzed)) return;

            float moveDist = host.GetSpeed(speed) * (time.thisTickTimes / 1000f);
            if (dist > 0)
            {
                Status = CycleStatus.InProgress;
                host.ValidateAndMove(host.X + moveDist, host.Y);
                host.UpdateCount++;
                dist -= moveDist;
                if (dist <= 0)
                {
                    dist = -DISTANCE;
                    Status = CycleStatus.Completed;
                }
            }
            else
            {
                Status = CycleStatus.InProgress;
                host.ValidateAndMove(host.X - moveDist, host.Y);
                host.UpdateCount++;
                dist -= moveDist;
                if (dist >= 0)
                {
                    dist = DISTANCE;
                    Status = CycleStatus.Completed;
                }
            }

            state = dist;
        }
    }
}
