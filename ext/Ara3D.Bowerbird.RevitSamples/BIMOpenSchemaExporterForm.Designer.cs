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
            buttonLanchAra3D = new System.Windows.Forms.Button();
            richTextBox1 = new System.Windows.Forms.RichTextBox();
            comboBoxLod = new System.Windows.Forms.ComboBox();
            SuspendLayout();
            // 
            // exportDirTextBox
            // 
            exportDirTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            exportDirTextBox.Location = new System.Drawing.Point(18, 43);
            exportDirTextBox.Name = "exportDirTextBox";
            exportDirTextBox.Size = new System.Drawing.Size(680, 31);
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
            chooseFolderButton.Location = new System.Drawing.Point(715, 27);
            chooseFolderButton.Name = "chooseFolderButton";
            chooseFolderButton.Size = new System.Drawing.Size(214, 47);
            chooseFolderButton.TabIndex = 3;
            chooseFolderButton.Text = "Choose folder ...";
            chooseFolderButton.UseVisualStyleBackColor = true;
            chooseFolderButton.Click += chooseFolderButton_Click;
            // 
            // linkLabel1
            // 
            linkLabel1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            linkLabel1.Location = new System.Drawing.Point(109, 601);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new System.Drawing.Size(702, 25);
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
            buttonExport.Location = new System.Drawing.Point(36, 190);
            buttonExport.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            buttonExport.Name = "buttonExport";
            buttonExport.Size = new System.Drawing.Size(847, 55);
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
            // buttonLanchAra3D
            // 
            buttonLanchAra3D.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            buttonLanchAra3D.Font = new System.Drawing.Font("Segoe UI", 9F);
            buttonLanchAra3D.Location = new System.Drawing.Point(36, 534);
            buttonLanchAra3D.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            buttonLanchAra3D.Name = "buttonLanchAra3D";
            buttonLanchAra3D.Size = new System.Drawing.Size(847, 43);
            buttonLanchAra3D.TabIndex = 12;
            buttonLanchAra3D.Text = "Show in Ara 3D Viewer ...";
            buttonLanchAra3D.UseVisualStyleBackColor = true;
            buttonLanchAra3D.Click += buttonLaunchAra3D_Click;
            // 
            // richTextBox1
            // 
            richTextBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            richTextBox1.Location = new System.Drawing.Point(11, 271);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new System.Drawing.Size(910, 230);
            richTextBox1.TabIndex = 13;
            richTextBox1.Text = "";
            richTextBox1.TextChanged += richTextBox1_TextChanged;
            // 
            // comboBoxLod
            // 
            comboBoxLod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxLod.FormattingEnabled = true;
            comboBoxLod.Items.AddRange(new object[] { "Coarse Detail", "Medium Detail", "Fine Detail" });
            comboBoxLod.Location = new System.Drawing.Point(199, 133);
            comboBoxLod.Name = "comboBoxLod";
            comboBoxLod.Size = new System.Drawing.Size(188, 33);
            comboBoxLod.TabIndex = 16;
            // 
            // BIMOpenSchemaExporterForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(941, 649);
            Controls.Add(comboBoxLod);
            Controls.Add(richTextBox1);
            Controls.Add(buttonLanchAra3D);
            Controls.Add(checkBoxMeshGeometry);
            Controls.Add(checkBoxIncludeLinks);
            Controls.Add(buttonExport);
            Controls.Add(linkLabel1);
            Controls.Add(chooseFolderButton);
            Controls.Add(label1);
            Controls.Add(exportDirTextBox);
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
        private System.Windows.Forms.Button buttonLanchAra3D;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.ComboBox comboBoxLod;
    }
}