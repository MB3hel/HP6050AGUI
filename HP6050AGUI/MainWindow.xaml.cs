using NationalInstruments.Visa;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HP6050AGUI {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        struct DataPoint {
            public DataPoint(long timeMs, double measuredVoltage, double measuredCurrent) {
                this.timeMs = timeMs;
                this.measuredVoltage = measuredVoltage;
                this.measuredCurrent = measuredCurrent;
            }
            public long timeMs { get; private set; }
            public double measuredVoltage { get; private set; }
            public double measuredCurrent { get; private set; }
        }

        List<DataPoint> testResults = new List<DataPoint>();

        bool userCanceledTest = false;
        string lastResourceString;
        MessageBasedSession mbSession;

        public MainWindow() {
            InitializeComponent();
            setControlState(false);
        }

        public void setControlState(bool isOpen) {
            openSession.IsEnabled = !isOpen;
            closeSession.IsEnabled = isOpen;
            testSection.IsEnabled = isOpen;
        }

        private void openSession_Click(object sender, RoutedEventArgs e) {
            var d = new SelectResourceDialog();
            d.Owner = this;
            if (lastResourceString != null)
                d.ResourceName = lastResourceString;
            bool? res = d.ShowDialog();
            if (res == true) {
                lastResourceString = d.ResourceName;
                using(var rmSession = new ResourceManager()) {
                    try {
                        Console.WriteLine("Opening " + d.ResourceName);
                        mbSession = (MessageBasedSession)rmSession.Open(d.ResourceName);
                    } catch (InvalidCastException) {
                        MessageBox.Show("Resource selected must be a message-based session", "Invalid Resourece", MessageBoxButton.OK, MessageBoxImage.Error);
                    }catch(Exception err) {
                        MessageBox.Show(err.Message, "Error Opening Session", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void closeSession_Click(object sender, RoutedEventArgs e) {
            setControlState(false);
            mbSession.Dispose();
        }

        // This is how to start a test
        private async void quickTestBtn_Click(object sender, RoutedEventArgs e) {
            bool doTest = false;
            if(testResults.Count > 0) {
                var res = MessageBox.Show("Starting a new test will discard all unsaved data from the previous test. Are you sure you want to start a new test?", 
                    "Confirm Test", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Question);
                doTest = (res == MessageBoxResult.Yes);
            } else {
                doTest = true;
            }

            userCanceledTest = false;
            await startBatteryTest(10, 1, 50, 10000);
            MessageBox.Show("The test has finished.", "Test Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void saveButton_Click(object sender, RoutedEventArgs e) {
            if(testResults.Count > 0) {
                // TODO: Save
            } else {
                // TODO: Show message box
            }
        }

        private void stopTestButton_Click(object sender, RoutedEventArgs e) {
            userCanceledTest = true; // This will cause a stop after the current capture operation
        }





        /// <summary>
        /// Start a battery test using the same method as the script on page 3-6
        /// </summary>
        /// <param name="eodVoltage">End of discharge voltage for a single cell</param>
        /// <param name="cellCount">The number of cells to be discharged in series</param>
        /// <param name="dischargeRate">Constant current dicharge rate in amps</param>
        public async Task startBatteryTest(double eodVoltage, int cellCount, double dischargeRate, long maxTimeMs = -1) {
            await Task.Run(() => {
                mbSession.RawIO.Write("INPUT OFF");
                mbSession.RawIO.Write("MODE:CURRENT");
                mbSession.RawIO.Write("CURRENT:LEVEL" + dischargeRate);
                mbSession.RawIO.Write("INPUT ON");
                // Start timing
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                double measuredVoltage = 0;
                double measuredCurrent = 0;
                bool shouldEnd = false;
                do {
                    testProgress.IsIndeterminate = true;
                    try {
                        // Read voltage
                        mbSession.RawIO.Write("MEASURE:VOLTAGE?");
                        measuredVoltage = Double.Parse(mbSession.RawIO.ReadString());
                        // Read actual current
                        mbSession.RawIO.Write("MEASURE:CURRENT?");
                        measuredCurrent = Double.Parse(mbSession.RawIO.ReadString());

                        // Add the data to the list and to the UI
                        testResults.Add(new DataPoint(stopWatch.ElapsedMilliseconds, measuredVoltage, measuredCurrent));
                        voltageReading.Text = "" + measuredVoltage;
                        currentReading.Text = "" + measuredCurrent;
                    }catch (Exception e) {
                        Console.WriteLine(e.Message);
                    }

                    // Stop conditions
                    bool timedOut = false;
                    if (maxTimeMs > -1)
                        timedOut = stopWatch.ElapsedMilliseconds >= maxTimeMs;
                    shouldEnd = (measuredVoltage < cellCount * eodVoltage) || shouldEnd;
                } while (!shouldEnd || !userCanceledTest);
                testProgress.IsIndeterminate = false;
                testProgress.Value = 0;
                Console.WriteLine("Test done.");
                stopWatch.Stop();
            });
        }

    }
}
