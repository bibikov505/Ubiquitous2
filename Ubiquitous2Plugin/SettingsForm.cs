using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ubiquitous2Plugin
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            uint i = 0;
            if (!String.IsNullOrWhiteSpace(textBox1.Text) && !uint.TryParse(textBox1.Text, out i) )
            {
                MessageBox.Show("Enter Valid Number");
                textBox1.Text = String.Empty;
            }
        }
    }
}
