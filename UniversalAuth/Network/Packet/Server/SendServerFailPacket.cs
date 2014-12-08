using System;
using System.IO;

namespace UniversalAuth.Network.Packet.Server
{
    public class SendServerFailPacket : IPacket
    {
        public Byte ReasonCode { get; set; }

        public void Unserialize(BinaryReader reader)
        {
            ReasonCode = reader.ReadByte();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write((Byte) ServerOpcode.SendServerListFail);
            writer.Write(ReasonCode);
        }

        public override String ToString()
        {
            return String.Format("SendServerFailPacket({0})", ReasonCode);
        }

        public void Dispose()
        {
        }
    }
}
