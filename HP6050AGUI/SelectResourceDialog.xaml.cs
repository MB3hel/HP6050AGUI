using NationalInstruments.Visa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HP6050AGUI {
    /// <summary>
    /// Interaction logic for SelectResourceDialog.xaml
    /// </summary>
    public partial class SelectResourceDialog : Window {
        public SelectResourceDialog() {
            InitializeComponent();
        }

        private void okButton_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            using (var rmSession = new ResourceManager()) {
                try {
                    var resources = rmSession.Find("(ASRL|GPIB|TCPIP|USB)?*");
                    foreach (string s in resources) {
                        availableResourceList.Items.Add(s);
                    }
                } catch (Ivi.Visa.VisaException) {
                    Console.WriteLine("No Supported devices found!");
                }
            }
        }

        private void availableResourceList_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            string selectedString = (string)availableResourceList.SelectedItem;
            ResourceName = selectedString;
            this.DialogResult = true;
            this.Close();
        }

        public string ResourceName {
            get {
                return resourceString.Text;
            }
            set {
                resourceString.Text = value;
            }
        }
    }
}
