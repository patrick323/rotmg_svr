using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.realm;
using Mono.Game;

namespace wServer.logic.behaviors
{
    class Wander : Behavior
    {
        //State storage: direction & remain time
        class WanderStorage
        {
            public Vector2 Direction;
            public float RemainingDistance;
        }


        float speed;
        public Wander(double speed)
        {
            this.speed = (float)speed;
        }

        static Cooldown period = new Cooldown(500, 200);
        protected override bool? TickCore(Entity host, RealmTime time, ref object state)
        {
            WanderStorage storage;
            if (state == null) storage = new WanderStorage();
            else storage = (WanderStorage)state;

            if (host.HasConditionEffect(ConditionEffects.Paralyzed)) return true;

            bool ret = false;
            if (storage.RemainingDistance <= 0)
            {
                storage.Direction = new Vector2(Random.Next(-1, 2), Random.Next(-1, 2));
                storage.Direction.Normalize();
                storage.RemainingDistance = period.Next(Random) / 1000f;
                ret = true;
            }
            float dist = host.GetSpeed(speed) * (time.thisTickTimes / 1000f);
            host.ValidateAndMove(host.X + storage.Direction.X * dist, host.Y + storage.Direction.Y * dist);
            host.UpdateCount++;

            storage.RemainingDistance -= dist;

            state = storage;
            return ret;
        }
    }
}
