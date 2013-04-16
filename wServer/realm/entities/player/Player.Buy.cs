using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.networking.cliPackets;
using wServer.networking.svrPackets;

namespace wServer.realm.entities
{
    partial class Player
    {
        public void Buy(BuyPacket pkt)
        {
            SellableObject obj = Owner.GetEntity(pkt.ObjectId) as SellableObject;
            if (obj != null)
                obj.Buy(this);
        }

        public void CheckCredits(CheckCreditsPacket pkt)
        {
            client.Database.ReadStats(client.Account);
            Credits = client.Account.Credits;
            UpdateCount++;
        }
    }
}
