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
            
            DropCounterListBox.Children.Add(new SensorSelectionItem("FD53A9C5",EVernierSensorType.Drop));
            DropCounterListBox.Children.Add(new SensorSelectionItem("A6CB864E",EVernierSensorType.Drop));
            DropCounterListBox.Children.Add(new SensorSelectionItem("E8F946CC",EVernierSensorType.Drop));
            
            ConductivityListBox.Children.Add(new SensorSelectionItem("F85BCA4C",EVernierSensorType.Conductivity));
            ConductivityListBox.Children.Add(new SensorSelectionItem("AC5642BE",EVernierSensorType.Conductivity));
            ConductivityListBox.Children.Add(new SensorSelectionItem("F24AC8AB",EVernierSensorType.Conductivity));
            ConductivityListBox.Children.Add(new SensorSelectionItem("AC56CA4C",EVernierSensorType.Conductivity));
            ConductivityListBox.Children.Add(new SensorSelectionItem("CB5C42BE",EVernierSensorType.Conductivity));
        }

        private void ContinueButton_OnClick(object sender, RoutedEventArgs e)
        {
            SensorSelected?.Invoke(_dropSensor, _conductivitySensor);
        }
    }
}
