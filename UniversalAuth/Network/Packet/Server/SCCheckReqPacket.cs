using System;
using System.IO;

namespace UniversalAuth.Network.Packet.Server
{
    public class SCCheckReqPacket : IPacket
    {
        public UInt32 UserId { get; set; }
        public Byte CardKey { get; set; }

        public void Unserialize(BinaryReader reader)
        {
            UserId = reader.ReadUInt32();
            CardKey = reader.ReadByte();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write((Byte) ServerOpcode.SCCheckReq);
            writer.Write(UserId);
            writer.Write(CardKey);
        }

        public override String ToString()
        {
            return String.Format("SCCheckReqPacket({0}, {1})", UserId, CardKey);
        }

        public void Dispose()
        {
        }
    }
}
