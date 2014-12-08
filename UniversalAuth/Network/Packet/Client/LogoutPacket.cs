using System;
using System.IO;

namespace UniversalAuth.Network.Packet.Client
{
    public class LogoutPacket : IPacket
    {
        public UInt32 SessionId1 { get; set; }
        public UInt32 SessionId2 { get; set; }

        public void Unserialize(BinaryReader reader)
        {
            SessionId1 = reader.ReadUInt32();
            SessionId2 = reader.ReadUInt32();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write((Byte) ClientOpcode.Logout);
            writer.Write(SessionId1);
            writer.Write(SessionId2);
        }

        public override String ToString()
        {
            return String.Format("LogoutPacket({0}, {1})", SessionId1, SessionId2);
        }

        public void Dispose()
        {
        }
    }
}
