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
using VernierMasterNode.Shared;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace VernierMasterNode.UWP
{
    public sealed partial class SensorSelectionItem : UserControl
    {
        public VernierSensor Sensor { get; set; }

        public delegate void SelectedHandler(SensorSelectionItem item);

        public event SelectedHandler Selected;
        public SensorSelectionItem(VernierSensor sensor)
        {
            Sensor = sensor;
            this.InitializeComponent();


            SerialNumber.Text = sensor.DeviceIdToText();
            switch ((EVernierSensorType)sensor.Id)
            {
                case EVernierSensorType.Drop:
                    TypeIcon.Glyph = "\uEB42";
                    break;
                case EVernierSensorType.Conductivity:
                    TypeIcon.Glyph = "\uE945";
                    break;
                default:
                    TypeIcon.Glyph = "\uE9CE";
                    break;
            }

            HorizontalAlignment = HorizontalAlignment.Stretch;
        }

        public void SetActive(bool value)
        {
            StatusIcon.Symbol = value ? Symbol.Accept : Symbol.Add;
        }

        private void SelectButton_OnClick(object sender, RoutedEventArgs e)
        {
            Selected?.Invoke(this);
        }
    }
}
