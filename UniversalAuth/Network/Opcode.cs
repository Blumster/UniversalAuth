namespace UniversalAuth.Network
{
    public enum ClientOpcode : byte
    {
        Login                     = 0,
        AboutToPlay               = 2,
        Logout                    = 3,
        ServerListExt             = 5,
        SCCheck                   = 6
    }

    public enum ServerOpcode : byte
    {
        ProtocolVersion           = 0,
        LoginFail                 = 1,
        BlockedAccount            = 2,
        LoginOk                   = 3,
        SendServerListExt         = 4,
        SendServerListFail        = 5,
        PlayFail                  = 6,
        PlayOk                    = 7,
        AccountKicked             = 8,
        BlockedAccountWithMessage = 9,
        SCCheckReq                = 10,
    }
}
