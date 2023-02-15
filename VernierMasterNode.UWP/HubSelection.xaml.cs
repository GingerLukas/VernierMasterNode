using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VernierMasterNode.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HubSelection : Page, IDisposable
    {
        public delegate void HubSelectedHandler(Client client);

        public event HubSelectedHandler HubSelected;
        
        private readonly Thread _discoveryThread;
        private readonly UdpClient _udp;
        private readonly Dictionary<string, DateTime> _discoveredHubs = new Dictionary<string, DateTime>();

        private Client? _currentClient;
        
        
        public HubSelection()
        {
            this.InitializeComponent();
            _discoveryThread = new Thread(DiscoveryLoop);

            _udp = new UdpClient();
            _udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _udp.ExclusiveAddressUse = false;
                
            _udp.Client.MulticastLoopback = true;
            _udp.MulticastLoopback = true;
            _udp.Client.Bind(new IPEndPoint(IPAddress.Any, 2442));
            _udp.JoinMulticastGroup(IPAddress.Parse("239.244.244.224"));

            _discoveryThread.Start();
            
            HubListBox.Children.Add(new HubSelectionItem("192.168.88.20"));
            HubListBox.Children.Add(new HubSelectionItem("10.10.5.3"));
        }

        private void DiscoveryLoop()
        {
            while (true)
            {
                IPEndPoint sender = new IPEndPoint(0, 2442);
                byte[] buffer = _udp.Receive(ref sender);
                lock (_discoveredHubs)
                {
                    _discoveredHubs[sender.Address.ToString()] = DateTime.Now;
                }
            }
        }

        public void Dispose()
        {
            _discoveryThread?.Abort();
        }

        private void ContinueButton_OnClick(object sender, RoutedEventArgs e)
        {
            /*
            if (_currentClient == null)
            {
                //TODO: handle no hub selected
                return;
            }
            */

            HubSelected?.Invoke(_currentClient);
        }
    }
}
