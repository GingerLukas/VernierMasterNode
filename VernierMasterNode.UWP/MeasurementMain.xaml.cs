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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VernierMasterNode.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MeasurementMain : Page
    {
        
        public delegate void MeasurementFinishedHandler(List<IndexValuePair> values);
        public event MeasurementFinishedHandler MeasurementFinished;
        private List<IndexValuePair> _values;
        public MeasurementMain()
        {
            this.InitializeComponent();
        }

        private void ContinueButton_OnClick(object sender, RoutedEventArgs e)
        {
            MeasurementFinished?.Invoke(_values);
        }
    }
}
