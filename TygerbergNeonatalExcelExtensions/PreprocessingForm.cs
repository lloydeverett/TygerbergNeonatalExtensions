using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace TygerbergNeonatalAddin
{
    public partial class PreprocessingForm : Form
    {
        Model model;
        Filter SelectedFilter => IsFilterSelected ? model.Filters[SelectedFilterIndex] : null;
        int SelectedFilterIndex => listBoxFilters.SelectedIndex;
        bool IsFilterSelected => listBoxFilters.SelectedIndex != -1;

        public PreprocessingForm(Model model)
        {
            InitializeComponent();

            this.model = model;
        }

        private void PreprocessingForm_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < (int)(FilterTypes.Count); i++)
            {
                comboBoxFilter.Items.Add(((FilterTypes)(i)).Description());
            }

            foreach (Filter filter in model.Filters)
            {
                listBoxFilters.Items.Add(FormatFilterForListBox(filter));
            }

            UpdateControls();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void UpdateControls()
        {
            buttonMinus.Enabled = IsFilterSelected;
            buttonUp.Enabled = IsFilterSelected;
            buttonDown.Enabled = IsFilterSelected;
            comboBoxFilter.Enabled = IsFilterSelected;
            labelFilterName.Enabled = IsFilterSelected;
            textBoxFilterName.Enabled = IsFilterSelected;
            checkBoxExcludeFromBatchProcessing.Enabled = IsFilterSelected;
            panelExtraOptions.Enabled = IsFilterSelected;

            if (IsFilterSelected)
            {
                SetComboBoxSelectedIndexWithoutFiringEvent((int)(SelectedFilter.FilterType));
                SetFilterNameTextBoxWithoutFiringEvent(SelectedFilter.Name);
                checkBoxExcludeFromBatchProcessing.Checked = SelectedFilter.ExcludeFromBatchProcessing;
            }
            else
            {
                SetComboBoxSelectedIndexWithoutFiringEvent(0);
                SetFilterNameTextBoxWithoutFiringEvent("");
                checkBoxExcludeFromBatchProcessing.Checked = false;
            }

            if (!IsFilterSelected)
            {
                panelExtraOptions.Controls.Clear();
            }
            else
            {
                Control control;
                FilterTypes type = SelectedFilter.FilterType;
                if (type == FilterTypes.Whitelist)
                {
                    control = new WhitelistFilterUserControl((WhitelistFilter)SelectedFilter);
                }
                else if (type == FilterTypes.Blacklist)
                {
                    control = new BlacklistFilterUserControl((BlacklistFilter)SelectedFilter);
                }
                else if (type == FilterTypes.DuplicateRemoval)
                {
                    control = new DuplicateRemovalFilterUserControl((DuplicateRemovalFilter)SelectedFilter);
                }
                else if (type == FilterTypes.Null)
                {
                    control = new NullFilterUserControl((NullFilter)SelectedFilter);
                }
                else if (type == FilterTypes.TimePeriod)
                {
                    control = new TimePeriodFilterUserControl((TimePeriodFilter)SelectedFilter);
                }
                else if (type == FilterTypes.Grouping)
                {
                    control = new GroupingFilterUserControl((GroupingFilter)SelectedFilter);
                }
                else throw new InvalidOperationException();

                panelExtraOptions.Controls.Clear();
                panelExtraOptions.Controls.Add(control);
            }
        }

        private void PreprocessingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
        }
        
        private string FormatFilterForListBox(Filter filter)
        {
            string ret = "";
            ret += (filter.ExcludeFromBatchProcessing ? "[E] " : "");
            ret += (filter.Name == "" ? "<No name>" : filter.Name);
            ret += " (" + filter.FilterType.Description() + ")";
            return ret;
        }

        private void buttonPlus_Click(object sender, EventArgs e)
        {
            Filter filter = new NullFilter("", excludeFromBatchProcessing: false);
            model.Filters.Add(filter);
            listBoxFilters.Items.Add(FormatFilterForListBox(filter));
            listBoxFilters.SelectedIndex = listBoxFilters.Items.Count - 1;
            UpdateControls();
        }

        DialogResult AvoidAccidentalLossOfGroupingDataIfApplicable(string warningMessage)
        {
            if (panelExtraOptions.HasChildren)
            {
                GroupingFilterUserControl groupControl = panelExtraOptions.Controls[0] as GroupingFilterUserControl;
                if (groupControl != null && groupControl.FilterIsConfiguredDifferentlyFromDefault)
                {
                    return MessageBox.Show(warningMessage, "", MessageBoxButtons.YesNo);
                }
            }

            return DialogResult.Yes;
        }

        private void comboBoxFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!IsFilterSelected) return;

            FilterTypes type = (FilterTypes)(comboBoxFilter.SelectedIndex);

            if (type == SelectedFilter.FilterType) return;
            
            if (AvoidAccidentalLossOfGroupingDataIfApplicable("Changing the filter type will discard all of the grouping information.\n\nAre you sure you want to continue?") == DialogResult.No)
            {
                comboBoxFilter.SelectedIndex = (int)(FilterTypes.Grouping);
                return;
            }

            string name = SelectedFilter.Name;
            bool exclude = SelectedFilter.ExcludeFromBatchProcessing;

            Filter replacement;

            if (type == FilterTypes.Whitelist)
            {
                replacement = new WhitelistFilter(name, exclude);
            }
            else if (type == FilterTypes.Blacklist)
            {
                replacement = new BlacklistFilter(name, exclude);
            }
            else if (type == FilterTypes.DuplicateRemoval)
            {
                replacement = new DuplicateRemovalFilter(name, exclude);
            }
            else if (type == FilterTypes.Null)
            {
                replacement = new NullFilter(name, exclude);
            }
            else if (type == FilterTypes.TimePeriod)
            {
                replacement = new TimePeriodFilter(name, exclude);
            }
            else if (type == FilterTypes.Grouping)
            {
                replacement = new GroupingFilter(name, exclude);
            }
            else throw new InvalidOperationException();

            model.Filters[SelectedFilterIndex] = replacement;

            ChangeListBoxValueWithoutFiringEvent(SelectedFilterIndex, FormatFilterForListBox(SelectedFilter));

            UpdateControls();
        }

        private void buttonMinus_Click(object sender, EventArgs e)
        {
            if (AvoidAccidentalLossOfGroupingDataIfApplicable("Removing this filter will discard all of of its grouping information.\n\nAre you sure you want to continue?") == DialogResult.No)
            {
                return;
            }

            model.Filters.RemoveAt(SelectedFilterIndex);
            listBoxFilters.Items.RemoveAt(SelectedFilterIndex);
        }

        private void textBoxFilterName_TextChanged(object sender, EventArgs e)
        {     
            if (!IsFilterSelected) return;

            SelectedFilter.Name = textBoxFilterName.Text;
            ChangeListBoxValueWithoutFiringEvent(SelectedFilterIndex, FormatFilterForListBox(SelectedFilter));
        }

        private void checkBoxExcludeFromBatchProcessing_CheckedChanged(object sender, EventArgs e)
        {
            if (!IsFilterSelected) return;
            SelectedFilter.ExcludeFromBatchProcessing = checkBoxExcludeFromBatchProcessing.Checked;
            ChangeListBoxValueWithoutFiringEvent(SelectedFilterIndex, FormatFilterForListBox(SelectedFilter));
        }

        void SetFilterNameTextBoxWithoutFiringEvent(string newText)
        {
            textBoxFilterName.TextChanged -= textBoxFilterName_TextChanged;
            textBoxFilterName.Text = newText;
            textBoxFilterName.TextChanged += textBoxFilterName_TextChanged;
        }

        void SetComboBoxSelectedIndexWithoutFiringEvent(int newValue)
        {
            comboBoxFilter.TextChanged -= comboBoxFilter_SelectedIndexChanged;
            comboBoxFilter.SelectedIndex = newValue;
            comboBoxFilter.TextChanged += comboBoxFilter_SelectedIndexChanged;
        }

        void ChangeListBoxValueWithoutFiringEvent(int index, string newValue)
        {
            listBoxFilters.SelectedIndexChanged -= listBox1_SelectedIndexChanged;
            listBoxFilters.Items[index] = newValue;
            listBoxFilters.SelectedIndexChanged += listBox1_SelectedIndexChanged;
        }

        void ChangeListBoxSelectedIndexWithoutFiringEvent(int newSelectionIndex)
        {
            listBoxFilters.SelectedIndexChanged -= listBox1_SelectedIndexChanged;
            listBoxFilters.SelectedIndex = newSelectionIndex;
            listBoxFilters.SelectedIndexChanged += listBox1_SelectedIndexChanged;
        }

        private void Swap(int indexA, int indexB)
        {
            Filter filterA = model.Filters[indexA];
            Filter filterB = model.Filters[indexB];

            model.Filters[indexA] = filterB;
            model.Filters[indexB] = filterA;

            int desiredSelectionIndex;
            if (SelectedFilterIndex == indexA)
            {
                desiredSelectionIndex = indexB;
            }
            else if (SelectedFilterIndex == indexB)
            {
                desiredSelectionIndex = indexA;
            }
            else
            {
                desiredSelectionIndex = SelectedFilterIndex;
            }

            ChangeListBoxValueWithoutFiringEvent(indexA, FormatFilterForListBox(filterB));
            ChangeListBoxValueWithoutFiringEvent(indexB, FormatFilterForListBox(filterA));
            ChangeListBoxSelectedIndexWithoutFiringEvent(desiredSelectionIndex);
        }

        private void buttonUp_Click(object sender, EventArgs e)
        {
            if (SelectedFilterIndex == 0 || SelectedFilterIndex == -1) return;
            Swap(SelectedFilterIndex, SelectedFilterIndex - 1);
        }

        private void buttonDown_Click(object sender, EventArgs e)
        {
            if (SelectedFilterIndex == listBoxFilters.Items.Count - 1 || SelectedFilterIndex == -1) return;
            Swap(SelectedFilterIndex, SelectedFilterIndex + 1);
        }
    }
}
