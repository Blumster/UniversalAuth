using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace UniversalAuth.Network
{
    using Encryption;

    public class LengthedSocket
    {
        private readonly Socket _socket;
        private readonly SizeType _sizeHeaderLen;
        private Boolean _closed;
        private Boolean _needDecrypt;

        public LengthedSocket(SizeType sizeHeaderLen)
        {
            _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            _sizeHeaderLen = sizeHeaderLen;
        }

        private LengthedSocket(Socket s, SizeType sizeHeaderLen)
        {
            _socket = s;
            _sizeHeaderLen = sizeHeaderLen;
        }

        public Boolean Connected { get { return _socket.Connected; } }

        public void Bind(EndPoint ep)
        {
            _socket.Bind(ep);
        }

        public void Listen(Int32 backlog)
        {
            _socket.Listen(backlog);
        }

        public IAsyncResult BeginAccept(AsyncCallback callback)
        {
            return _closed ? null : _socket.BeginAccept(callback, this);
        }

        public LengthedSocket EndAccept(IAsyncResult result)
        {
            return _closed || result == null ? null : new LengthedSocket(_socket.EndAccept(result), _sizeHeaderLen);
        }

        public IAsyncResult BeginConnect(EndPoint remoteEp, AsyncCallback callback, Object state)
        {
            return _closed ? null : _socket.BeginConnect(remoteEp, callback, state);
        }

        public void EndConnect(IAsyncResult result)
        {
            if (!_closed && result != null)
                _socket.EndConnect(result);
        }

        public IAsyncResult BeginReceive(Byte[] buffer, Int32 offset, Int32 size, AsyncCallback callback)
        {
            if (_closed)
                return null;

            try
            {
                return _socket.BeginReceive(buffer, offset, size, SocketFlags.None, callback, buffer);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknow error at LengthedSocket::BeginReceive! Exception: {0}", e);
            }
            return null;
        }

        public Byte[] EndReceive(IAsyncResult result)
        {
            if (_closed)
                return null;

            Int32 i;
            try
            {
                i = _socket.EndReceive(result);
                if (i <= (Int32) _sizeHeaderLen)
                    return null;
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown error at LengthedSocket::EndReceive! Exception: {0}", e);
                return null;
            }


            var buffer = (Byte[]) result.AsyncState;
            var len = 0UL;

            switch (_sizeHeaderLen)
            {
                case SizeType.None:
                    len = (UInt64) i;
                    break;

                case SizeType.Char:
                    len = buffer[0];
                    break;

                case SizeType.Word:
                    len = BitConverter.ToUInt16(buffer, 0);
                    break;

                case SizeType.Dword:
                    len = BitConverter.ToUInt32(buffer, 0);
                    break;

                case SizeType.Qword:
                    len = BitConverter.ToUInt64(buffer, 0);
                    break;
            }

            var buff = new Byte[len - (Byte) _sizeHeaderLen];
            if (buff.Length == 0)
                return null;

            Array.Copy(buffer, (Byte) _sizeHeaderLen, buff, 0, buff.Length);

            if (_needDecrypt)
                CryptEngine.Decrypt(buff);

            return buff;
        }

        public IAsyncResult BeginSend(Byte[] buffer, Int32 offset, Int32 size, AsyncCallback callback, Boolean needEncryption)
        {
            if (_closed)
                return null;

            buffer = needEncryption ? CryptEngine.Encrypt(buffer, ref size) : buffer;

            var buff = new List<Byte>(buffer.Length + (Int32) _sizeHeaderLen);

            switch (_sizeHeaderLen)
            {
                case SizeType.None:
                    break;

                case SizeType.Char:
                    buff.Add((Byte) (buffer.Length + 1));
                    break;

                case SizeType.Word:
                    buff.AddRange(BitConverter.GetBytes((UInt16) (buffer.Length + 2U)));
                    break;

                case SizeType.Dword:
                    buff.AddRange(BitConverter.GetBytes((UInt32) (buffer.Length + 4U)));
                    break;

                case SizeType.Qword:
                    buff.AddRange(BitConverter.GetBytes((UInt64) (buffer.Length + 8U)));
                    break;
            }


            buff.AddRange(buffer);

            return _socket.BeginSend(buff.ToArray(), 0, buff.Count, SocketFlags.None, callback, null);
        }

        public Int32 EndSend(IAsyncResult result)
        {
            return _closed || result == null ? -1 : _socket.EndSend(result);
        }

        public void Close()
        {
            _closed = true;
            _socket.Close();
        }

        public void SetNeedDecrypt(Boolean need)
        {
            _needDecrypt = need;
        }

        public IPAddress RemoteAddress
        {
            get { return ((IPEndPoint) _socket.RemoteEndPoint).Address; }
        }

        public enum SizeType : byte
        {
            None  = 0,
            Char  = 1,
            Word  = 2,
            Dword = 4,
            Qword = 8
        }
    }
}
