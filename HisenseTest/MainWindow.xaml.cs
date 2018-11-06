using System;
using System.Windows;
using System.Collections.Generic;

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
            TVMacrosList.ItemsSource = HisenseKeyMacro.AllMacros;

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
            SendMacroTVButton.IsEnabled = (TVList.SelectedItem != null && TVMacrosList.SelectedItem != null);
        }

        private async void SendCommandTVButton_Click(object sender, RoutedEventArgs e)
        {
            await (TVList.SelectedItem as HisenseTV).SendKeyAsync((TVCommandsList.SelectedItem as HisenseKey).Command);
        }

        private async void SendMacroTVButton_Click(object sender, RoutedEventArgs e)
        {
            await (TVList.SelectedItem as HisenseTV).SendMacroAsync((HisenseKeyMacro)TVMacrosList.SelectedItem);
        }
    }
}
