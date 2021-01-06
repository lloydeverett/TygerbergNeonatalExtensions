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
    public partial class AnalysisForm : Form
    {
        FindClustersTransformation findClustersTransformation;

        public AnalysisForm(Model model)
        {
            InitializeComponent();
            
            findClustersTransformation = model.FindClustersTransformation;
            textBox1.Text = findClustersTransformation.DateColumnHeader;
            numericUpDown1.Value = findClustersTransformation.MinimumNumberOfInstancesPerCluster;
            numericUpDown2.Value = (decimal)findClustersTransformation.MaximumAdjacentSpan.TotalDays;
            textBox2.Text = UserInterfaceUtil.TextBoxContentFromValues(findClustersTransformation.ClusterDefiningColumnHeaders);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            findClustersTransformation.DateColumnHeader = textBox1.Text;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            findClustersTransformation.MinimumNumberOfInstancesPerCluster = (int)(numericUpDown1.Value);
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            findClustersTransformation.MaximumAdjacentSpan = TimeSpan.FromDays((double)numericUpDown2.Value);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            findClustersTransformation.ClusterDefiningColumnHeaders = UserInterfaceUtil.ValuesFromTextBoxContent(textBox2.Text);
        }
    }
}
