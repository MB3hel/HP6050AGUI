using HP6050AInterface;
using System;
using System.Collections.Generic;
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

        Communicator tester;

        public MainWindow() {
            InitializeComponent();
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e) {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        async Task runBatteryTest() {
            tester.stopTest();
            BatteryTest test = new BatteryTest(10, 1, 30);
            tester.startTest(test);
            await Task.Run(() => {
                BatteryTestOutputParser parser = new BatteryTestOutputParser(tester);
                while (tester.isRunningTest()) {
                    parser.process();
                    //TODO: Log the voltage and current for this point in time
                }
            });
        }
    }
}
