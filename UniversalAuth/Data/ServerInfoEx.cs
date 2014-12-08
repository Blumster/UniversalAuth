using System;
using System.Net;

namespace UniversalAuth.Data
{
    public class ServerInfoEx
    {
        public Byte ServerId { get; set; }
        public IPAddress Ip { get; set; }
        public UInt32 Port { get; set; }
        public Byte AgeLimit { get; set; }
        public Byte PKFlag { get; set; }
        public UInt16 CurrentPlayers { get; set; }
        public UInt16 MaxPlayers { get; set; }
        public Byte Status { get; set; }
    }
}
