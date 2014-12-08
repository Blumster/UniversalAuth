using System;
using System.IO;

namespace UniversalAuth.Network.Packet
{
    public interface IPacket : IDisposable
    {
        void Unserialize(BinaryReader reader);
        void Serialize(BinaryWriter writer);
    }
}
