using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace UniversalAuth.Network.Packet.Client
{
    public class LoginPacket : IPacket
    {
        private static DESCryptoServiceProvider _cryptoService;

        public String UserName { get; set; }
        public String Password { get; set; }
        public UInt32 Subscription { get; set; }
        public UInt16 CDKey { get; set; }

        public static void InitializeCryptoService(Byte[] key)
        {
            var temp = new Byte[key.Length];
            Array.Copy(key, temp, key.Length);

            _cryptoService = new DESCryptoServiceProvider
            {
                KeySize = 64,
                Key = temp,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.None
            };
        }

        public void Unserialize(BinaryReader reader)
        {
            var buff = reader.ReadBytes(30);

            using (var decryptor = _cryptoService.CreateDecryptor())
            {
                var decbuff = decryptor.TransformFinalBlock(buff, 0, 24);

                Array.Copy(decbuff, 0, buff, 0, 24);
            }

            UserName = Encoding.UTF8.GetString(buff, 0, FirstZeroIndex(buff, 0, 14));
            Password = Encoding.UTF8.GetString(buff, 14, FirstZeroIndex(buff, 14, 16));
            Subscription = reader.ReadUInt32();
            CDKey = reader.ReadUInt16();
        }

        public void Serialize(BinaryWriter writer)
        {
            var data = new Byte[30];
            var unBuf = Encoding.UTF8.GetBytes(UserName);
            var pwBuf = Encoding.UTF8.GetBytes(Password);

            Array.Copy(unBuf, 0, data, 0, UserName.Length >= 14 ? 14 : UserName.Length);
            Array.Copy(pwBuf, 0, data, 14, Password.Length >= 16 ? 16 : Password.Length);

            using (var encryptor = _cryptoService.CreateEncryptor())
            {
                var encBuff = encryptor.TransformFinalBlock(data, 0, 24);

                Array.Copy(encBuff, 0, data, 0, 24);
            }

            writer.Write((Byte) ClientOpcode.Login);
            writer.Write(data);
            writer.Write(Subscription);
            writer.Write(CDKey);
        }

        private static Int32 FirstZeroIndex(Byte[] data, Int32 off, Int32 length)
        {
            for (var i = 0; i < length; ++i)
                if (data[off + i] == 0)
                    return i;

            return length;
        }

        public override String ToString()
        {
            return String.Format("LoginPacket(\"{0}\", \"{1}\", {2}, {3})", UserName, Password, Subscription, CDKey);
        }

        public void Dispose()
        {
            _cryptoService.Dispose();
        }
    }
}
