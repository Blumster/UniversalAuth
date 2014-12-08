using System;
using System.IO;

namespace UniversalAuth.Network.Packet.Server
{
    public class PlayFailPacket : IPacket
    {
        public Byte ResultCode { get; set; }

        public void Unserialize(BinaryReader reader)
        {
            ResultCode = reader.ReadByte();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write((Byte) ServerOpcode.PlayFail);
            writer.Write(ResultCode);
        }

        public override String ToString()
        {
            return String.Format("PlayFailPacket({0})", ResultCode);
        }

        public void Dispose()
        {
        }
    }
}
