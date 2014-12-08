using System;
using System.Diagnostics;
using System.IO;

namespace UniversalAuth.Network.Packet.Server
{
    public class BlockedAccountWithMsgPacket : IPacket
    {
        public void Unserialize(BinaryReader reader)
        {
            var count = reader.ReadByte();
            for (var i = 0; i < count; ++i)
            {
                Debugger.Break();
            }

            throw new NotImplementedException();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write((Byte) ServerOpcode.BlockedAccountWithMessage);
            writer.Write((Byte) 0); // TODO: when needed

            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}
