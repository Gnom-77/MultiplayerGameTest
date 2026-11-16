namespace _MultiplayerGameTest._Scripts.Core.Networking
{
    
    /// <summary>
    /// Parses command line arguments:
    /// -transport utp|steam   -address 192.168.0.5   -port 1234
    /// </summary>
    public class LaunchOptionsParser
    {
        public static LaunchOptions Parse(string[] args, NetworkSettings defaults)
        {
            var transport = defaults.DefaultTransport;
            var address = defaults.DefaultAdress;
            var port = defaults.DefaultPort;

            for (var i = 0; i < args.Length - 1; i++)
            {
                var value = args[i + 1];
                switch (args[i].ToLowerInvariant())
                {
                    case "-transport":
                        if (TryParseTransport(value, out var parsed)) transport = parsed;
                        break;
                    case "-address":
                        address = value;
                        break;
                    case "-port":
                        if (int.TryParse(value, out var parsedPort)) port = parsedPort;
                        break;
                }
            }

            return new LaunchOptions(transport, address, port);
        }

        private static bool TryParseTransport(string value, out TransportKind kind)
        {
            switch (value.ToLowerInvariant())
            {
                case "utp":
                case "unity":
                    kind = TransportKind.UnityTransport;
                    return true;
                case "steam":
                    kind = TransportKind.Steam;
                    return true;
                default:
                    kind = default;
                    return false;
            }
        }
    }
}