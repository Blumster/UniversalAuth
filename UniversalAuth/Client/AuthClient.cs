using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UniversalAuth.Network.Packet.Client;
using UniversalAuth.Network.Packet.Server;

namespace UniversalAuth.Client
{
    using Data;
    using Network;
    using Network.Packet;

    public abstract class AuthClient
    {
        public ConnectionState ConnectionState { get; set; }
        public IPEndPoint ServerAddress { get; private set; }
        public LengthedSocket Socket { get; private set; }
        public UInt32 OneTimeKey { get; set; }
        public UInt32 ProtocolVersion { get; set; }
        public Boolean EncryptOn { get { return _encryptOn; } set { _encryptOn = value; if (Socket != null) Socket.SetNeedDecrypt(_encryptOn); } }
        public Queue<IPacket> MessageQueue { get; set; }

        private Boolean _encryptOn;

        private readonly Queue<Byte[]> _inQueue = new Queue<Byte[]>(); 
        private readonly Byte[] _internalBuffer = new Byte[2048];
        private readonly Dictionary<Byte, Action<BinaryReader>> _handlers = new Dictionary<Byte, Action<BinaryReader>>();

        protected AuthClient()
        {
            ConnectionState = ConnectionState.Disconnected;
            Socket = null;
            EncryptOn = false;
            MessageQueue = new Queue<IPacket>();

            RegisterHandler(ServerOpcode.AccountKicked, MsgAccountKicked);
            RegisterHandler(ServerOpcode.BlockedAccount, MsgBlockedAccount);
            RegisterHandler(ServerOpcode.BlockedAccountWithMessage, MsgBlockedAccountWithMessage);
            RegisterHandler(ServerOpcode.LoginFail, MsgLoginFail);
            RegisterHandler(ServerOpcode.LoginOk, MsgLoginOk);
            RegisterHandler(ServerOpcode.PlayFail, MsgPlayFail);
            RegisterHandler(ServerOpcode.PlayOk, MsgPlayOk);
            RegisterHandler(ServerOpcode.ProtocolVersion, MsgProtocolVersion);
            RegisterHandler(ServerOpcode.SendServerListFail, MsgSendServerListFail);
            RegisterHandler(ServerOpcode.SendServerListExt, MsgSendServerListExt);
            RegisterHandler(ServerOpcode.SCCheckReq, MsgSCCheckReq);
        }

        ~AuthClient()
        {
            if (Socket != null)
                Socket.Close();
        }

        private void RouteMessageData(Byte opc, BinaryReader reader)
        {
            if (_handlers.ContainsKey(opc))
                _handlers[opc](reader);
            else
                Console.WriteLine("Unregistered packet! Type: {0}", opc);
        }

        private void RegisterHandler(ServerOpcode opc, Action<BinaryReader> action)
        {
            _handlers.Add((Byte)opc, action);
        }

        private void MsgProtocolVersion(BinaryReader reader)
        {
            var msg = new ProtocolVersionPacket();
            msg.Unserialize(reader);

            OneTimeKey = msg.OneTimeKey;
            ProtocolVersion = msg.ProtocolVersion;

            EncryptOn = true;

            ConnectionState = ConnectionState.Connected;
            OnConnected(ServerAddress);
        }

        protected abstract void MsgAccountKicked(BinaryReader reader);
        protected abstract void MsgBlockedAccount(BinaryReader reader);
        protected abstract void MsgBlockedAccountWithMessage(BinaryReader reader);
        protected abstract void MsgLoginFail(BinaryReader reader);
        protected abstract void MsgLoginOk(BinaryReader reader);
        protected abstract void MsgPlayFail(BinaryReader reader);
        protected abstract void MsgPlayOk(BinaryReader reader);
        protected abstract void MsgSendServerListFail(BinaryReader reader);
        protected abstract void MsgSendServerListExt(BinaryReader reader);
        protected abstract void MsgSCCheckReq(BinaryReader reader);

        protected abstract void OnConnected(IPEndPoint severAddress);
        protected abstract void OnDisconnected();
        protected abstract void OnConnectionFailed(Byte reason);

        protected void SendSCCheck(UInt32 userId, UInt32 cardValue)
        {
            lock (MessageQueue)
            {
                MessageQueue.Enqueue(new SCCheckPacket
                {
                    UserId = userId,
                    CardValue = cardValue
                });
            }
        }

        protected void SendServerListEx(UInt64 sessionId)
        {
            var sId = BitConverter.GetBytes(sessionId);

            lock (MessageQueue)
            {
                MessageQueue.Enqueue(new ServerListExtPacket
                {
                    ListKind = 1,
                    SessionId1 = BitConverter.ToUInt32(sId, 0),
                    SessionId2 = BitConverter.ToUInt32(sId, 4)
                });
            }
        }

        protected void SendAboutToPlay(UInt64 sessionId, Byte serverId)
        {
            var sId = BitConverter.GetBytes(sessionId);

            lock (MessageQueue)
            {
                MessageQueue.Enqueue(new AboutToPlayPacket
                {
                    ServerId = serverId,
                    SessionId1 = BitConverter.ToUInt32(sId, 0),
                    SessionId2 = BitConverter.ToUInt32(sId, 4)
                });
            }
        }

        protected void SendLogout(UInt64 sessionId)
        {
            var sId = BitConverter.GetBytes(sessionId);

            lock (MessageQueue)
            {
                MessageQueue.Enqueue(new LogoutPacket
                {
                    SessionId1 = BitConverter.ToUInt32(sId, 0),
                    SessionId2 = BitConverter.ToUInt32(sId, 4)
                });
            }
        }

        protected void SendLogin(String userName, String password, UInt32 subscription, UInt16 cdkey)
        {
            lock (MessageQueue)
            {
                MessageQueue.Enqueue(new LoginPacket
                {
                    UserName = userName,
                    Password = password,
                    Subscription = subscription,
                    CDKey = cdkey
                });
            }
        }

        public void Connect(IPEndPoint server)
        {
            if (ConnectionState != ConnectionState.Disconnected)
                return;

            ServerAddress = server;

            ConnectionState = ConnectionState.Connecting;

            Socket = new LengthedSocket(LengthedSocket.SizeType.Word);
            Socket.BeginConnect(ServerAddress, EndConnect, null);
        }

        public void Disconnect()
        {
            if (ConnectionState != ConnectionState.Disconnected)
            {
                Socket.Close();
                Socket = null;

                OnDisconnected();
            }

            ConnectionState = ConnectionState.Disconnected;
        }

        public void Process()
        {
            if (ConnectionState == ConnectionState.Disconnected)
                return;

            lock (_inQueue)
                while (_inQueue.Count > 0)
                    ProcessPacket(_inQueue.Dequeue());

            lock (MessageQueue)
            {
                while (MessageQueue.Count > 0)
                {
                    using (var writer = new BinaryWriter(new MemoryStream()))
                    {
                        var msg = MessageQueue.Dequeue();

                        msg.Serialize(writer);

                        var arr = ((MemoryStream) writer.BaseStream).ToArray();
                        
                        Socket.BeginSend(arr, 0, arr.Length, EndSend, EncryptOn);
                    }
                }
            }
        }

        private void ProcessPacket(Byte[] data)
        {
            RouteMessageData(data[0], new BinaryReader(new MemoryStream(data, 1, data.Length - 1)));
        }

        private void EndReceive(IAsyncResult result)
        {
            var buffer = Socket.EndReceive(result);
            if (buffer == null)
                return;

            lock (_inQueue)
                _inQueue.Enqueue(buffer);

            Socket.BeginReceive(_internalBuffer, 0, _internalBuffer.Length, EndReceive);
        }

        private void EndSend(IAsyncResult result)
        {
            Socket.EndSend(result);
        }

        private void EndConnect(IAsyncResult result)
        {
            Socket.EndConnect(result);

            if (!Socket.Connected)
            {
                ConnectionState = ConnectionState.Disconnected;
                OnConnectionFailed(0);
                return;
            }

            Socket.BeginReceive(_internalBuffer, 0, _internalBuffer.Length, EndReceive);
        }
    }
}
