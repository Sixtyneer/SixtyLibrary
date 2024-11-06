using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace SixtyLibrary
{
    public static class NetworkUtilities
    {
        public static List<(string AdapterName, bool IsConnected)> GetNetworkAdapters()
        {
            var adapters = new List<(string AdapterName, bool IsConnected)>();
            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                bool isConnected = adapter.OperationalStatus == OperationalStatus.Up;
                adapters.Add((adapter.Name, isConnected));
            }
            return adapters;
        }
        public static Dictionary<string, IPv4InterfaceStatistics> GetNetworkInterfaceStatistics()
        {
            var stats = new Dictionary<string, IPv4InterfaceStatistics>();

            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (adapter.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    var interfaceStats = adapter.GetIPv4Statistics();
                    stats[adapter.Name] = interfaceStats;
                }
            }
            return stats;
        }
        public static List<string> GetMacAddresses()
        {
            var macAddresses = new List<string>();

            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                string macAddress = adapter.GetPhysicalAddress().ToString();
                if (!string.IsNullOrEmpty(macAddress))
                {
                    macAddresses.Add(macAddress);
                }
            }
            return macAddresses;
        }
        public static List<string> GetLocalIPAddresses()
        {
            var ipAddresses = new List<string>();

            foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (var ip in adapter.GetIPProperties().UnicastAddresses)
                {
                    ipAddresses.Add(ip.Address.ToString());
                }
            }
            return ipAddresses;
        }
        public static async Task<string> GetPublicIPAddress()
        {
            string[] apiUrls =
            {
                "https://api.ipify.org",           // Option 1
                "https://checkip.amazonaws.com",   // Option 2
                "https://ifconfig.me/ip",          // Option 3
                "https://icanhazip.com"            // Option 4
            };
            using (HttpClient client = new HttpClient())
            {
                foreach (string url in apiUrls)
                {
                    try
                    {
                        // Append "?format=json" or similar if an API requires JSON response format
                        return await client.GetStringAsync(url);
                    }
                    catch
                    {
                        continue; // Try the next API if there's an error (if 4 option not enough, you are in trouble :))
                    }
                }
            }
            return "Unable to retrieve public IP.";
        }
        public static async Task<long?> PingHost(string host)
        {
            using (var ping = new Ping())
            {
                try
                {
                    PingReply reply = await ping.SendPingAsync(host);
                    return reply.Status == IPStatus.Success ? reply.RoundtripTime : null;
                }
                catch
                {
                    return null;
                }
            }
        }
        public static async Task<List<string>> Traceroute(string host, int maxHops = 30)
        {
            var route = new List<string>();
            using (var ping = new Ping())
            {
                for (int ttl = 1; ttl <= maxHops; ttl++)
                {
                    var options = new PingOptions(ttl, true);
                    try
                    {
                        PingReply reply = await ping.SendPingAsync(host, 3000, new byte[32], options);
                        route.Add(reply.Address != null ? reply.Address.ToString() : "Request timed out.");

                        if (reply.Status == IPStatus.Success)
                            break;
                    }
                    catch
                    {
                        route.Add("Error occurred during traceroute.");
                        break;
                    }
                }
            }
            return route;
        }
        public static async Task<bool> CheckInternetConnectivity()
        {
            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            try
            {
                // Attempt to reach Google as a simple test for connectivity
                HttpResponseMessage response = await client.GetAsync("https://www.google.com");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false; // If any exception, assume no internet connection
            }
        }
        public static async Task<string> DnsLookup(string hostOrIp)
        {
            try
            {
                // Check if the input is an IP address for reverse lookup
                if (IPAddress.TryParse(hostOrIp, out IPAddress? ip))
                {
                    IPHostEntry hostEntry = await Dns.GetHostEntryAsync(ip);
                    return $"Host name for {hostOrIp}: {hostEntry.HostName}";
                }
                else // Perform forward lookup
                {
                    IPAddress[] addresses = await Dns.GetHostAddressesAsync(hostOrIp);
                    var addressList = string.Join(", ", addresses.Select(addr => addr.ToString()));
                    return $"IP addresses for {hostOrIp}: {addressList}";
                }
            }
            catch (Exception ex)
            {
                return $"Error in DNS lookup: {ex.Message}";
            }
        }
        public static (IPAddress NetworkAddress, IPAddress BroadcastAddress) CalculateSubnet(IPAddress ipAddress, IPAddress subnetMask)
        {
            byte[] ipBytes = ipAddress.GetAddressBytes();
            byte[] maskBytes = subnetMask.GetAddressBytes();

            if (ipBytes.Length != maskBytes.Length)
                throw new ArgumentException("IP address and subnet mask length do not match.");

            byte[] networkAddressBytes = new byte[ipBytes.Length];
            byte[] broadcastAddressBytes = new byte[ipBytes.Length];

            for (int i = 0; i < ipBytes.Length; i++)
            {
                networkAddressBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
                broadcastAddressBytes[i] = (byte)(ipBytes[i] | ~maskBytes[i]);
            }

            IPAddress networkAddress = new IPAddress(networkAddressBytes);
            IPAddress broadcastAddress = new IPAddress(broadcastAddressBytes);

            return (networkAddress, broadcastAddress);
        }
        public static async Task<List<int>> PortScan(string host, int startPort = 1, int endPort = 1024, int timeout = 200)
        {
            var openPorts = new List<int>();
            var tasks = new List<Task>();

            for (int port = startPort; port <= endPort; port++)
            {
                int currentPort = port;  // To avoid closure issues
                tasks.Add(Task.Run(async () =>
                {
                    using (TcpClient tcpClient = new TcpClient())
                    {
                        try
                        {
                            var connectTask = tcpClient.ConnectAsync(host, currentPort);
                            if (await Task.WhenAny(connectTask, Task.Delay(timeout)) == connectTask && tcpClient.Connected)
                            {
                                openPorts.Add(currentPort);
                            }
                        }
                        catch
                        {
                            // Port is closed or not responding
                        }
                    }
                }));
            }

            await Task.WhenAll(tasks);
            return openPorts;
        }
    }
}
