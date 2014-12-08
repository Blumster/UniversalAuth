using System;
using System.Collections.Generic;
using System.IO;
using UniversalAuth.Network.Packet.Server;

namespace UniversalAuth.Server
{
    using Data;
    using Network;
    using Network.Packet;
    using Network.Packet.Client;

    public class Client
    {
        private readonly Dictionary<Byte, Action<BinaryReader>> _handlers = new Dictionary<Byte, Action<BinaryReader>>();

        public LengthedSocket Socket { get; private set; }
        public AuthServer Server { get; private set; }

        public UInt32 OneTimeKey;
        public UInt32 SessionId1;
        public UInt32 SessionId2;

        private Boolean _disconnect;
        private readonly Byte[] _internalBuffer = new Byte[2048];

        public Client(LengthedSocket socket, AuthServer server)
        {
            Socket = socket;
            Server = server;

            RegisterHandler(ClientOpcode.AboutToPlay, MsgAboutToPlay);
            RegisterHandler(ClientOpcode.Login, MsgLogin);
            RegisterHandler(ClientOpcode.Logout, MsgLogout);
            RegisterHandler(ClientOpcode.SCCheck, MsgSCCheck);
            RegisterHandler(ClientOpcode.ServerListExt, MsgServerListExt);

            Server.GenerateData(out OneTimeKey, out SessionId1, out SessionId2);

            SendProtocolVersion();

            Socket.SetNeedDecrypt(true);
            Socket.BeginReceive(_internalBuffer, 0, 2048, EndReceive);
        }

        private void RouteMessageData(Byte opc, BinaryReader reader)
        {
            if (_handlers.ContainsKey(opc))
                _handlers[opc](reader);
            else
                Console.WriteLine("Unregistered packet! Type: {0}", opc);
        }

        private void RegisterHandler(ClientOpcode opc, Action<BinaryReader> action)
        {
            _handlers.Add((Byte) opc, action);
        }

        private void SendProtocolVersion()
        {
            var s = new ProtocolVersionPacket
            {
                OneTimeKey = OneTimeKey,
                ProtocolVersion = 0
            };

            var data = CreatePacket(s);

            Socket.BeginSend(data, 0, data.Length, EndSend, false);
        }

        private void EndSend(IAsyncResult result)
        {
            if (Socket == null)
                return;

            Socket.EndSend(result);

            TryToDisconnect();
        }

        private void EndReceive(IAsyncResult result)
        {
            if (Socket == null)
                return;

            var buff = Socket.EndReceive(result);
            if (buff != null)
                RouteMessageData(buff[0], new BinaryReader(new MemoryStream(buff, 1, buff.Length - 1)));
            else
                _disconnect = true;

            if (TryToDisconnect())
                return;

            Socket.BeginReceive(_internalBuffer, 0, 2048, EndReceive);
        }

        protected void MsgAboutToPlay(BinaryReader reader)
        {
            var packet = new AboutToPlayPacket();
            packet.Unserialize(reader);

            IPacket s;

            if (Server.ValidateServer(packet.ServerId))
            {
                s = new PlayOkPacket
                {
                    OneTimeKey = OneTimeKey,
                    ServerId = packet.ServerId,
                    UserId = 0
                };
            }
            else
            {
                s = new PlayFailPacket
                {
                    ResultCode = 1
                };

                _disconnect = true;
            }

            var data = CreatePacket(s);

            Socket.BeginSend(data, 0, data.Length, EndSend, true);
        }

        protected void MsgLogin(BinaryReader reader)
        {
            var packet = new LoginPacket();
            packet.Unserialize(reader);

            IPacket s;
            if (Server.ValidateLogin(packet.UserName, packet.Password, packet.Subscription, packet.CDKey))
            {
                s = new LoginOkPacket
                {
                    SessionId1 = SessionId1,
                    SessionId2 = SessionId2
                };
            }
            else
            {
                s = new LoginFailPacket
                {
                    ResultCode = 1
                };

                _disconnect = true;
            }

            var data = CreatePacket(s);

            Socket.BeginSend(data, 0, data.Length, EndSend, true);
        }

        protected void MsgLogout(BinaryReader reader)
        {
            var packet = new LogoutPacket();
            packet.Unserialize(reader);

            _disconnect = true;
        }

        protected void MsgSCCheck(BinaryReader reader)
        {
            var packet = new SCCheckPacket();
            packet.Unserialize(reader);

            // I have no idea what this is
        }

        protected void MsgServerListExt(BinaryReader reader)
        {
            var packet = new ServerListExtPacket();
            packet.Unserialize(reader);

            IPacket s;
            List<ServerInfoEx> servers;

            if (Server.GetServerInfos(out servers))
            {
                s = new SendServerListExtPacket
                {
                    ServerList = servers
                };
            }
            else
            {
                s = new AccountKickedPacket
                {
                    ReasonCode = 1,
                };

                _disconnect = true;
            }

            var data = CreatePacket(s);

            Socket.BeginSend(data, 0, data.Length, EndSend, true);
        }

        private Boolean TryToDisconnect()
        {
            if (_disconnect)
            {
                Socket.Close();
                Socket = null;

                Server.Remove(this);
            }

            return _disconnect;
        }

        private static Byte[] CreatePacket(IPacket packet)
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            packet.Serialize(writer);

            return stream.ToArray();
        }
    }
}
