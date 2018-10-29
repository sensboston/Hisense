﻿//#define USE_BROADCAST

using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.NetworkInformation;

namespace HisenseTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<HisenseTV> tvList { get; set; } = new List<HisenseTV>();

        public MainWindow()
        {
            InitializeComponent();

            ScanTVButton_Click(this, null);
        }

#if USE_BROADCAST
        private void ScanTVButton_Click(object sender, RoutedEventArgs e)
        {
            tvList.Clear();
            TVList.ItemsSource = null;
            try
            {
                using (Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                    s.SendTimeout = s.ReceiveTimeout = 500;
                    s.Bind(new IPEndPoint(IPAddress.Any, 54321));
                    s.SendTo(Encoding.ASCII.GetBytes("DISCOVERY#"), new IPEndPoint(IPAddress.Broadcast, 50000));
                    byte[] buf = new byte[1024];
                    int bufSize = s.Receive(buf);
                    var receivedString = Encoding.ASCII.GetString(buf, 0, bufSize);
                    var foundTVs = receivedString.Split(new string[] { "DISCOVERYACK#" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var tv in foundTVs)
                        tvList.Add(new HisenseTV(tv));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                TVList.ItemsSource = tvList;
            }
        }
#else
        private async void ScanTVButton_Click(object sender, RoutedEventArgs e)
        {
            var localIP = LocalIPAddress?.GetAddressBytes();
            if (localIP.Length == 4)
            {
                tvList.Clear();
                TVList.ItemsSource = null;
                for (byte i = 2; i < 255; i++)
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            using (Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                            {
                                s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                                s.SendTimeout = s.ReceiveTimeout = 500;
                                s.Bind(new IPEndPoint(IPAddress.Any, 54000 + i));
                                s.SendTo(Encoding.ASCII.GetBytes("DISCOVERY#"), new IPEndPoint(new IPAddress(new byte[] { localIP[0], localIP[1], localIP[2], i }), 50000));
                                byte[] buf = new byte[1024];
                                int bufSize = s.Receive(buf);
                                var receivedString = Encoding.ASCII.GetString(buf, 0, bufSize);
                                var foundTVs = receivedString.Split(new string[] { "DISCOVERYACK#" }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var tv in foundTVs)
                                    tvList.Add(new HisenseTV(tv));

                            // Assigning ItemSource should run in UI thread context
                            Dispatcher.Invoke(new Action(() => { TVList.ItemsSource = tvList; }));
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    });
                }
            }
        }
#endif

        /// <summary>
        /// This routine kinda tricky and should be refactored
        /// </summary>
        private IPAddress LocalIPAddress
        {
            get
            {
                IPAddress ipAddress = null;
                foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
                    if (item.OperationalStatus == OperationalStatus.Up && item.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                        foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                                ipAddress = ip.Address;
                return ipAddress;
            }
        }
    }
}
