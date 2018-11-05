//#define USE_BROADCAST

using System;
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

            TVCommandsList.ItemsSource = HisenseKey.AllKeys;

            HisenseTV.TVDiscovered += HisenseTV_TVDiscovered;
            HisenseTV.DiscoverTVs();
        }

        private void HisenseTV_TVDiscovered(object sender, HisenseTV e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                TVList.ItemsSource = null;
                tvList.Add(e);
                TVList.ItemsSource = tvList;
            }));
        }

        private void ScanTVButton_Click(object sender, RoutedEventArgs e)
        {
            tvList.Clear();
            HisenseTV.DiscoverTVs();
        }


        private void TVList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            SendCommandTVButton.IsEnabled = (TVList.SelectedItem != null && TVCommandsList.SelectedItem != null);
        }

        private async void SendCommandTVButton_Click(object sender, RoutedEventArgs e)
        {
            await (TVList.SelectedItem as HisenseTV).SendKeyAsync((TVCommandsList.SelectedItem as HisenseKey).Command);
        }
    }
}
