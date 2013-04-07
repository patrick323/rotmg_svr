﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.realm;
using wServer.svrPackets;

namespace wServer.logic.behaviors
{
    class Flash : Behavior
    {
        //State storage: none

        uint color;
        int flashPeriod;
        int flashRepeats;
        public Flash(uint color, int flashPeriod, int flashRepeats)
        {
            this.color = color;
            this.flashPeriod = flashPeriod;
            this.flashRepeats = flashRepeats;
        }

        protected override void TickCore(Entity host, RealmTime time, ref object state) { }

        protected override void OnStateEntry(Entity host, RealmTime time, ref object state)
        {
            host.Owner.BroadcastPacket(new ShowEffectPacket()
            {
                EffectType = EffectType.Flashing,
                PosA = new Position() { X = flashPeriod, Y = flashRepeats },
                TargetId = host.Id,
                Color = new ARGB(color)
            }, null);
        }
    }
}
