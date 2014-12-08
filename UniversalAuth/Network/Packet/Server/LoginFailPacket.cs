using System;
using System.IO;

namespace UniversalAuth.Network.Packet.Server
{
    public class LoginFailPacket : IPacket
    {
        public Byte ResultCode { get; set; }

        public void Unserialize(BinaryReader reader)
        {
            ResultCode = reader.ReadByte();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write((Byte) ServerOpcode.LoginFail);
            writer.Write(ResultCode);
        }

        public override String ToString()
        {
            return String.Format("LoginFailPacket({0})", ResultCode);
        }

        public void Dispose()
        {
        }
    }
}
