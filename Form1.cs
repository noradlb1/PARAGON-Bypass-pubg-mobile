using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Solace_Bypass
{
    public partial class SY : Form
    {

        public static api KeyAuthApp = new api(
   name: "Sniper",
    ownerid: "XCF4wPS23J",
    secret: "bd96a27b45c59f6e0ba6aa97f69ea1fab6a1aea0ddba29e9432903fae139df17",
    version: "1.0"
);

        public SY()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            KeyAuthApp.init();
            if (!KeyAuthApp.response.success)
            {
                MessageBox.Show(KeyAuthApp.response.message);
                Environment.Exit(0);
            }
            KeyAuthApp.check();
            if(File.Exists(@"C:\BypassKey.txt")) textBox1.Text = File.ReadAllText(@"C:\BypassKey.txt");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private void LicBtn_Click(object sender, EventArgs e)
        {
            KeyAuthApp.license(textBox1.Text);
            if (KeyAuthApp.response.success)
            {
                File.WriteAllText(@"C:\BypassKey.txt", textBox1.Text);
                fMain main = new fMain();
                main.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show(KeyAuthApp.response.message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void siticoneRoundedButton1_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
