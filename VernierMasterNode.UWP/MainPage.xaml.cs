using System;
using System.Collections.Generic;
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
using Windows.Web.Http;
using Microsoft.AspNetCore.SignalR.Client;
using VernierMasterNode.Shared;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace VernierMasterNode.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private HubConnection _connection;
        public MainPage()
        {
            this.InitializeComponent();

            _connection = new HubConnectionBuilder().WithUrl(new Uri("http://127.0.0.1:5153/Realtime")).WithAutomaticReconnect().Build();

            _connection.On<string, ulong>(nameof(IRealtimeClient.DeviceFound), OnDeviceFound);

            _connection.StartAsync().GetAwaiter().GetResult();

            _connection.InvokeAsync("RegisterForEvents", "FFAACCAABBFF");
        }

        private void OnDeviceFound(string uid, ulong serialId)
        {
            throw new NotImplementedException();
        }
    }
}
