using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.realm;
using Mono.Game;
using wServer.realm.entities;

namespace wServer.logic.behaviors
{
    class Follow : Behavior
    {
        //State storage: follow state
        enum FollowState
        {
            DontKnowWhere,
            Acquired,
            Resting
        }

        float speed;
        float acquireRange;
        float range;
        public Follow(double speed, double acquireRange = 10, double range = 6)
        {
            this.speed = (float)speed;
            this.acquireRange = (float)acquireRange;
            this.range = (float)range;
        }

        protected override bool? TickCore(Entity host, RealmTime time, ref object state)
        {
            FollowState s;
            if (state == null) s = FollowState.DontKnowWhere;
            else s = (FollowState)state;

            bool ret = false;

            if (host.HasConditionEffect(ConditionEffects.Paralyzed)) return true;
            
            var player = (Player)host.GetNearestEntity(acquireRange, null);
            Vector2 vect;
            switch (s)
            {
                case FollowState.DontKnowWhere:
                    if (player != null)
                    {
                        s = FollowState.Acquired;
                        goto case FollowState.Acquired;
                    }
                    break;
                case FollowState.Acquired:
                    if (player == null)
                    {
                        s = FollowState.DontKnowWhere;
                        break;
                    }
                    vect = new Vector2(player.X - host.X, player.Y - host.Y);
                    if (vect.Length() > range)
                    {
                        vect.Normalize();
                        float dist = host.GetSpeed(speed) * (time.thisTickTimes / 1000f);
                        host.ValidateAndMove(host.X + vect.X * dist, host.Y + vect.Y * dist);
                        host.UpdateCount++;
                        ret = true;
                    }
                    else
                        s = FollowState.Resting;
                    break;
                case FollowState.Resting:
                    if (player == null)
                    {
                        s = FollowState.DontKnowWhere;
                        break;
                    }
                    vect = new Vector2(player.X - host.X, player.Y - host.Y);
                    if (vect.Length() > (range + acquireRange) / 2)
                    {
                        s = FollowState.Acquired;
                        goto case FollowState.Acquired;
                    }
                    break;

            }

            state = s;
            return ret;
        }
    }
}
