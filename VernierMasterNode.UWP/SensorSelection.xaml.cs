using System;
using System.Collections.Generic;
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
    public sealed partial class SensorSelection : Page
    {
        public delegate void SensorSelectedHandler(VernierSensor dropSensor, VernierSensor conductivitySensor);

        public event SensorSelectedHandler SensorSelected;

        private VernierSensor _dropSensor;
        private VernierSensor _conductivitySensor;

        public SensorSelection()
        {
            this.InitializeComponent();


            SensorService.SensorFound += SensorServiceOnSensorFound;
            SensorService.Start();


        }

        private void SensorServiceOnSensorFound(VernierSensor sensor)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                switch (sensor.Id)
                {
                    case 421:
                        DropCounterListBox.Children.Add(new SensorSelectionItem(sensor.DeviceIdToText(),
                            EVernierSensorType.Drop));
                        break;
                    case 403:
                        ConductivityListBox.Children.Add(new SensorSelectionItem(sensor.DeviceIdToText(),
                            EVernierSensorType.Conductivity));
                        break;
                }
            });
        }

        private void ContinueButton_OnClick(object sender, RoutedEventArgs e)
        {
            SensorSelected?.Invoke(_dropSensor, _conductivitySensor);
        }
    }
}