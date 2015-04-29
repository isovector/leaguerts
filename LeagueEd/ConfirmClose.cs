using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LeagueEd {
    public partial class ConfirmClose : Form {
        public ConfirmClose() {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.No;
            Close();
        }

        private void button1_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Yes;
            Close();
        }

        private void button3_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}