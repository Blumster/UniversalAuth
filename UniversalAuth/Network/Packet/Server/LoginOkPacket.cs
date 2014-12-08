using System;
using System.IO;

namespace UniversalAuth.Network.Packet.Server
{
    public class LoginOkPacket : IPacket
    {
        public UInt32 SessionId1 { get; set; }
        public UInt32 SessionId2 { get; set; }
        public UInt32 UpdateKey1 { get; set; }
        public UInt32 UpdateKey2 { get; set; }
        public UInt32 PayStat { get; set; }
        public UInt32 RemainingTime { get; set; }
        public UInt32 QuotaTime { get; set; }
        public UInt32 WarnFlag { get; set; }
        public UInt32 LoginFlag { get; set; }

        public void Unserialize(BinaryReader reader)
        {
            SessionId1 = reader.ReadUInt32();
            SessionId2 = reader.ReadUInt32();
            UpdateKey1 = reader.ReadUInt32();
            UpdateKey2 = reader.ReadUInt32();
            PayStat = reader.ReadUInt32();
            RemainingTime = reader.ReadUInt32();
            QuotaTime = reader.ReadUInt32();
            WarnFlag = reader.ReadUInt32();
            LoginFlag = reader.ReadUInt32();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write((Byte) ServerOpcode.LoginOk);
            writer.Write(SessionId1);
            writer.Write(SessionId2);
            writer.Write(UpdateKey1);
            writer.Write(UpdateKey2);
            writer.Write(PayStat);
            writer.Write(RemainingTime);
            writer.Write(QuotaTime);
            writer.Write(WarnFlag);
            writer.Write(LoginFlag);
        }

        public override String ToString()
        {
            return String.Format("LoginOkPacket({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})", SessionId1, SessionId2, UpdateKey1, UpdateKey2, PayStat, RemainingTime, QuotaTime, WarnFlag, LoginFlag);
        }

        public void Dispose()
        {
        }
    }
}
