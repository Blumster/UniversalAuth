using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace UniversalAuth.Network.Packet.Server
{
    using Data;

    public class SendServerListExtPacket : IPacket
    {
        public List<ServerInfoEx> ServerList { get; set; }

        public void Unserialize(BinaryReader reader)
        {
            ServerList = new List<ServerInfoEx>();

            var count = reader.ReadByte();
            /*var unk = */reader.ReadByte();

            for (var i = 0; i < count; ++i)
            {
                ServerList.Add(new ServerInfoEx
                {
                    ServerId       = reader.ReadByte(),
                    Ip             = new IPAddress(reader.ReadBytes(4)),
                    Port           = reader.ReadUInt32(),
                    AgeLimit       = reader.ReadByte(),
                    PKFlag         = reader.ReadByte(),
                    CurrentPlayers = reader.ReadUInt16(),
                    MaxPlayers     = reader.ReadUInt16(),
                    Status         = reader.ReadByte()
                });
            }
        }

        public void Serialize(BinaryWriter writer)
        {
            if (ServerList == null)
                throw new InvalidOperationException("You must specify a list of ServerInfo before you can serialize it!");

            var count = ServerList.Count(s => s.Status != 0);
            if (count >= 16)
                count = 16;

            writer.Write((Byte) ServerOpcode.SendServerListExt);
            writer.Write((Byte) count);
            writer.Write((Byte) 0);

            var c = 0U;

            foreach (var s in ServerList.Where(s => s.Status != 0))
            {
                writer.Write(s.ServerId);
                writer.Write(s.Ip.GetAddressBytes());
                writer.Write(s.Port);
                writer.Write(s.AgeLimit);
                writer.Write(s.PKFlag);
                writer.Write(s.CurrentPlayers);
                writer.Write(s.MaxPlayers);
                writer.Write(s.Status);

                if (++c == count)
                    break;
            }
        }

        public override String ToString()
        {
            return String.Format("SendServerListExtPacket(Count: {0})", ServerList != null ? ServerList.Count : -1);
        }

        public void Dispose()
        {
        }
    }
}
