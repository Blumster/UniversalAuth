using System;
using System.IO;

namespace UniversalAuth.Network.Packet.Client
{
    public class AboutToPlayPacket : IPacket
    {
        public UInt32 SessionId1 { get; set; }
        public UInt32 SessionId2 { get; set; }
        public Byte ServerId { get; set; }

        public void Unserialize(BinaryReader reader)
        {
            SessionId1 = reader.ReadUInt32();
            SessionId2 = reader.ReadUInt32();
            ServerId = reader.ReadByte();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write((Byte) ClientOpcode.AboutToPlay);
            writer.Write(SessionId1);
            writer.Write(SessionId2);
            writer.Write(ServerId);
        }

        public override String ToString()
        {
            return String.Format("AboutToPlayPacket({0}, {1}, {2})", SessionId1, SessionId2, ServerId);
        }

        public void Dispose()
        {
        }
    }
}
