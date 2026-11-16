namespace _MultiplayerGameTest._Scripts.Core.Networking
{
    /// <summary>
    /// Transport-agnostic description of a host to connect to.
    /// For Unity Transport: Address is an IP. For Steam: Address is a SteamID (decimal string).
    /// </summary>
    public readonly struct ConnectionTarget
    {
        public string Address { get; }
        public int Port { get; }

        public ConnectionTarget(string address, int port)
        {
            Address = address;
            Port = port;
        }

        public override string ToString() => $"{Address}:{Port}";
    }
}