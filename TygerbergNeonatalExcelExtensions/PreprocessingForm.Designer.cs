namespace TygerbergNeonatalAddin
{
    partial class PreprocessingForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.listBoxFilters = new System.Windows.Forms.ListBox();
            this.buttonPlus = new System.Windows.Forms.Button();
            this.buttonMinus = new System.Windows.Forms.Button();
            this.buttonUp = new System.Windows.Forms.Button();
            this.buttonDown = new System.Windows.Forms.Button();
            this.comboBoxFilter = new System.Windows.Forms.ComboBox();
            this.checkBoxExcludeFromBatchProcessing = new System.Windows.Forms.CheckBox();
            this.panelExtraOptions = new System.Windows.Forms.Panel();
            this.textBoxFilterName = new System.Windows.Forms.TextBox();
            this.labelFilterName = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // listBoxFilters
            // 
            this.listBoxFilters.FormattingEnabled = true;
            this.listBoxFilters.Location = new System.Drawing.Point(12, 12);
            this.listBoxFilters.Name = "listBoxFilters";
            this.listBoxFilters.Size = new System.Drawing.Size(186, 407);
            this.listBoxFilters.TabIndex = 0;
            this.listBoxFilters.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // buttonPlus
            // 
            this.buttonPlus.Location = new System.Drawing.Point(12, 431);
            this.buttonPlus.Name = "buttonPlus";
            this.buttonPlus.Size = new System.Drawing.Size(23, 23);
            this.buttonPlus.TabIndex = 1;
            this.buttonPlus.Text = "+";
            this.buttonPlus.UseVisualStyleBackColor = true;
            this.buttonPlus.Click += new System.EventHandler(this.buttonPlus_Click);
            // 
            // buttonMinus
            // 
            this.buttonMinus.Location = new System.Drawing.Point(41, 431);
            this.buttonMinus.Name = "buttonMinus";
            this.buttonMinus.Size = new System.Drawing.Size(23, 23);
            this.buttonMinus.TabIndex = 2;
            this.buttonMinus.Text = "-";
            this.buttonMinus.UseVisualStyleBackColor = true;
            this.buttonMinus.Click += new System.EventHandler(this.buttonMinus_Click);
            // 
            // buttonUp
            // 
            this.buttonUp.Location = new System.Drawing.Point(88, 431);
            this.buttonUp.Name = "buttonUp";
            this.buttonUp.Size = new System.Drawing.Size(52, 23);
            this.buttonUp.TabIndex = 3;
            this.buttonUp.Text = "Up";
            this.buttonUp.UseVisualStyleBackColor = true;
            this.buttonUp.Click += new System.EventHandler(this.buttonUp_Click);
            // 
            // buttonDown
            // 
            this.buttonDown.Location = new System.Drawing.Point(146, 431);
            this.buttonDown.Name = "buttonDown";
            this.buttonDown.Size = new System.Drawing.Size(52, 23);
            this.buttonDown.TabIndex = 4;
            this.buttonDown.Text = "Down";
            this.buttonDown.UseVisualStyleBackColor = true;
            this.buttonDown.Click += new System.EventHandler(this.buttonDown_Click);
            // 
            // comboBoxFilter
            // 
            this.comboBoxFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFilter.FormattingEnabled = true;
            this.comboBoxFilter.Location = new System.Drawing.Point(204, 12);
            this.comboBoxFilter.Name = "comboBoxFilter";
            this.comboBoxFilter.Size = new System.Drawing.Size(404, 21);
            this.comboBoxFilter.TabIndex = 7;
            this.comboBoxFilter.SelectedIndexChanged += new System.EventHandler(this.comboBoxFilter_SelectedIndexChanged);
            // 
            // checkBoxExcludeFromBatchProcessing
            // 
            this.checkBoxExcludeFromBatchProcessing.AutoSize = true;
            this.checkBoxExcludeFromBatchProcessing.Location = new System.Drawing.Point(207, 65);
            this.checkBoxExcludeFromBatchProcessing.Name = "checkBoxExcludeFromBatchProcessing";
            this.checkBoxExcludeFromBatchProcessing.Size = new System.Drawing.Size(212, 17);
            this.checkBoxExcludeFromBatchProcessing.TabIndex = 9;
            this.checkBoxExcludeFromBatchProcessing.Text = "Exclude this filter from batch processing";
            this.checkBoxExcludeFromBatchProcessing.UseVisualStyleBackColor = true;
            this.checkBoxExcludeFromBatchProcessing.CheckedChanged += new System.EventHandler(this.checkBoxExcludeFromBatchProcessing_CheckedChanged);
            // 
            // panelExtraOptions
            // 
            this.panelExtraOptions.Location = new System.Drawing.Point(204, 88);
            this.panelExtraOptions.Name = "panelExtraOptions";
            this.panelExtraOptions.Size = new System.Drawing.Size(404, 366);
            this.panelExtraOptions.TabIndex = 10;
            // 
            // textBoxFilterName
            // 
            this.textBoxFilterName.Location = new System.Drawing.Point(268, 39);
            this.textBoxFilterName.Name = "textBoxFilterName";
            this.textBoxFilterName.Size = new System.Drawing.Size(340, 20);
            this.textBoxFilterName.TabIndex = 11;
            this.textBoxFilterName.TextChanged += new System.EventHandler(this.textBoxFilterName_TextChanged);
            // 
            // labelFilterName
            // 
            this.labelFilterName.AutoSize = true;
            this.labelFilterName.Location = new System.Drawing.Point(204, 42);
            this.labelFilterName.Name = "labelFilterName";
            this.labelFilterName.Size = new System.Drawing.Size(58, 13);
            this.labelFilterName.TabIndex = 12;
            this.labelFilterName.Text = "Filter name";
            // 
            // PreprocessingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(620, 466);
            this.Controls.Add(this.labelFilterName);
            this.Controls.Add(this.textBoxFilterName);
            this.Controls.Add(this.panelExtraOptions);
            this.Controls.Add(this.buttonDown);
            this.Controls.Add(this.checkBoxExcludeFromBatchProcessing);
            this.Controls.Add(this.comboBoxFilter);
            this.Controls.Add(this.buttonUp);
            this.Controls.Add(this.buttonMinus);
            this.Controls.Add(this.buttonPlus);
            this.Controls.Add(this.listBoxFilters);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "PreprocessingForm";
            this.Text = "Filter configuration";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PreprocessingForm_FormClosing);
            this.Load += new System.EventHandler(this.PreprocessingForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxFilters;
        private System.Windows.Forms.Button buttonPlus;
        private System.Windows.Forms.Button buttonMinus;
        private System.Windows.Forms.Button buttonUp;
        private System.Windows.Forms.Button buttonDown;
        private System.Windows.Forms.ComboBox comboBoxFilter;
        private System.Windows.Forms.CheckBox checkBoxExcludeFromBatchProcessing;
        private System.Windows.Forms.Panel panelExtraOptions;
        private System.Windows.Forms.TextBox textBoxFilterName;
        private System.Windows.Forms.Label labelFilterName;
    }
}