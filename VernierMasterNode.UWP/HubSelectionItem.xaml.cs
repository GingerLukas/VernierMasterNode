using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace VernierMasterNode.UWP
{
    public sealed partial class HubSelectionItem : UserControl
    {
        public Client? Client { get; set; }

        public HubSelectionItem(string ipAddress)
        {
            this.InitializeComponent();
            IpAddress.Text = ipAddress;
            HorizontalAlignment = HorizontalAlignment.Stretch;
        }

        private void ConnectButton_OnClick(object sender, RoutedEventArgs e)
        {
            Task.Run(TryToConnect);
        }

        private async void TryToConnect()
        {
            Client client;
            ProgressRing ring = null;
            object prev = null;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                prev = ConnectButton.Content;
                ring = new ProgressRing();
                //ring.Foreground = new SolidColorBrush((Color)Resources.ThemeDictionaries["SystemAccentColorDark3"]);
                ConnectButton.Content = ring;
            });
            try
            {
                string ip = "";
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    ip = IpAddress.Text;
                    ring.IsActive = true;
                });
                client = new Client(ip);
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { StatusIcon.Symbol = Symbol.Accept; });
            }
            catch (Exception e)
            {
                client = null;
            }

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ring.IsActive = false;
                ConnectButton.Content = prev;
            });

            Client = client;
        }
    }
}