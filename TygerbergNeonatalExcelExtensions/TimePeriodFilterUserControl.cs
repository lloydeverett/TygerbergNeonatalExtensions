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
    public partial class TimePeriodFilterUserControl : UserControl
    {
        TimePeriodFilter filter;

        public TimePeriodFilterUserControl(TimePeriodFilter filter)
        {
            InitializeComponent();

            this.filter = filter;

            if (TimeSpan.FromDays(filter.MaximumAdjacentSpan.Days) != filter.MaximumAdjacentSpan)
            {
                throw new InvalidOperationException("Max adjacent span is not an integer number of days.");
            }

            textBox2.Text = filter.DateColumnHeader;
            textBox3.Text = UserInterfaceUtil.TextBoxContentFromValues(filter.ClusterDefiningColumnHeaders);
            textBox1.Text = filter.MaximumAdjacentSpan.Days.ToString();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            filter.DateColumnHeader = textBox2.Text;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (Int32.TryParse(textBox1.Text, out value))
            {
                textBox1.BackColor = Color.White;
                filter.MaximumAdjacentSpan = TimeSpan.FromDays(value);
            }
            else
            {
                textBox1.BackColor = Color.DarkRed;
                filter.MaximumAdjacentSpan = TimeSpan.FromDays(7);
            }
        }

        private void textBox3_TextChanged_1(object sender, EventArgs e)
        {
            filter.ClusterDefiningColumnHeaders = UserInterfaceUtil.ValuesFromTextBoxContent(textBox3.Text);
        }
    }
}
