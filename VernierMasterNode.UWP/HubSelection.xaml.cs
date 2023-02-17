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
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using VernierMasterNode.UWP.Services;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VernierMasterNode.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HubSelection : Page
    {
        public delegate void HubSelectedHandler(Client client);

        public event HubSelectedHandler HubSelected;


        private Dictionary<string, HubSelectionItem> _elements = new Dictionary<string, HubSelectionItem>();


        public HubSelection()
        {
            this.InitializeComponent();

            HubDiscoveryService.HubFound += HubDiscoveryServiceOnHubFound;
            HubDiscoveryService.HubLost += HubDiscoveryServiceOnHubLost;
            HubDiscoveryService.Start();
        }

        private void HubDiscoveryServiceOnHubLost(string ip, DateTime time)
        {
            lock (_elements)
            {
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => { HubListBox.Children.Remove(_elements[ip]); });
            }
        }

        private void HubDiscoveryServiceOnHubFound(string ip, DateTime time)
        {
            lock (_elements)
            {
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => { HubListBox.Children.Add(_elements[ip] = new HubSelectionItem(ip) { Name = ip }); });
            }
        }


        private void ContinueButton_OnClick(object sender, RoutedEventArgs e)
        {
            lock (_elements)
            {
                //TODO: select last hub connected
                HubSelectionItem? item = _elements.Values.FirstOrDefault(x => x.Client != null);
                if (item == null)
                {
#if DEBUG
                    Client client = new Client("localhost");
                    if (client != null)
                    {

                        HubSelected?.Invoke(client);
                    }
#endif
                    //TODO: messagebox to show the error
                    return;
                }
                HubSelected?.Invoke(item.Client);
            }
        }
    }
}