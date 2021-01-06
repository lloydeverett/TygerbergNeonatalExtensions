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
    public partial class ErrorListForm : Form
    {
        public ErrorListForm(string explanation, IEnumerable<string> detailedErrors)
        {
            InitializeComponent();

            label1.Text = explanation;
            textBox1.Text = string.Join("\r\n", detailedErrors);
        }

        private void ErrorListForm_Load(object sender, EventArgs e)
        {

        }

        public static void Show(string explanation, IEnumerable<string> detailedErrors)
        {
            ErrorListForm form = new ErrorListForm(explanation, detailedErrors);
            form.ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
