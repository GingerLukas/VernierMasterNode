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
        public SensorSelectionItem(string name,EVernierSensorType sensorType)
        {
            this.InitializeComponent();


            SerialNumber.Text = name;
            switch (sensorType)
            {
                case EVernierSensorType.Drop:
                    TypeIcon.Glyph = "&#xEB42";
                    break;
                case EVernierSensorType.Conductivity:
                    TypeIcon.Glyph = "&#xE945";
                    break;
                default:
                    TypeIcon.Glyph = "&#xE9CE";
                    break;
            }
        }
    }
}
