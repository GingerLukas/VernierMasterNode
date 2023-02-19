using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using VernierMasterNode.Shared;
using VernierMasterNode.UWP.Services;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VernierMasterNode.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MeasurementMain : Page
    {
        public delegate void MeasurementFinishedHandler(IndexValuePair[] values,string drops, string conductivity);

        public event MeasurementFinishedHandler MeasurementFinished;
        private readonly ObservableCollection<IndexValuePair> _values;

        private decimal? _lastConductivity = null;

        public MeasurementMain()
        {
            this.InitializeComponent();
            _values = new ObservableCollection<IndexValuePair>();
            LineSeriesMain.ItemsSource = _values;
            SensorService.SensorValuesUpdated += SensorServiceOnSensorValuesUpdated;
        }

        public void SetSensorInfo(VernierSensor drops, VernierSensor conductivity)
        {
            LinearAxisDrops.Title = $"{drops.Description} [{drops.Unit}]";
            LinearAxisConductivity.Title = $"{conductivity.Description} [{conductivity.Unit}]";
        }

        private async void SensorServiceOnSensorValuesUpdated(string uid, ulong serialid, uint sensorid,
            SensorValuesPacket packet)
        {
            switch ((EVernierSensorType)sensorid)
            {
                case EVernierSensorType.Conductivity:
                    _lastConductivity = packet.Values.LastOrDefault();
                    break;
                case EVernierSensorType.Drop:
                    if (_lastConductivity != null)
                    {
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                            () => { _values.Add(new IndexValuePair(_values.Count, _lastConductivity.Value)); });
                    }

                    break;
            }
        }

        private async void ContinueButton_OnClick(object sender, RoutedEventArgs e)
        {
            ProgressRingNext.IsActive = true;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    MeasurementFinished?.Invoke(_values.ToArray(),(string)LinearAxisDrops.Title, (string)LinearAxisConductivity.Title);
                    
                });
        }

        private async void DemoButton_OnClick(object sender, RoutedEventArgs e)
        {
            ProgressRingDemo.IsActive = true;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                _values.Clear();
                _values.Add(new IndexValuePair(_values.Count, 6.1));
                _values.Add(new IndexValuePair(_values.Count, 5.82));
                _values.Add(new IndexValuePair(_values.Count, 5.72));
                _values.Add(new IndexValuePair(_values.Count, 5.52));
                _values.Add(new IndexValuePair(_values.Count, 5.34));
                _values.Add(new IndexValuePair(_values.Count, 5.14));
                _values.Add(new IndexValuePair(_values.Count, 5.04));
                _values.Add(new IndexValuePair(_values.Count, 4.84));
                _values.Add(new IndexValuePair(_values.Count, 4.74));
                _values.Add(new IndexValuePair(_values.Count, 4.54));
                _values.Add(new IndexValuePair(_values.Count, 4.35));
                _values.Add(new IndexValuePair(_values.Count, 4.16));
                _values.Add(new IndexValuePair(_values.Count, 3.97));
                _values.Add(new IndexValuePair(_values.Count, 3.78));
                _values.Add(new IndexValuePair(_values.Count, 3.68));
                _values.Add(new IndexValuePair(_values.Count, 3.5));
                _values.Add(new IndexValuePair(_values.Count, 3.31));
                _values.Add(new IndexValuePair(_values.Count, 3.13));
                _values.Add(new IndexValuePair(_values.Count, 2.94));
                _values.Add(new IndexValuePair(_values.Count, 2.85));
                _values.Add(new IndexValuePair(_values.Count, 2.66));
                _values.Add(new IndexValuePair(_values.Count, 2.48));
                _values.Add(new IndexValuePair(_values.Count, 2.29));
                _values.Add(new IndexValuePair(_values.Count, 1.969));
                _values.Add(new IndexValuePair(_values.Count, 1.874));
                _values.Add(new IndexValuePair(_values.Count, 1.914));
                _values.Add(new IndexValuePair(_values.Count, 2.07));
                _values.Add(new IndexValuePair(_values.Count, 2.21));
                _values.Add(new IndexValuePair(_values.Count, 2.36));
                _values.Add(new IndexValuePair(_values.Count, 2.51));
                _values.Add(new IndexValuePair(_values.Count, 2.66));
                _values.Add(new IndexValuePair(_values.Count, 2.81));
                _values.Add(new IndexValuePair(_values.Count, 2.96));
                _values.Add(new IndexValuePair(_values.Count, 3.11));
                _values.Add(new IndexValuePair(_values.Count, 3.26));
                _values.Add(new IndexValuePair(_values.Count, 3.4));
                _values.Add(new IndexValuePair(_values.Count, 3.56));
                _values.Add(new IndexValuePair(_values.Count, 3.71));
                _values.Add(new IndexValuePair(_values.Count, 3.84));
                _values.Add(new IndexValuePair(_values.Count, 4.0));
                _values.Add(new IndexValuePair(_values.Count, 4.14));
                _values.Add(new IndexValuePair(_values.Count, 4.29));
                _values.Add(new IndexValuePair(_values.Count, 4.43));
                _values.Add(new IndexValuePair(_values.Count, 4.58));
                _values.Add(new IndexValuePair(_values.Count, 4.72));
                _values.Add(new IndexValuePair(_values.Count, 4.86));
                _values.Add(new IndexValuePair(_values.Count, 5.01));
                _values.Add(new IndexValuePair(_values.Count, 5.15));
                _values.Add(new IndexValuePair(_values.Count, 5.23));
                _values.Add(new IndexValuePair(_values.Count, 5.3));
                _values.Add(new IndexValuePair(_values.Count, 5.37));
            });
            
            ProgressRingDemo.IsActive = false;
        }
    }
}