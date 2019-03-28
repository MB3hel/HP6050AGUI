using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HP6050AGUI {
    public class ControlWriter : TextWriter {
        private Window owner;
        private TextBox textbox;
        public ControlWriter(Window owner, Control textbox) {
            this.owner = owner;
            this.textbox = textbox as TextBox;
        }

        public override void Write(char value) {
            owner.Dispatcher.Invoke(() => {
                textbox.Text += value;
            });
        }

        public override void Write(string value) {
            owner.Dispatcher.Invoke(() => {
                textbox.Text += value;
            });
        }

        public override Encoding Encoding {
            get { return Encoding.ASCII; }
        }
    }
}
