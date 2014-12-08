using System;
using System.IO;

namespace UniversalAuth.Network.Packet.Server
{
    public class ProtocolVersionPacket : IPacket
    {
        public UInt32 ProtocolVersion { get; set; }
        public UInt32 OneTimeKey { get; set; }

        public void Unserialize(BinaryReader reader)
        {
            OneTimeKey = reader.ReadUInt32();
            ProtocolVersion = reader.ReadUInt32();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write((Byte) ServerOpcode.ProtocolVersion);
            writer.Write(OneTimeKey);
            writer.Write(ProtocolVersion);
        }

        public override String ToString()
        {
            return String.Format("ProtocolVersionPacket({0}, {1})", ProtocolVersion, OneTimeKey);
        }

        public void Dispose()
        {
        }
    }
}
