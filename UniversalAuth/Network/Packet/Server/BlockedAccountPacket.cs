using System;
using System.IO;

namespace UniversalAuth.Network.Packet.Server
{
    public class BlockedAccountPacket : IPacket
    {
        public UInt32 Reason { get; set; }

        public void Unserialize(BinaryReader reader)
        {
            Reason = reader.ReadUInt32();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write((Byte) ServerOpcode.BlockedAccount);
            writer.Write(Reason);
        }

        public override String ToString()
        {
            return String.Format("BlockedAccountPacket({0})", Reason);
        }

        public void Dispose()
        {
        }
    }
}
