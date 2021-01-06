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
    public partial class BlacklistFilterUserControl : UserControl
    {
        BlacklistFilter filter;

        public BlacklistFilterUserControl(BlacklistFilter filter)
        {
            InitializeComponent();

            this.filter = filter;

            textBox1.Text = filter.ColumnHeader;
            textBox2.Text = UserInterfaceUtil.TextBoxContentFromValues(filter.DisallowedValues);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            filter.ColumnHeader = textBox1.Text;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            filter.DisallowedValues = UserInterfaceUtil.ValuesFromTextBoxContent(textBox2.Text);
        }
    }
}
