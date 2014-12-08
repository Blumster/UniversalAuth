using System;
using System.IO;

namespace UniversalAuth.Network.Packet.Client
{
    public class ServerListExtPacket : IPacket
    {
        public UInt32 SessionId1 { get; set; }
        public UInt32 SessionId2 { get; set; }
        public Byte ListKind { get; set; }

        public void Unserialize(BinaryReader reader)
        {
            SessionId1 = reader.ReadUInt32();
            SessionId2 = reader.ReadUInt32();
            ListKind = reader.ReadByte();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write((Byte) ClientOpcode.ServerListExt);
            writer.Write(SessionId1);
            writer.Write(SessionId2);
            writer.Write(ListKind);
        }

        public override String ToString()
        {
            return String.Format("ServerListExtPacket({0}, {1}, {2})", SessionId1, SessionId2, ListKind);
        }

        public void Dispose()
        {
        }
    }
}
