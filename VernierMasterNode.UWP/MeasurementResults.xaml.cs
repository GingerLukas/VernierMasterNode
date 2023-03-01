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
    public sealed partial class MeasurementResults : Page
    {
        public delegate void RestartHandler(VernierSensor drop, VernierSensor conductivity);

        public event RestartHandler Restart;

        private (double x, double y) _intersection;
        private double _dropVolume;
        private double _dilutionFactor;
        private double _acidConcentration;
        private double _result;
        private VernierSensor _dropSensor;
        private VernierSensor _conductivitySensor;

        public MeasurementResults()
        {
            this.InitializeComponent();


            _dropVolume = double.Parse(TextBoxDropVolume.Text);
            _dilutionFactor = double.Parse(TextBoxDilutionFactor.Text);
            _acidConcentration = double.Parse(TextBoxAcidConcentration.Text);
        }

        public void SetSensorInfo(VernierSensor drops, VernierSensor conductivity)
        {
            _dropSensor = drops;
            _conductivitySensor = conductivity;

            LinearAxisDrops.Title = $"{drops.Description} [{drops.Unit}]";
            LinearAxisConductivity.Title = $"{conductivity.Description} [{conductivity.Unit}]";
        }

        private IndexValuePair[] _values;

        public IndexValuePair[] Values
        {
            get => _values;
            set
            {
                _values = value;
                ValuesUpdated();
            }
        }

        private void TextBox_OnBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c) && c != '.');
        }

        private void TextBox_OnTextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            sender.Text = new String(sender.Text.Where(c => char.IsDigit(c) || c == '.').ToArray());
        }

        private void ValuesUpdated()
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { LineSeriesMain.ItemsSource = _values; });
            LinerRegression left = new LinerRegression();
            LinerRegression right = new LinerRegression();

            double min = Double.MaxValue;
            int index = -1;
            for (int i = 0; i < _values.Length; i++)
            {
                if (_values[i].Value < min)
                {
                    min = _values[i].Value;
                    index = i;
                }
            }

            left.Fit(_values.Take(index + 1).ToArray());
            right.Fit(_values.Skip(index).ToArray());

            List<IndexValuePair> leftValues = new List<IndexValuePair>();
            for (int i = 0; i < index + 2; i++)
            {
                leftValues.Add(new IndexValuePair(i, left.Predict(i)));
            }

            List<IndexValuePair> rightValues = new List<IndexValuePair>();
            for (int i = index - 1; i < _values.Length; i++)
            {
                rightValues.Add(new IndexValuePair(i, right.Predict(i)));
            }

            LineSeriesLeft.ItemsSource = leftValues;
            LineSeriesRight.ItemsSource = rightValues;

            _intersection = right.Intersection(left);

            Recalculate();
        }

        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            _dropVolume = double.Parse(TextBoxDropVolume.Text);
            _dilutionFactor = double.Parse(TextBoxDilutionFactor.Text);
            _acidConcentration = double.Parse(TextBoxAcidConcentration.Text);

            Recalculate();
        }

        private void Recalculate()
        {
            double totalDropVolume = _dropVolume * _intersection.x;
            _result = 36.45 * _acidConcentration * totalDropVolume *
                      _dilutionFactor; //zredovaci faktor isto solution volume (delusion factor)

            TextBlockResult.Text = $"{_result:F4} mg/l";
        }


        private void TextBoxAcidConcentration_OnTextCompositionEnded(TextBox sender, TextCompositionEndedEventArgs args)
        {
            _dropVolume = double.Parse(TextBoxDropVolume.Text);
            _dilutionFactor = double.Parse(TextBoxDilutionFactor.Text);
            _acidConcentration = double.Parse(TextBoxAcidConcentration.Text);

            Recalculate();
        }

        private void RestartButton_OnClick(object sender, RoutedEventArgs e)
        {
            Restart?.Invoke(_dropSensor, _conductivitySensor);
        }
    }
}