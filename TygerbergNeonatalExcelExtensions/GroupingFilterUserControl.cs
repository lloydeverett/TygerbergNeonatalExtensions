using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.Diagnostics;


namespace TygerbergNeonatalAddin
{
    public partial class GroupingFilterUserControl : UserControl
    {
        int newItemIndex = -1;
        GroupingFilter filter;

        // todo deal with inconsistencies

        // todo have the option for input/export

        public bool FilterIsConfiguredDifferentlyFromDefault => filter.MembersDictionary.Count > 0;

        public GroupingFilterUserControl(GroupingFilter filter)
        {
            InitializeComponent();

            this.filter = filter;

            textBox1.Text = filter.ColumnHeader;

            comboBox2.Items.Add("<Show error if cannot group>");

            foreach (string group in filter.MembersDictionary.Keys)
            {
                listView1.Items.Add(group);
                comboBox2.Items.Add(group);
                UpdateComboBoxEnabledStatus();
            }
            
            if (filter.GroupForMembersThatCannotBeGrouped == null)
            {
                comboBox2.SelectedIndex = 0;
            }
            else
            {
                comboBox2.SelectedIndex = (from object item in comboBox2.Items select comboBox2.GetItemText(item)).ToList()
                    .FindIndex(text => text == filter.GroupForMembersThatCannotBeGrouped);
            }

            DoSelectedFilterChangedUpdate(null);
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            DoSelectedFilterChangedUpdate();
        }

        private void DoSelectedFilterChangedUpdate()
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                DoSelectedFilterChangedUpdate(assumeSelectedFilterIs: listView1.SelectedItems[0].Text);
            }
            else
            {
                DoSelectedFilterChangedUpdate(assumeSelectedFilterIs: null);
            }
        }

        private void DoSelectedFilterChangedUpdate(string assumeSelectedFilterIs)
        {
            if (assumeSelectedFilterIs == null)
            {
                button2.Enabled = false;
                textBox2.Enabled = false;
                ChangeTextBox2TextWithoutFiringChangedEvent("");
            }
            else
            {
                button2.Enabled = true;
                textBox2.Enabled = true;
                ChangeTextBox2TextWithoutFiringChangedEvent(UserInterfaceUtil.TextBoxContentFromValues(filter.MembersDictionary[assumeSelectedFilterIs]));

                // todo I guess check inconsistencies here...?
                // todo also in text box change
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            newItemIndex = listView1.Items.Count;
            listView1.SelectedIndexChanged -= listView1_SelectedIndexChanged;
            var item = listView1.Items.Add("");
            newItemIndex = item.Index;
            item.BeginEdit();
            listView1.SelectedIndexChanged += listView1_SelectedIndexChanged;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string selectionText = listView1.SelectedItems[0].Text;
            if (comboBox2.SelectedIndex == listView1.SelectedIndices[0] + 1)
            {
                comboBox2.SelectedIndex = 0;
            }
            comboBox2.Items.RemoveAt(listView1.SelectedIndices[0] + 1);
            listView1.Items.RemoveAt(listView1.SelectedIndices[0]);
            UpdateComboBoxEnabledStatus();
            filter.MembersDictionary.Remove(selectionText);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            filter.ColumnHeader = textBox1.Text;
        }

        private void listView1_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            string oldText = listView1.Items[e.Item].Text;
            string newText = e.Label;
            bool isNewItem = newItemIndex == e.Item;
            newItemIndex = -1;
            if (newText == null)
            {
                newText = oldText;
                listView1.Items[e.Item].Text = oldText;
            }

            for (int i = 0; i < listView1.Items.Count; i++)
            {
                if (i == e.Item) continue;

                if (newText == listView1.Items[i].Text)
                {
                    MessageBox.Show("Another group has this name already.");

                    e.CancelEdit = true;
                    if (isNewItem) newItemIndex = e.Item;
                    listView1.Items[e.Item].BeginEdit();
                    return;
                }
            }

            if (isNewItem)
            {
                filter.MembersDictionary[newText] = new List<string>();
            }
            else
            {
                List<string> members = filter.MembersDictionary[oldText];
                filter.MembersDictionary.Remove(oldText);
                filter.MembersDictionary[newText] = members;
            }

            string assumeSelectedFilterIs;
            if (listView1.SelectedItems.Count == 0) assumeSelectedFilterIs = null;
            else if (listView1.SelectedItems[0] == listView1.Items[e.Item]) assumeSelectedFilterIs = newText;
            else assumeSelectedFilterIs = listView1.Items[e.Item].Text;
            DoSelectedFilterChangedUpdate(assumeSelectedFilterIs);

            if (isNewItem)
            {
                comboBox2.Items.Add(newText);
                UpdateComboBoxEnabledStatus();
            }
            else
            {
                comboBox2.Items[e.Item + 1] = newText;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            filter.MembersDictionary[listView1.SelectedItems[0].Text] = UserInterfaceUtil.ValuesFromTextBoxContent(textBox2.Text);
        }

        private void ChangeTextBox2TextWithoutFiringChangedEvent(string newValue)
        {
            textBox2.TextChanged -= textBox2_TextChanged;
            textBox2.Text = newValue;
            textBox2.TextChanged += textBox2_TextChanged;
        }

        private void ChangeListView1SelectedIndexWithoutFiringChangedEvent(int newValue)
        {
            listView1.SelectedIndexChanged -= listView1_SelectedIndexChanged;
            listView1.SelectedItems.Clear();
            listView1.Items[newValue].Selected = true;
            listView1.Update();
            listView1.SelectedIndexChanged += listView1_SelectedIndexChanged;
        }
        
        private void UpdateComboBoxEnabledStatus()
        {
            comboBox2.Enabled = comboBox2.Items.Count > 1;
        }

        private void GroupingFilterUserControl_Load(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex == 0)
            {
                filter.GroupForMembersThatCannotBeGrouped = null;
            }
            else
            {
                filter.GroupForMembersThatCannotBeGrouped = comboBox2.GetItemText(comboBox2.SelectedItem);
            }
        }
    }
}
