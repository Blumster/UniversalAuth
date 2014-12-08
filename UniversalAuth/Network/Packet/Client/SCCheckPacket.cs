using System;
using System.IO;

namespace UniversalAuth.Network.Packet.Client
{
    public class SCCheckPacket : IPacket
    {
        public UInt32 UserId { get; set; }
        public UInt32 CardValue { get; set; }

        public void Unserialize(BinaryReader reader)
        {
            UserId = reader.ReadUInt32();
            CardValue = reader.ReadUInt32();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write((Byte) ClientOpcode.SCCheck);
            writer.Write(UserId);
            writer.Write(CardValue);
        }

        public override String ToString()
        {
            return String.Format("SCCheckPacket({0}, {1})", UserId, CardValue);
        }

        public void Dispose()
        {
        }
    }
}
