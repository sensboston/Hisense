using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hisense
{
    public enum PlatformOS
    {
        Android = 0,
        Linux
    }

    public class HisenseTV
    {
        public bool IsActive { get; set; } = false;
        private string VersionString { get; set; }
        public IPAddress IP { get; private set; }
        public string DeviceName { get; private set; }
        public string VersionID { get; private set; }
        public Dictionary<string, int> SourceDesp { get; private set; }
        public int InputMethod { get; private set; }
        public int TVConvergence { get; private set; }
        public bool UDPChannelSurport { get; private set; }
        public PlatformOS Platform { get; private set; }

        /// <summary>
        /// This constructor creates HisenseTV object by connecting to the specific IP address
        /// and parsing actual response string
        /// </summary>
        /// <param name="ip"></param>
        public HisenseTV(IPAddress ip)
        {
            try
            {
                using (Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    s.SendTimeout = s.ReceiveTimeout = 500;
                    s.Bind(new IPEndPoint(IPAddress.Any, 55000));
                    s.SendTo(Encoding.ASCII.GetBytes("DISCOVERY#"), new IPEndPoint(ip, 50000));
                    byte[] buf = new byte[1024];
                    int bufSize = s.Receive(buf);
                    var discoveryString = Encoding.ASCII.GetString(buf, 0, bufSize);
                    ParseDiscoveryString(discoveryString);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// This constructor creates HisenseTV object by parsing string
        /// </summary>
        /// <param name="discoveryString"></param>
        private HisenseTV(string discoveryString)
        {
            ParseDiscoveryString(discoveryString);
        }

        /// <summary>
        /// This function parses Hisense TVs response like below:
        /// 
        /// VERSION102#192.168.1.33#SmartTV08#
        /// ip = 192.168.1.33
        /// devicename = Smart TV
        /// versionid = VERSION300
        /// sourcedesp = ATV:0; DTV: 1; VIDEO1: 2; VIDEO2: 3; YPbPr: 4; HDMI1: 5; HDMI2: 6; HDMI3: 7; VGA: 8; MC: 10
        /// inputmethod = 1
        /// tvconvergence = 0
        /// udpchannelsurport = 1
        /// platform = 1  #0-android 1-linux
        /// 
        /// </summary>
        /// <param name="discoveryString"></param>
        private void ParseDiscoveryString(string discoveryString)
        {
            var props = discoveryString.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (props.Length > 0)
            {
                IsActive = true;

                if (props[0].Contains("VERSION")) VersionString = props[0];

                for (int i = 1; i < props.Length; i++)
                {
                    if (props[i].StartsWith("ip")) IP = IPAddress.Parse(props[i].Substring(props[i].IndexOf("=") + 1).Trim());

                    if (props[i].StartsWith("devicename")) DeviceName = props[i].Substring(props[i].IndexOf("=") + 1).Trim();

                    if (props[i].StartsWith("versionid")) VersionID = props[i].Substring(props[i].IndexOf("=") + 1).Trim();

                    if (props[i].StartsWith("sourcedesp"))
                    {
                        var s = props[i].Substring(props[i].IndexOf("=") + 1).Trim();
                        var subProps = s.Split(';');
                        foreach (var sp in subProps)
                        {
                            var val = sp.Split('=');
                            if (val.Length == 2 && int.TryParse(val[1], out int n))
                                SourceDesp[val[0]] = n;
                        }
                    }
                    if (props[i].StartsWith("inputmethod"))
                        if (int.TryParse(props[i].Substring(props[i].IndexOf("=") + 1).Trim(), out int inputMethod))
                            InputMethod = inputMethod;

                    if (props[i].StartsWith("tvconvergence"))
                        if (int.TryParse(props[i].Substring(props[i].IndexOf("=") + 1).Trim(), out int tvConvergence))
                            TVConvergence = tvConvergence;

                    if (props[i].StartsWith("udpchannelsurport"))
                        if (int.TryParse(props[i].Substring(props[i].IndexOf("=") + 1).Trim(), out int udpChannelSurport))
                            UDPChannelSurport = (udpChannelSurport == 1);

                    if (props[i].StartsWith("platform"))
                        if (int.TryParse(props[i].Substring(props[i].IndexOf("=") + 1, 3).Trim(), out int platform))
                            Platform = (platform == 1) ? PlatformOS.Linux : PlatformOS.Android;
                }
            }
        }

        public static event EventHandler<HisenseTV> TVDiscovered;

        private static CancellationTokenSource cts;

        private static List<HisenseTV> tvList = new List<HisenseTV>();

        private static IPAddress GetLocalIP()
        {
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
                if (item.OperationalStatus == OperationalStatus.Up && item.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork && ip.Address.ToString().StartsWith("192.168"))
                            return ip.Address;
            return null;
        }

        public static async void DiscoverTVs()
        {
            // Cancel running task (if any)
            cts?.Cancel();

            // Clear our local list of discovered TVs
            tvList.Clear();

            // First, try broadcast packet (sometimes it's not working
            try
            {
                using (Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                    s.SendTimeout = s.ReceiveTimeout = 500;
                    s.Bind(new IPEndPoint(IPAddress.Any, 54321));
                    s.SendTo(Encoding.ASCII.GetBytes("DISCOVERY#"), new IPEndPoint(IPAddress.Broadcast, 50000));
                    byte[] buf = new byte[4096];
                    int bufSize = s.Receive(buf);
                    var receivedString = Encoding.ASCII.GetString(buf, 0, bufSize);
                    var foundTVs = receivedString.Split(new string[] { "DISCOVERYACK#" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var tv in foundTVs)
                    {
                        var hisenseTV = new HisenseTV(tv);
                        tvList.Add(hisenseTV);
                        TVDiscovered?.Invoke(null, hisenseTV);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            // If no TVs found via broadcast packet, let's loop throw local network IP range
            if (tvList.Count == 0)
            {
                var ipAddress = GetLocalIP();
                if (ipAddress != null)
                {
                    cts = new CancellationTokenSource();
                    var localIP = ipAddress?.GetAddressBytes();
                    if (localIP.Length == 4)
                    {
                        for (byte i = 1; i < 255; i++)
                        {
                            await Task.Factory.StartNew(() =>
                            {
                                var tvIP = new IPAddress(new byte[] { localIP[0], localIP[1], localIP[2], i });
                                Debug.WriteLine($"Attempting to connect to {tvIP.ToString()}");

                                try
                                {
                                    using (Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                                    {
                                        s.SendTimeout = s.ReceiveTimeout = 200;
                                        s.Bind(new IPEndPoint(IPAddress.Any, 54000 + i));
                                        s.SendTo(Encoding.ASCII.GetBytes("DISCOVERY#"), new IPEndPoint(tvIP, 50000));
                                        byte[] buf = new byte[2048];
                                        int bufSize = s.Receive(buf);
                                        var receivedString = Encoding.ASCII.GetString(buf, 0, bufSize);
                                        TVDiscovered?.Invoke(null, new HisenseTV(receivedString));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine(ex.Message);
                                }
                            }, cts.Token);
                        }
                    }
                }
            }
        }

        public bool SendKey(string key)
        {
            bool result = false;
            if (IsActive)
            {
                try
                {
                    using (Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                    {
                        string id = new Random().Next((ushort.MaxValue), int.MaxValue).ToString("0000000");
                        s.SendTimeout = s.ReceiveTimeout = 500;
                        s.Bind(new IPEndPoint(IPAddress.Any, 54000));
                        s.SendTo(Encoding.ASCII.GetBytes($"CTCREATE\r\nID: {id}\r\n\r\n\0"), new IPEndPoint(IP, 60030));
                        byte[] buf = new byte[256];
                        int bufSize = s.Receive(buf);
                        var received = Encoding.ASCII.GetString(buf, 0, bufSize);
                        if (received.StartsWith("STATUS 500"))
                        {
                            // Parse actual connection port
                            received = received.Substring(received.IndexOf("PORT:") + 5);
                            var portStr = received.Substring(0, received.IndexOf("\r"));
                            if (int.TryParse(portStr, out int port))
                            {
                                s.SendTo(Encoding.ASCII.GetBytes("SUS\0"), new IPEndPoint(IP, port));
                                s.SendTo(Encoding.ASCII.GetBytes($"CTCREATE\r\nMAC: appmac_appmac_app\r\nVERSION: 0001\r\n\r\n\0"), new IPEndPoint(IP, port));
                                bufSize = s.Receive(buf);
                                received = Encoding.ASCII.GetString(buf, 0, bufSize);
                                if (received.StartsWith("STATUS 500"))
                                {
                                    s.SendTo(Encoding.ASCII.GetBytes($"CCCREATE\r\nID: {id}\r\n\r\n\0"), new IPEndPoint(IP, port));
                                    bufSize = s.Receive(buf);
                                    received = Encoding.ASCII.GetString(buf, 0, bufSize);
                                    if (received.StartsWith("STATUS 500"))
                                    {
                                        // Parse actual connection port
                                        received = received.Substring(received.IndexOf("PORT:") + 5);
                                        portStr = received.Substring(0, received.IndexOf("\r"));
                                        if (int.TryParse(portStr, out int cmdPort))
                                        {
                                            s.SendTo(Encoding.ASCII.GetBytes("SUS\0"), new IPEndPoint(IP, cmdPort));
                                            string cmd = $"\r\n1\r\n1HISENSE_DELIMITER2HISENSE_DELIMITER2HISENSE_DELIMITER{key}HISENSE_DELIMITER10HISENSE_DELIMITER0HISENSE_DELIMITER0\r\n\r\n\0";
                                            cmd = "CMD " + (cmd.Length + 10).ToString("000000") + cmd;
                                            s.SendTo(Encoding.ASCII.GetBytes(cmd), new IPEndPoint(IP, cmdPort));
                                            bufSize = s.Receive(buf);
                                            result = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            return result;
        }

        public async Task SendMacroAsync(HisenseKeyMacro macro)
        {
            if (IsActive)
            {
                foreach (var cmd in macro.Commands)
                {
                    // Try to re-send key twice (if first time fails)
                    if (!SendKey(cmd.Key))
                    {
                        await Task.Delay(500);
                        // If SendKey() fails second time, abort the loop
                        if (!SendKey(cmd.Key))
                            break;
                    }
                    await Task.Delay(cmd.Delay);
                }
            }
        }
    }
}
