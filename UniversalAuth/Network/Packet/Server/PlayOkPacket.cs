using System;
using System.IO;

namespace UniversalAuth.Network.Packet.Server
{
    public class PlayOkPacket : IPacket
    {
        public UInt32 OneTimeKey { get; set; }
        public UInt32 UserId { get; set; }
        public Byte ServerId { get; set; }

        public void Unserialize(BinaryReader reader)
        {
            OneTimeKey = reader.ReadUInt32();
            UserId = reader.ReadUInt32();
            ServerId = reader.ReadByte();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write((Byte) ServerOpcode.PlayOk);
            writer.Write(OneTimeKey);
            writer.Write(UserId);
            writer.Write(ServerId);
        }

        public override String ToString()
        {
            return String.Format("PlayOkPacket({0}, {1}, {2})", OneTimeKey, UserId, ServerId);
        }

        public void Dispose()
        {
        }
    }
}
