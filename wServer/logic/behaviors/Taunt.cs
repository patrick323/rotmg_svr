using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.realm;
using wServer.realm.entities;
using wServer.svrPackets;
using Mono.Game;

namespace wServer.logic.behaviors
{
    class Taunt : Behavior
    {
        //State storage: none

        float probability;
        string text;
        public Taunt(string text, double probability = 1)
        {
            this.text = text;
            this.probability = (float)probability;
        }

        protected override void TickCore(Entity host, RealmTime time, ref object state) { }

        protected override void OnStateEntry(Entity host, RealmTime time, ref object state)
        {
            var taunt = this.text;
            if (taunt.Contains("{PLAYER}"))
            {
                Entity player = host.GetNearestEntity(10, null);
                if (player == null) return;
                taunt = taunt.Replace("{PLAYER}", player.Name);
            }
            taunt = taunt.Replace("{HP}", (host as Enemy).HP.ToString());

            host.Owner.BroadcastPacket(new TextPacket()
            {
                Name = "#" + (host.ObjectDesc.DisplayId ?? host.ObjectDesc.ObjectId),
                ObjectId = host.Id,
                Stars = -1,
                BubbleTime = 5,
                Recipient = "",
                Text = taunt,
                CleanText = ""
            }, null);
        }
    }
}
