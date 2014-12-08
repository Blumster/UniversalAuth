using System;
using System.Collections.Generic;
using System.Net;

using UniversalAuth.Data;
using UniversalAuth.Server;

namespace Test
{
    public class AAAuthServer : AuthServer
    {
        public override Boolean ValidateServer(Byte serverId)
        {
            return serverId == 1 || serverId == 2;
        }

        public override Boolean ValidateLogin(String user, String password, UInt32 subscription, UInt16 cdkey)
        {
            return user == "admin" && password == "test123";
        }

        public override Boolean GetServerInfos(out List<ServerInfoEx> servers)
        {
            var list = new List<ServerInfoEx>
            {
                new ServerInfoEx
                {
                    ServerId = 1,
                    AgeLimit = 18,
                    CurrentPlayers = 0,
                    Ip = IPAddress.Parse("127.0.0.1"),
                    MaxPlayers = 100,
                    PKFlag = 0,
                    Port = 26880,
                    Status = 1
                },
                new ServerInfoEx
                {
                    ServerId = 2,
                    AgeLimit = 18,
                    CurrentPlayers = 0,
                    Ip = IPAddress.Parse("127.0.0.1"),
                    MaxPlayers = 100,
                    PKFlag = 0,
                    Port = 26880,
                    Status = 1
                }
            };

            servers = list;
            return true;
        }
    }
}
