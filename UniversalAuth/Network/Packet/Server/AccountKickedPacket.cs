using System;
using System.IO;

namespace UniversalAuth.Network.Packet.Server
{
    public class AccountKickedPacket : IPacket
    {
        public Byte ReasonCode { get; set; }

        public void Unserialize(BinaryReader reader)
        {
            ReasonCode = reader.ReadByte();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write((Byte) ServerOpcode.AccountKicked);
            writer.Write(ReasonCode);
        }

        public override String ToString()
        {
            return String.Format("AccountKickedPacket({0})", ReasonCode);
        }

        public void Dispose()
        {
        }
    }
}
