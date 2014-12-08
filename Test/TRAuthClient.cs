using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

using UniversalAuth.Client;
using UniversalAuth.Data;
using UniversalAuth.Network.Packet.Server;

namespace Test
{
    public class TRAuthClient : AuthClient
    {
        public UInt32 CConnectionState { get; private set; }
        public String UserName { get; private set; }
        public String Password { get; private set; }
        public Boolean HasAuthKey { get; private set; }
        public Int32 RetryCount { get; private set; }
        public UInt64 SessionId { get; private set; }
        public List<ServerInfoEx> ServerList { get; private set; }
        public UInt32 ServerId { get; private set; }
        public UInt32 AuthKey { get; private set; }
        public UInt32 UserId { get; private set; }
        public Byte WhichServer { get; private set; }

        public TRAuthClient(String uName, String password)
        {
            SetUserAndPass(uName, password);

            WhichServer = 2;
            RetryCount = 6;
        }

        public void SetUserAndPass(String uName, String password)
        {
            UserName = uName;
            Password = password;
        }

        public void ClearAuthKey()
        {
            HasAuthKey = false;
            AuthKey = 0U;
        }

        public void SendShardSelection(Byte shard)
        {
            ServerId = shard;
            WhichServer = shard;

            SendAboutToPlay(SessionId, shard);
        }

        protected override void MsgAccountKicked(BinaryReader reader)
        {
            Console.WriteLine("Received Account Kicked");
        }

        protected override void MsgBlockedAccount(BinaryReader reader)
        {
            Console.WriteLine("The server is currently unavailable or this account is blocked. If your login attempt falls within the scheduled play times, please contact customer service for more information.");
        }

        protected override void MsgBlockedAccountWithMessage(BinaryReader reader)
        {
            Console.WriteLine("Received Blocked Account With Message");
        }

        protected override void MsgLoginFail(BinaryReader reader)
        {
            var packet = new LoginFailPacket();
            packet.Unserialize(reader);

            switch (packet.ResultCode)
            {
                case 7:
                case 13:
                case 14:
                    if (RetryCount-- > 0)
                    {
                        Thread.Sleep(1500);
                        SendLogin(UserName, Password, 8, 1);
                    }
                    break;

                case 1:
                case 6:
                case 8:
                case 15:
                case 20:
                    Console.WriteLine("The server is currently unavailable. Please try again later.");
                    break;

                case 4:
                case 5:
                case 10:
                case 11:
                case 12:
                case 16:
                case 17:
                case 18:
                    Console.WriteLine("There is a problem with your account, please contact customer service.");
                    break;

                case 2:
                case 3:
                    break;

                default:
                    Console.WriteLine("Unknow error.");
                    break;
            }
        }

        protected override void MsgLoginOk(BinaryReader reader)
        {
            Console.WriteLine("Received Login Ok");

            var packet = new LoginOkPacket();
            packet.Unserialize(reader);

            CConnectionState = 2;
            SessionId = (packet.SessionId1 << 32) | packet.SessionId2;

            SendServerListEx(SessionId);
        }

        protected override void MsgPlayFail(BinaryReader reader)
        {
            Console.WriteLine("Failed to login! Please try again in a moment.");
        }

        protected override void MsgPlayOk(BinaryReader reader)
        {
            Console.WriteLine("Received Play Ok");

            var packet = new PlayOkPacket();
            packet.Unserialize(reader);

            AuthKey = packet.OneTimeKey;
            HasAuthKey = true;
            UserId = packet.UserId;

            // Start global connection
        }

        protected override void MsgSendServerListFail(BinaryReader reader)
        {
            var packet = new SendServerFailPacket();
            packet.Unserialize(reader);

            Console.WriteLine("Received Send Server Failed with code: {0}", packet.ReasonCode);
        }

        protected override void MsgSendServerListExt(BinaryReader reader)
        {
            Console.WriteLine("Received Server List Extended");

            var packet = new SendServerListExtPacket();
            packet.Unserialize(reader);

            CConnectionState = 3;

            if (packet.ServerList.Count > 0)
                SendShardSelection(packet.ServerList[0].ServerId);
            else
                Console.WriteLine("There are no servers available!");
        }

        protected override void MsgSCCheckReq(BinaryReader reader)
        {
            Console.WriteLine("Recveived SCCheck Request");
        }

        protected override void OnConnected(IPEndPoint severAddress)
        {
            RetryCount = 6;

            SendLogin(UserName, Password, 8, 1);

            CConnectionState = 1;
        }

        protected override void OnDisconnected()
        {
            CConnectionState = 0;

            Console.WriteLine("Failed to login! Please try again in a moment.");
        }

        protected override void OnConnectionFailed(Byte reason)
        {
            Console.WriteLine("OnConnectionFailed: code: {0}", reason);
        }
    }
}
