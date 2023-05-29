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
                var element = new SensorSelectionItem(sensor);
                switch (sensor.Id)
                {
                    case 421:
                        element.Selected += DropSensorOnSelected;
                        DropCounterListBox.Children.Add(element);
                        break;
                    case 403:
                        element.Selected += ConductivitySensorOnSelected;
                        ConductivityListBox.Children.Add(element);
                        break;
                }
            });
        }

        private void ConductivitySensorOnSelected(SensorSelectionItem element)
        {
            foreach (SensorSelectionItem item in ConductivityListBox.Children.OfType<SensorSelectionItem>())
            {
                item.SetActive(false);
            }

            element.SetActive(true);
            _conductivitySensor = element.Sensor;
        }

        private void DropSensorOnSelected(SensorSelectionItem element)
        {
            foreach (SensorSelectionItem item in DropCounterListBox.Children.OfType<SensorSelectionItem>())
            {
                item.SetActive(false);
            }

            element.SetActive(true);
            _dropSensor = element.Sensor;
        }

        private void ContinueButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_dropSensor == null || _conductivitySensor == null)
            {
                return;
            }

            ProgressRingNext.IsActive = true;
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => { SensorSelected?.Invoke(_dropSensor, _conductivitySensor); });
        }
    }
}