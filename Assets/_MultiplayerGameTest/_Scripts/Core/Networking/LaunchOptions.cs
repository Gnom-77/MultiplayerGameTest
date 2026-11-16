namespace _MultiplayerGameTest._Scripts.Core.Networking
{
    public class LaunchOptions
    {
        public TransportKind Transport { get; }
        public string Address { get; }
        public int Port { get; }
        
        public LaunchOptions(TransportKind transport, string address, int port)
        {
            Transport = transport;
            Address = address;
            Port = port;
        }

        public override string ToString() => $"{Transport} {Address}:{Port}";
    }
}