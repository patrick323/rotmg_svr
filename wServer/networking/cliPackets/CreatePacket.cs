using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wServer.networking.cliPackets
{
    public class CreatePacket : ClientPacket
    {
        public short ObjectType { get; set; }

        public override PacketID ID { get { return PacketID.Create; } }
        public override Packet CreateInstance() { return new CreatePacket(); }

        protected override void Read(NReader rdr)
        {
            ObjectType = rdr.ReadInt16();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.Write(ObjectType);
        }
    }
}
