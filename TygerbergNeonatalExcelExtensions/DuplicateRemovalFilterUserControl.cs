using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TygerbergNeonatalAddin
{
    public partial class DuplicateRemovalFilterUserControl : UserControl
    {
        DuplicateRemovalFilter filter;

        public DuplicateRemovalFilterUserControl(DuplicateRemovalFilter filter)
        {
            InitializeComponent();

            this.filter = filter;

            textBox1.Text = filter.ColumnHeader;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            filter.ColumnHeader = textBox1.Text;
        }
    }
}
