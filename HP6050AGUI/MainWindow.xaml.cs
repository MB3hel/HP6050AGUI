using Microsoft.Win32;
using NationalInstruments.Visa;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
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
            public DataPoint(long timeMs) {
                this.timeMs = timeMs;
                this.measuredVoltages = new double[0];
                this.measuredCurrents = new double[0];
            }
            public long timeMs { get; set; }
            public double[] measuredVoltages { get; set; }
            public double[] measuredCurrents { get; set; }
        }

        const string HEADER_CHANNEL = "Channel";
        const string HEADER_VOLTAGE = "Voltage (V)";
        const string HEADER_CURRENT = "Current (A)";
        const string HEADER_BATNAME = "Battery Name";

        public class BatteryEntry {
            public int channel { get; set; }
            public double voltage { get; set; }
            public double current { get; set; }
            public string batteryName { get; set; }
        }

        public class TestSettings {
            public string testName { get; set; }
            public double eodVoltage { get; set; }
            public double dischargeRateAmps { get; set; }
            public long maxTimeSec { get; set; }
        }

        Dictionary<object, TestSettings> registeredTests = new Dictionary<object, TestSettings>();
        ObservableCollection<BatteryEntry> batteryEntries = new ObservableCollection<BatteryEntry>();

        StreamWriter logFile;

        int channelCount;
        string endReason = "";
        bool userCanceledTest = false;
        string lastResourceString;
        ElectronicLoad tester = new ElectronicLoad();

        public MainWindow() {
            InitializeComponent();

            this.Closing += MainWindow_Closing;

            batteryDataGrid.ItemsSource = batteryEntries;
            setControlState(false);
            ControlWriter writer = new ControlWriter(this, output);
            Console.SetOut(writer);
            Console.SetError(writer);

            // Setup tests here
            createTest(new TestSettings() {
                testName = "Quick Test",
                eodVoltage = 11.95,
                dischargeRateAmps = 0.5,
                maxTimeSec = 10
            });
            createTest(new TestSettings() {
                testName = "10A Test",
                eodVoltage = 11.95,
                dischargeRateAmps = 10,
                maxTimeSec = 5400   // 1.5 hrs
            });
            createTest(new TestSettings() {
                testName = "18A Test",
                eodVoltage = 11.95,
                dischargeRateAmps = 18,
                maxTimeSec = 3600
            });
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e) {
            userCanceledTest = true;
            for (int i = 1; i <= channelCount; ++i) {
                tester.inputOff(i);
            }
        }

        public void setControlState(bool isOpen) {
            openSession.IsEnabled = !isOpen;
            closeSession.IsEnabled = isOpen;
            testSection.IsEnabled = isOpen;
            statusSection.IsEnabled = isOpen;
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
                        tester.open(d.ResourceName);
                        setControlState(true);

                        batteryEntries.Clear();
                        channelCount = tester.channelCount();
                        for (int i = 1; i <= channelCount; ++i) {
                            batteryEntries.Add(new BatteryEntry() { channel = i, voltage = 0, current = 0, batteryName = "" });
                        }
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
            bool wasOpen = tester.hasBeenOpened();
            setControlState(false);
            tester.close();
            if(wasOpen)
                Console.WriteLine("Closing session...");
        }

        private void createTest(TestSettings settings) {
            Button button = new Button();
            button.Content = settings.testName;
            button.Click += testButtonHandler;
            button.Margin = new Thickness(0, 2, 5, 2);

            testsPanel.Children.Add(button);
            registeredTests.Add(button, settings);
        }

        private async void testButtonHandler(object sender, RoutedEventArgs e) {

            if (!registeredTests.ContainsKey(sender)) {
                Console.WriteLine("ATTEMPTED TO START UNKNOWN TEST!!! THIS SHOULD NOT BE POSSIBLE.");
                return;
            }

            TestSettings settings = registeredTests[sender];

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.FileName = "Log-" + DateTime.Now.ToString("MM_dd_yyyy_HH_mm_ss_tt");
            saveDialog.DefaultExt = ".csv";
            saveDialog.Filter = "CSV Files (.csv)|*.csv";
            bool? res = saveDialog.ShowDialog();
            if (res.GetValueOrDefault()) {

                logFile = new StreamWriter(saveDialog.FileName, append: false);

                // Write header
                string testInfoHeader = settings.testName + ",";
                string mainHeader = "Time (ms),";
                for (int i = 0; i < channelCount; ++i) {
                    testInfoHeader += batteryEntries[i].batteryName + "," + batteryEntries[i].batteryName + ",";
                    mainHeader += "Voltage" + (i + 1) + " (V),Current" + (i + 1) + " (A),";
                }
                testInfoHeader = testInfoHeader.Remove(testInfoHeader.Length - 1);
                mainHeader = mainHeader.Remove(mainHeader.Length - 1);

                logFile.WriteLine(testInfoHeader);
                logFile.WriteLine(mainHeader);
            } else {
                Console.WriteLine("Test aborted beofre starting.");
                return;
            }            

            userCanceledTest = false;
            await startBatteryTest(settings.eodVoltage, settings.dischargeRateAmps, settings.maxTimeSec * 1000);

            Console.WriteLine("Test completed.");
            Console.WriteLine(endReason);
            MessageBox.Show(endReason, "Test Complete", MessageBoxButton.OK, MessageBoxImage.Information);
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
        public async Task startBatteryTest(double eodVoltage, double dischargeRate, long maxTimeMs = -1) {
            await Task.Run(() => {
                lock (tester) {
                    try {

                        // Setup data table
                        Dispatcher.Invoke(() => {
                            // Do not allow editing of battery names during the test
                            batteryDataGrid.Columns[3].IsReadOnly = true;
                        });

                        // Setup all inputs and turn on
                        for (int i = 1; i <= channelCount; ++i) {
                            tester.inputOff(i);
                            tester.setCurrent(i, (int)(dischargeRate * 1000));
                            tester.inputOn(i);
                        }

                        // Start timing
                        Stopwatch stopWatch = new Stopwatch();
                        stopWatch.Start();
                        double[] measuredVoltages = new double[channelCount];
                        double[] measuredCurrents = new double[channelCount];
                        bool[] offInputs = new bool[channelCount];

                        for (int i = 0; i < channelCount; ++i) {
                            measuredVoltages[i] = 0;
                            measuredCurrents[i] = 0;
                            offInputs[i] = false;
                        }

                        bool shouldEnd = false;
                        bool timedOut = false;
                        do {
                            Dispatcher.InvokeAsync(() => {
                                testProgress.IsIndeterminate = true;
                            });
                            try {

                                for (int i = 1; i <= channelCount; ++i) {
                                    measuredVoltages[i - 1] = tester.readVoltage(i);
                                    measuredCurrents[i - 1] = tester.readCurrent(i);
                                    batteryEntries[i - 1].voltage = measuredVoltages[i - 1];
                                    batteryEntries[i - 1].current = measuredCurrents[i - 1];
                                }

                                // Log the sample
                                string data = stopWatch.ElapsedMilliseconds + ",";
                                for (int i = 0; i < channelCount; ++i) {
                                    data += measuredVoltages[i] + "," + measuredCurrents[i] + ",";
                                }
                                data = data.Remove(data.Length - 1);
                                logFile.WriteLine(data);

                                long elapsed = stopWatch.ElapsedMilliseconds;


                                //TODO: May not need to do all of this from dispatcher. Would speed up samples
                                Dispatcher.InvokeAsync(() => {
                                    remainingTime.Text = "" + ((maxTimeMs - elapsed) / 1000.0);
                                    batteryDataGrid.Items.Refresh();
                                });
                            } catch (Exception e) {
                                Console.WriteLine("Error running test: " + e.Message);
                            }

                            // Turn off inputs for eod channels
                            for (int i = 1; i <= channelCount; ++i) {
                                if (!offInputs[i - 1]) {
                                    if (measuredVoltages[i - 1] < eodVoltage) {
                                        tester.inputOff(i);
                                        offInputs[i - 1] = true;
                                    }
                                }
                            }

                            // Stop conditions
                            if (maxTimeMs > -1)
                                timedOut = stopWatch.ElapsedMilliseconds >= maxTimeMs;

                            shouldEnd = true;
                            for (int i = 0; i < channelCount; ++i) {
                                shouldEnd = shouldEnd && offInputs[i];
                                if (!shouldEnd) break;
                            }

                        } while (!shouldEnd && !userCanceledTest && !timedOut);

                        if (userCanceledTest) {
                            endReason = "Canceled by user.";
                        } else if (timedOut) {
                            endReason = "Reached time limit.";
                        } else {
                            endReason = "All channels reached end of discharge voltage.";
                        }
                        stopWatch.Stop();

                    } catch (Exception e) {
                        endReason = "Exception occurred: " + e.Message;
                    }

                    // Turn all inputs off
                    for (int i = 1; i <= channelCount; ++i) {
                        tester.inputOff(i);
                    }

                    this.Dispatcher.Invoke(() => {
                        // Allow editing of battery names after the test
                        batteryDataGrid.Columns[3].IsReadOnly = false;

                        testProgress.IsIndeterminate = false;
                        testProgress.Value = 0;
                    });

                    logFile.Close();
                }
            });
        }

    }
}
