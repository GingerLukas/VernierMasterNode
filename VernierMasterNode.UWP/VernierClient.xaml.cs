using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using VernierMasterNode.Shared;
using System.Threading.Tasks;
using Windows.UI.Core;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace VernierMasterNode.UWP
{
    public sealed partial class VernierClient : UserControl
    {
        private Client _client;

        private List<string> _espDevices = new List<string>();
        private UInt32 _deviceXid = 421;
        private UInt32 _deviceYid = 403;
        private decimal _sum = 0;
        private int _count = 0;
        private ObservableCollection<IndexValuePair> _values = new ObservableCollection<IndexValuePair>();


        private ulong _deviceXserial = 0;
        private VernierSensor? _deviceX;

        private ulong _deviceYserial = 0;
        private VernierSensor? _deviceY;


        public VernierClient()
        {
            this.InitializeComponent();

            SetStatus(TextConductivityStatus, "Not Found");
            SetStatus(TextDropCountStatus, "Not Found");

            LineSeries.ItemsSource = _values;
            _client = new Client();
            _client.OnEspDeviceConnected += ClientOnOnEspDeviceConnected;
            _client.OnDeviceFound += ClientOnOnDeviceFound;
            _client.OnSensorInfo += ClientOnOnSensorInfo;
            _client.OnSensorValuesUpdated += ClientOnOnSensorValuesUpdated;
            _client.OnScanStopped += ClientOnOnScanStopped;

            Task.Run(async () =>
            {
                var devices = await _client.GetEspDevices();

                foreach (EspDevice espDevice in devices)
                {
                    ClientOnOnEspDeviceConnected(espDevice.Name);
                }
            });
        }

        private async void OnDrop()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (_count > 0)
                {
                    _values.Add(new IndexValuePair(_values.Count, _sum / _count));
                }
                else if (_values.Count > 0)
                {
                    _values.Add(new IndexValuePair(_values.Count, _values.Last().Value));
                }

                _sum = 0;
                _count = 0;
            });
        }

        private void ClientOnOnScanStopped(string uid)
        {
            if (_started)
            {
                _client.StartSensor(uid, _deviceXserial, _deviceX.Id).GetAwaiter().GetResult();
                _client.StartSensor(uid, _deviceYserial, _deviceY.Id).GetAwaiter().GetResult();

                SetStatus(TextDropCountStatus, "OK");
                SetStatus(TextConductivityStatus, "OK");
            }
        }

        private void ClientOnOnSensorValuesUpdated(string uid, ulong serialid, uint sensorid, SensorValuesPacket packet)
        {
            if (_deviceX.Id == sensorid)
            {
                lock (_values)
                {
                    OnDrop();
                }
            }

            if (_deviceY.Id == sensorid)
            {
                lock (_values)
                {
                    _sum += packet.Values.Sum();
                    _count += packet.Values.Length;
                }
            }
        }


        bool _started = false;

        private void SetStatus(TextBlock textBlock, string status)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                lock (textBlock)
                {
                    textBlock.Text = status;
                }
            });
        }

        private void ClientOnOnSensorInfo(string uid, ulong serialid, VernierSensor sensor)
        {
            if (_deviceXid == sensor.Id)
            {
                _deviceX = sensor;
                _deviceXserial = serialid;
                SetStatus(TextDropCountStatus, "Found");
            }

            if (_deviceYid == sensor.Id)
            {
                _deviceY = sensor;
                _deviceYserial = serialid;
                SetStatus(TextConductivityStatus, "Found");
            }

            if (!_started && _deviceX != null && _deviceY != null)
            {
                _started = true;

                _client.StopScan(uid).GetAwaiter().GetResult();
            }
        }

        private async void ClientOnOnDeviceFound(string uid, ulong serialId)
        {
            await _client.ConnectToDevice(uid, serialId);
        }

        private async void ClientOnOnEspDeviceConnected(string uid)
        {
            await _client.RegisterForEvents(uid);
            await _client.StartScan(uid);
        }
    }
}