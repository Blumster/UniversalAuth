using System;

using UniversalAuth.Encryption;
using UniversalAuth.Network.Packet.Client;

namespace Test
{
    class Program
    {
        static void Main()
        {
            CryptEngine.Initialize(new Byte[] { 00 });
            LoginPacket.InitializeCryptoService(new Byte[] { 00, 00, 00, 00, 00, 00, 00, 00 });
            
            /* var server = new AAAuthServer();
            server.Start(IPAddress.Any, 2106);*/

            /*var client = new AAAuthClient("admin", "test123");
            client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2106));
            
            while (true)
            {
                client.Process();
                System.Threading.Thread.Sleep(1500);
            }*/


            /*var server = new TRAuthServer();
            server.Start(IPAddress.Any, 2106);*/

            /*var client = new TRAuthClient("admin", "test123");
            client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2106));
            
            while (true)
            {
                client.Process();
                System.Threading.Thread.Sleep(1500);
            }*/

            Console.ReadLine();
        }
    }
}
