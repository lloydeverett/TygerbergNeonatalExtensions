using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TygerbergNeonatalAddin
{
    public partial class SelectCellsForm : Form
    {
        bool okClicked = false;

        public SelectCellsForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            okClicked = true;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void SelectCellsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult = okClicked ? DialogResult.OK : DialogResult.Cancel;

        }
    }
}
