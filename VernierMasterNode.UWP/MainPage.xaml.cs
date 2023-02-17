using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
using VernierMasterNode.UWP.Services;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace VernierMasterNode.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Page _selectHub;
        private Page _selectSensors;
        private Page _measurement;
        private Page _result;
        public MainPage()
        {
            this.InitializeComponent();
            MainFrame.Navigate(typeof(HubSelection));

            HubSelection selection = MainFrame.Content as HubSelection;
            selection.HubSelected += SelectionOnHubSelected;
        }

        private void SelectionOnHubSelected(Client client)
        {
            SensorService.SetClient(client);
            MainFrame.Navigate(typeof(SensorSelection));
            
            
            SensorSelection selection = MainFrame.Content as SensorSelection;
            selection.SensorSelected += SelectionOnSensorSelected;
        }

        private void SelectionOnSensorSelected(VernierSensor dropsensor, VernierSensor conductivitysensor)
        {
            MainFrame.Navigate(typeof(MeasurementMain));
            
            
            MeasurementMain selection = MainFrame.Content as MeasurementMain;
            selection.MeasurementFinished += SelectionOnMeasurementFinished;
        }

        private void SelectionOnMeasurementFinished(List<IndexValuePair> values)
        {
            MainFrame.Navigate(typeof(MeasurementResults));
            
            
            MeasurementResults selection = MainFrame.Content as MeasurementResults;
        }
    }
}
