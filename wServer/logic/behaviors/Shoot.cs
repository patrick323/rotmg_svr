using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.realm;
using wServer.realm.entities;
using wServer.svrPackets;

namespace wServer.logic.behaviors
{
    class Shoot : Behavior
    {
        //State storage: cooldown timer

        double radius;
        int count;
        double shootAngle;
        double? fixedAngle;
        double angleOffset;
        double? defaultAngle;
        int projectileIndex;
        Cooldown coolDown;

        public Shoot(double radius, int count = 1, double? shootAngle = null, 
            int projectileIndex = 0, double? fixedAngle = null, double angleOffset = 0,
            double? defaultAngle = null, Cooldown coolDown = new Cooldown())
        {
            this.radius = radius;
            this.count = count;
            this.shootAngle = count == 1 ? 0 : (shootAngle ?? 360.0 / count) * Math.PI / 180;
            this.fixedAngle = fixedAngle * Math.PI / 180;
            this.angleOffset = angleOffset * Math.PI / 180;
            this.defaultAngle = defaultAngle * Math.PI / 180;
            this.projectileIndex = projectileIndex;
            this.coolDown = coolDown.Normalize();
        }

        protected override bool? TickCore(Entity host, RealmTime time, ref object state)
        {
            int cool;
            if (state == null) cool = coolDown.Next(Random);
            else cool = (int)state;

            bool ret = false;
            if (cool <= 0)
            {
                if (host.HasConditionEffect(ConditionEffects.Stunned)) return true;

                Entity player = host.GetNearestEntity(radius, null);
                if (player != null || defaultAngle != null || fixedAngle != null)
                {
                    var desc = host.ObjectDesc.Projectiles[projectileIndex];

                    var a = fixedAngle ?? (player == null ? defaultAngle.Value : Math.Atan2(player.Y - host.Y, player.X - host.X));
                    a += angleOffset;

                    int dmg;
                    if (host is Character)
                        dmg = (host as Character).Random.Next(desc.MinDamage, desc.MaxDamage);
                    else
                        dmg = Random.Next(desc.MinDamage, desc.MaxDamage);

                    var startAngle = a - shootAngle * (count - 1) / 2;
                    byte prjId = 0;
                    Position prjPos = new Position() { X = host.X, Y = host.Y };
                    for (int i = 0; i < count; i++)
                    {
                        var prj = host.CreateProjectile(
                            desc, host.ObjectType, dmg, time.tickTimes,
                            prjPos, (float)(startAngle + shootAngle * i));
                        host.Owner.EnterWorld(prj);
                        if (i == 0)
                            prjId = prj.ProjectileId;
                    }

                    host.Owner.BroadcastPacket(new MultiShootPacket()
                    {
                        BulletId = prjId,
                        OwnerId = host.Id,
                        Position = prjPos,
                        Angle = (float)startAngle,
                        Damage = (short)dmg,
                        BulletType = (byte)(desc.BulletType),
                        AngleIncrement = (float)shootAngle,
                        NumShots = (byte)count,
                    }, null);
                    ret = true;
                }
                cool = coolDown.Next(Random);
            }
            else
                cool -= time.thisTickTimes;

            state = cool;
            return ret;
        }
    }
}
