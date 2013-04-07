using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.realm;
using Mono.Game;
using wServer.realm.entities;

namespace wServer.logic.behaviors
{
    class StayBack : Behavior
    {
        //State storage: none

        float speed;
        float distance;
        public StayBack(double speed, double distance = 8)
        {
            this.speed = (float)speed;
            this.distance = (float)distance;
        }

        protected override bool? TickCore(Entity host, RealmTime time, ref object state)
        {
            if (host.HasConditionEffect(ConditionEffects.Paralyzed)) return true;

            var player = (Player)host.GetNearestEntity(distance, null);
            if (player != null)
            {
                Vector2 vect;
                vect = new Vector2(player.X - host.X, player.Y - host.Y);
                vect.Normalize();
                float dist = host.GetSpeed(speed) * (time.thisTickTimes / 1000f);
                host.ValidateAndMove(host.X + (-vect.X) * dist, host.Y + (-vect.Y) * dist);
                host.UpdateCount++;
                return true;
            }

            return false;
        }
    }
}
