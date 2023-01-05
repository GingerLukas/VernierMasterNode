using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class VernierClient : UserControl
    {
        private Client _client;
        private BindingList<string> _espDevices = new BindingList<string>();
        public VernierClient()
        {
            this.InitializeComponent();
            _client = new Client();
            _client.OnEspDeviceConnected += ClientOnOnEspDeviceConnected;
            _client.OnEspDeviceDisconnected += ClientOnOnEspDeviceDisconnected;
            _client.OnDeviceFound += ClientOnOnDeviceFound;
        }

        private void ClientOnOnDeviceFound(string uid, ulong serialid)
        {
        }

        private void ClientOnOnEspDeviceDisconnected(string uid)
        {
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                TextBlock? textBlock = EspDevicesStackPanel.Children.OfType<TextBlock>()
                    .FirstOrDefault(x => x.Name == uid);
                if (textBlock == null)
                {
                    return;
                }

                EspDevicesStackPanel.Children.Remove(textBlock);
            });
        }

        private void ClientOnOnEspDeviceConnected(string uid)
        {
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                TextBlock text = new TextBlock() { Name = uid,Text = $"ESP: {uid}" };
                EspDevicesStackPanel.Children.Add(text);
            });
        }
    }
}
