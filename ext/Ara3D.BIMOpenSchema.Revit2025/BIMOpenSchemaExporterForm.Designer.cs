namespace Ara3D.BIMOpenSchema.Revit2025
{
    partial class BIMOpenSchemaExporterForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BIMOpenSchemaExporterForm));
            exportDirTextBox = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            chooseFolderButton = new System.Windows.Forms.Button();
            linkLabel1 = new System.Windows.Forms.LinkLabel();
            buttonExport = new System.Windows.Forms.Button();
            folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            checkBoxIncludeLinks = new System.Windows.Forms.CheckBox();
            checkBoxMeshGeometry = new System.Windows.Forms.CheckBox();
            buttonLuanchBOSExplorer = new System.Windows.Forms.Button();
            richTextBox1 = new System.Windows.Forms.RichTextBox();
            label2 = new System.Windows.Forms.Label();
            buttonLaunchWindowsExplorer = new System.Windows.Forms.Button();
            comboBoxLod = new System.Windows.Forms.ComboBox();
            SuspendLayout();
            // 
            // exportDirTextBox
            // 
            exportDirTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            exportDirTextBox.Location = new System.Drawing.Point(11, 43);
            exportDirTextBox.Name = "exportDirTextBox";
            exportDirTextBox.Size = new System.Drawing.Size(917, 31);
            exportDirTextBox.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(11, 15);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(140, 25);
            label1.TabIndex = 2;
            label1.Text = "Export Directory";
            // 
            // chooseFolderButton
            // 
            chooseFolderButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            chooseFolderButton.Location = new System.Drawing.Point(715, 98);
            chooseFolderButton.Name = "chooseFolderButton";
            chooseFolderButton.Size = new System.Drawing.Size(214, 45);
            chooseFolderButton.TabIndex = 3;
            chooseFolderButton.Text = "Choose folder ...";
            chooseFolderButton.UseVisualStyleBackColor = true;
            chooseFolderButton.Click += chooseFolderButton_Click;
            // 
            // linkLabel1
            // 
            linkLabel1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            linkLabel1.Location = new System.Drawing.Point(114, 604);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new System.Drawing.Size(706, 25);
            linkLabel1.TabIndex = 6;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "https://github.com/ara3d/bim-open-schema";
            linkLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            linkLabel1.Click += linkLabel1_Click;
            // 
            // buttonExport
            // 
            buttonExport.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            buttonExport.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            buttonExport.Location = new System.Drawing.Point(17, 184);
            buttonExport.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            buttonExport.Name = "buttonExport";
            buttonExport.Size = new System.Drawing.Size(915, 55);
            buttonExport.TabIndex = 8;
            buttonExport.Text = "Run Export";
            buttonExport.UseVisualStyleBackColor = true;
            buttonExport.Click += buttonExport_Click;
            // 
            // checkBoxIncludeLinks
            // 
            checkBoxIncludeLinks.AutoSize = true;
            checkBoxIncludeLinks.Checked = true;
            checkBoxIncludeLinks.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxIncludeLinks.Location = new System.Drawing.Point(16, 98);
            checkBoxIncludeLinks.Name = "checkBoxIncludeLinks";
            checkBoxIncludeLinks.Size = new System.Drawing.Size(241, 29);
            checkBoxIncludeLinks.TabIndex = 9;
            checkBoxIncludeLinks.Text = "Include linked documents";
            checkBoxIncludeLinks.UseVisualStyleBackColor = true;
            // 
            // checkBoxMeshGeometry
            // 
            checkBoxMeshGeometry.AutoSize = true;
            checkBoxMeshGeometry.Checked = true;
            checkBoxMeshGeometry.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxMeshGeometry.Location = new System.Drawing.Point(16, 133);
            checkBoxMeshGeometry.Name = "checkBoxMeshGeometry";
            checkBoxMeshGeometry.Size = new System.Drawing.Size(177, 29);
            checkBoxMeshGeometry.TabIndex = 11;
            checkBoxMeshGeometry.Text = "Include geometry";
            checkBoxMeshGeometry.UseVisualStyleBackColor = true;
            // 
            // buttonLuanchBOSExplorer
            // 
            buttonLuanchBOSExplorer.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            buttonLuanchBOSExplorer.Font = new System.Drawing.Font("Segoe UI", 9F);
            buttonLuanchBOSExplorer.Location = new System.Drawing.Point(61, 539);
            buttonLuanchBOSExplorer.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            buttonLuanchBOSExplorer.Name = "buttonLuanchBOSExplorer";
            buttonLuanchBOSExplorer.Size = new System.Drawing.Size(825, 43);
            buttonLuanchBOSExplorer.TabIndex = 12;
            buttonLuanchBOSExplorer.Text = "Launch BIM Open Schema Explorer";
            buttonLuanchBOSExplorer.UseVisualStyleBackColor = true;
            buttonLuanchBOSExplorer.Click += buttonLaunchBosExplorer_Click;
            // 
            // richTextBox1
            // 
            richTextBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            richTextBox1.Location = new System.Drawing.Point(11, 289);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new System.Drawing.Size(914, 188);
            richTextBox1.TabIndex = 13;
            richTextBox1.Text = "";
            richTextBox1.TextChanged += richTextBox1_TextChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(11, 261);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(46, 25);
            label2.TabIndex = 14;
            label2.Text = "Log:";
            // 
            // buttonLaunchWindowsExplorer
            // 
            buttonLaunchWindowsExplorer.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            buttonLaunchWindowsExplorer.Font = new System.Drawing.Font("Segoe UI", 9F);
            buttonLaunchWindowsExplorer.Location = new System.Drawing.Point(61, 488);
            buttonLaunchWindowsExplorer.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            buttonLaunchWindowsExplorer.Name = "buttonLaunchWindowsExplorer";
            buttonLaunchWindowsExplorer.Size = new System.Drawing.Size(825, 45);
            buttonLaunchWindowsExplorer.TabIndex = 15;
            buttonLaunchWindowsExplorer.Text = "Launch Windows Explorer";
            buttonLaunchWindowsExplorer.UseVisualStyleBackColor = true;
            buttonLaunchWindowsExplorer.Click += buttonLaunchWindowsExplorer_Click;
            // 
            // comboBoxLod
            // 
            comboBoxLod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxLod.FormattingEnabled = true;
            comboBoxLod.Items.AddRange(new object[] { "Coarse Detail", "Medium Detail", "Fine Detail" });
            comboBoxLod.Location = new System.Drawing.Point(199, 133);
            comboBoxLod.Name = "comboBoxLod";
            comboBoxLod.Size = new System.Drawing.Size(269, 33);
            comboBoxLod.TabIndex = 16;
            // 
            // BIMOpenSchemaExporterForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(945, 644);
            Controls.Add(comboBoxLod);
            Controls.Add(buttonLaunchWindowsExplorer);
            Controls.Add(label2);
            Controls.Add(richTextBox1);
            Controls.Add(buttonLuanchBOSExplorer);
            Controls.Add(checkBoxMeshGeometry);
            Controls.Add(checkBoxIncludeLinks);
            Controls.Add(buttonExport);
            Controls.Add(linkLabel1);
            Controls.Add(chooseFolderButton);
            Controls.Add(label1);
            Controls.Add(exportDirTextBox);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MinimumSize = new System.Drawing.Size(488, 314);
            Name = "BIMOpenSchemaExporterForm";
            Text = "BIM Open Schema - Parquet Exporter";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.TextBox exportDirTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button chooseFolderButton;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Button buttonExport;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.CheckBox checkBoxIncludeLinks;
        private System.Windows.Forms.CheckBox checkBoxMeshGeometry;
        private System.Windows.Forms.Button buttonLuanchBOSExplorer;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonLaunchWindowsExplorer;
        private System.Windows.Forms.ComboBox comboBoxLod;
    }
}