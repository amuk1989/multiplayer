using Unity.Networking.Transport;

namespace Unity.NetCode
{
    public class Bootstrap: ClientServerBootstrap
    {

        public override bool Initialize(string defaultWorldName)
        {
            DefaultConnectAddress = NetworkEndPoint.Parse("127.0.0.1", 7979);
            DefaultListenAddress = NetworkEndPoint.AnyIpv4;
            AutoConnectPort = 7979;
// #if !UNITY_EDITOR
            NetworkStreamReceiveSystem.s_DriverConstructor = new CustomDriverConstructor();
// #endif
            return base.Initialize(defaultWorldName);
        }

    }
}
