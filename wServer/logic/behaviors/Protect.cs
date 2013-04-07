using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.realm;
using Mono.Game;
using wServer.realm.entities;

namespace wServer.logic.behaviors
{
    class Protect : Behavior
    {
        //State storage: protect state
        enum ProtectState
        {
            DontKnowWhere,
            Protecting,
            Protected,
        }

        float speed;
        short protectee;
        float acquireRange;
        float protectionRange;
        float reprotectRange;
        public Protect(double speed, string protectee, double acquireRange = 10, double protectionRange = 3, double reprotectRange = 1.5)
        {
            this.speed = (float)speed;
            this.protectee = XmlDatas.IdToType[protectee];
            this.acquireRange = (float)acquireRange;
            this.protectionRange = (float)protectionRange;
            this.reprotectRange = (float)reprotectRange;
        }

        protected override bool? TickCore(Entity host, RealmTime time, ref object state)
        {
            ProtectState s;
            if (state == null) s = ProtectState.DontKnowWhere;
            else s = (ProtectState)state;

            bool ret = false;

            if (host.HasConditionEffect(ConditionEffects.Paralyzed)) return true;

            var entity = host.GetNearestEntity(acquireRange, protectee);
            Vector2 vect;
            switch (s)
            {
                case ProtectState.DontKnowWhere:
                    if (entity != null)
                    {
                        s = ProtectState.Protecting;
                        goto case ProtectState.Protecting;
                    }
                    break;
                case ProtectState.Protecting:
                    if (entity == null)
                    {
                        s = ProtectState.DontKnowWhere;
                        break;
                    }
                    vect = new Vector2(entity.X - host.X, entity.Y - host.Y);
                    if (vect.Length() > reprotectRange)
                    {
                        vect.Normalize();
                        float dist = host.GetSpeed(speed) * (time.thisTickTimes / 1000f);
                        host.ValidateAndMove(host.X + vect.X * dist, host.Y + vect.Y * dist);
                        host.UpdateCount++;
                        ret = true;
                    }
                    else
                        s = ProtectState.Protected;
                    break;
                case ProtectState.Protected:
                    if (entity == null)
                    {
                        s = ProtectState.DontKnowWhere;
                        break;
                    }
                    vect = new Vector2(entity.X - host.X, entity.Y - host.Y);
                    if (vect.Length() > protectionRange)
                    {
                        s = ProtectState.Protecting;
                        goto case ProtectState.Protecting;
                    }
                    break;

            }

            state = s;
            return ret;
        }
    }
}
