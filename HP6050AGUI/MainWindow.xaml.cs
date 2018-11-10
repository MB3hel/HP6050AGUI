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

        public MainWindow() {
            InitializeComponent();
        }

        private void openSession_Click(object sender, RoutedEventArgs e) {
            var d = new SelectResourceDialog();
            d.Owner = this;
            d.ShowDialog();
        }

        private void closeSession_Click(object sender, RoutedEventArgs e) {

        }
    }
}
