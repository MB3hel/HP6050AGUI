using Microsoft.Win32;
using NationalInstruments.Visa;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            public DataPoint(long timeMs, double[] measuredVoltages, double[] measuredCurrents) {
                this.timeMs = timeMs;
                this.measuredVoltages = measuredVoltages;
                this.measuredCurrents = measuredCurrents;
            }
            public long timeMs { get; private set; }
            public double[] measuredVoltages { get; private set; }
            public double[] measuredCurrents { get; private set; }
        }

        List<DataPoint> testResults = new List<DataPoint>();

        string endReason = "";
        bool userCanceledTest = false;
        string lastResourceString;
        string currentTestName = "";
        MessageBasedSession mbSession;

        public MainWindow() {
            InitializeComponent();
            setControlState(false);
            ControlWriter writer = new ControlWriter(this, output);
            Console.SetOut(writer);
            Console.SetError(writer);
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
                        setControlState(true);
                    } catch (InvalidCastException) {
                        string message = "Resource selected must be a message-based session";
                        string title = "Invalid Resource";
                        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
                        Console.WriteLine(Title + ": " + message);
                    } catch(Exception err) {
                        string title = "Error Opening Session";
                        MessageBox.Show(err.Message, title, MessageBoxButton.OK, MessageBoxImage.Error);
                        Console.WriteLine(title + ": " + err.Message);
                    }
                }
            }
        }

        private void closeSession_Click(object sender, RoutedEventArgs e) {
            setControlState(false);
            mbSession.Dispose();
            if(mbSession != null && !mbSession.IsDisposed)
                Console.WriteLine("Closing session...");
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

            // EDIT THESE LINES
            currentTestName = "Quick Test";
            await startBatteryTest(10, 1, 0.5, 10000);


            Console.WriteLine("Test completed.");
            Console.WriteLine(endReason);
            MessageBox.Show(endReason, "Test Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void button10a_clicked(object sender, RoutedEventArgs e) {
            bool doTest = false;
            if (testResults.Count > 0) {
                var res = MessageBox.Show("Starting a new test will discard all unsaved data from the previous test. Are you sure you want to start a new test?",
                    "Confirm Test",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                doTest = (res == MessageBoxResult.Yes);
            } else {
                doTest = true;
            }

            userCanceledTest = false;

            // EDIT THESE LINES
            currentTestName = "10A Test";
            await startBatteryTest(11.95, 1, 10, 3600 * 1000);


            Console.WriteLine("Test completed.");
            Console.WriteLine(endReason);
            MessageBox.Show(endReason, "Test Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void button18a_clicked(object sender, RoutedEventArgs e) {
            bool doTest = false;
            if (testResults.Count > 0) {
                var res = MessageBox.Show("Starting a new test will discard all unsaved data from the previous test. Are you sure you want to start a new test?",
                    "Confirm Test",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                doTest = (res == MessageBoxResult.Yes);
            } else {
                doTest = true;
            }

            userCanceledTest = false;

            // EDIT THESE LINES
            currentTestName = "18A Test";
            await startBatteryTest(11.95, 1, 18, 3600 * 1000);


            Console.WriteLine("Test completed.");
            Console.WriteLine(endReason);
            MessageBox.Show(endReason, "Test Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void saveButton_Click(object sender, RoutedEventArgs e) {
           if(testResults.Count > 0) {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.FileName = "Log-" + DateTime.Now.ToString("MM_dd_yyyy_HH_mm_ss_tt");
                saveDialog.DefaultExt = ".csv";
                saveDialog.Filter = "CSV Files (.csv)|*.csv";
                bool? res = saveDialog.ShowDialog();
                if(res == true) {
                    string filename = saveDialog.FileName;
                    saveLogToCSV(filename);
                }
            } else {
                MessageBox.Show("There is no test data to save.", "No Data", MessageBoxButton.OK, MessageBoxImage.Exclamation);
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
            testResults.Clear();
            await Task.Run(() => {
                // Newlines may be needed after each command???
                mbSession.RawIO.Write("CHANNEL 1");
                mbSession.RawIO.Write("INPUT OFF");
                mbSession.RawIO.Write("MODE:CURRENT");
                mbSession.RawIO.Write("CURR " + (dischargeRate * 1000) + "MA");
                mbSession.RawIO.Write("CHANNEL 2");
                mbSession.RawIO.Write("INPUT OFF");
                mbSession.RawIO.Write("MODE:CURRENT");
                mbSession.RawIO.Write("CURR " + (dischargeRate * 1000) + "MA");
                mbSession.RawIO.Write("CHANNEL 3");
                mbSession.RawIO.Write("INPUT OFF");
                mbSession.RawIO.Write("MODE:CURRENT");
                mbSession.RawIO.Write("CURR " + (dischargeRate * 1000) + "MA");

                mbSession.RawIO.Write("CHANNEL 1");
                mbSession.RawIO.Write("INPUT ON");
                mbSession.RawIO.Write("CHANNEL 2");
                mbSession.RawIO.Write("INPUT ON");
                mbSession.RawIO.Write("CHANNEL 3");
                mbSession.RawIO.Write("INPUT ON");

                // Start timing
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                double[] measuredVoltages = { 0, 0, 0 };
                double[] measuredCurrents = { 0, 0, 0 };
                bool[] offInputs = new bool[] { false, false, false };
                bool shouldEnd = false;
                bool timedOut = false;
                do {
                    this.Dispatcher.Invoke(() => {
                        testProgress.IsIndeterminate = true;
                    });
                    try {

                        for (int i = 1; i <= 3; ++i) {
                            // Switch channel
                            mbSession.RawIO.Write("CHANNEL " + i);

                            // Read voltage
                            mbSession.RawIO.Write("MEASURE:VOLTAGE?");
                            measuredVoltages[i - 1] = Double.Parse(mbSession.RawIO.ReadString());
                            // Read actual current
                            mbSession.RawIO.Write("MEASURE:CURRENT?");
                            measuredCurrents[i - 1] = Double.Parse(mbSession.RawIO.ReadString());
                        }

                        // Add the data to the list and to the UI
                        testResults.Add(new DataPoint(stopWatch.ElapsedMilliseconds, measuredVoltages, measuredCurrents));
                        long elapsed = stopWatch.ElapsedMilliseconds;
                        this.Dispatcher.Invoke(() => {
                            voltageReading1.Text = measuredVoltages[0] + " V";
                            voltageReading2.Text = measuredVoltages[1] + " V";
                            voltageReading3.Text = measuredVoltages[2] + " V";
                            currentReading1.Text = measuredCurrents[0] + " A";
                            currentReading2.Text = measuredCurrents[1] + " A";
                            currentReading3.Text = measuredCurrents[2] + " A";
                            remainingTime.Text = "" + ((maxTimeMs - elapsed) / 1000.0);
                        });
                    } catch (Exception e) {
                        Console.WriteLine("Error running test: " + e.Message);
                    }

                    // Turn off inputs for eod channels
                    for(int i = 1; i <= 3; ++i) {
                        if(!offInputs[i - 1]) {
                            if (measuredVoltages[i - 1] < cellCount * eodVoltage) {
                                mbSession.RawIO.Write("CHANNEL " + i);
                                mbSession.RawIO.Write("INPUT OFF");
                                offInputs[i - 1] = true;
                            }
                        }
                    }

                    // Stop conditions
                    if (maxTimeMs > -1)
                        timedOut = stopWatch.ElapsedMilliseconds >= maxTimeMs;
                    shouldEnd = offInputs[0] && offInputs[1] && offInputs[2];
                } while (!shouldEnd && !userCanceledTest && !timedOut);
                if (userCanceledTest) {
                    endReason = "Canceled by user.";
                } else if (timedOut) {
                    endReason = "Reached time limit.";
                }else {
                    endReason = "Reached end of discharge voltage.";
                }

                // Turn all inputs off
                mbSession.RawIO.Write("CHANNEL 1");
                mbSession.RawIO.Write("INPUT OFF");
                mbSession.RawIO.Write("CHANNEL 2");
                mbSession.RawIO.Write("INPUT OFF");
                mbSession.RawIO.Write("CHANNEL 3");
                mbSession.RawIO.Write("INPUT OFF");

                this.Dispatcher.Invoke(() => {
                    testProgress.IsIndeterminate = false;
                    testProgress.Value = 0;
                });
                stopWatch.Stop();
            });
        }

        public void saveLogToCSV(string filepath) {
            StreamWriter file = new StreamWriter(@filepath, append:false);

            // Add the headers
            file.WriteLine(currentTestName + "," + name1.Text + "," + name1.Text + "," + name2.Text + "," + name2.Text + "," + name3.Text + "," + name3.Text);
            file.WriteLine("Time (ms),Voltage1 (V),Current1 (A),Voltage2 (V),Current2 (A),Voltage3 (V),Current3 (A)");

            // Add each data point
            foreach(DataPoint p in testResults) {
                file.WriteLine(p.timeMs + "," + p.measuredVoltages[0] + "," + p.measuredCurrents[0] + "," + p.measuredVoltages[1] + "," + p.measuredCurrents[1] + "," + p.measuredVoltages[2] + "," + p.measuredCurrents[2]);
            }

            // Finish writing and close
            file.Flush();
            file.Close();
        }

    }
}
