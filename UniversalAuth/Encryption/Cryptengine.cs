using System;

namespace UniversalAuth.Encryption
{
    public static class CryptEngine
    {
        public static void Initialize(Byte[] key)
        {
            BlowfishCipher.Initialize(key);
        }

        public static Boolean Decrypt(Byte[] data)
        {
            BlowfishCipher.Decrypt(data);
            return VerifyChecksum(data);
        }

        public static Byte[] Encrypt(Byte[] data, ref Int32 size)
        {
            if (data.Length % 8 != 0)
                Array.Resize(ref data, ((data.Length >> 3) + 1) * 8);

            Array.Resize(ref data, data.Length + 8);

            AppendChecksum(data);

            BlowfishCipher.Encrypt(data);

            return data;
        }

        private static Boolean VerifyChecksum(Byte[] data)
        {
            long chksum = 0;
            for (var i = 0; i < (data.Length - 4); i += 4)
                chksum ^= BitConverter.ToUInt32(data, i);
            return 0 == chksum;
        }

        private static void AppendChecksum(Byte[] data)
        {
            var chksum = 0U;
            var count = data.Length - 8;
            int i;
            for (i = 0; i < count; i += 4)
                chksum ^= BitConverter.ToUInt32(data, i);
            Array.Copy(BitConverter.GetBytes(chksum), 0, data, i, 4);
        }
    }
}
