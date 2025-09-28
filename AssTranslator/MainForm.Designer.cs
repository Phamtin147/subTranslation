namespace AssTranslator
{
    partial class MainForm
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
            this.lblApiKey = new System.Windows.Forms.Label();
            this.txtApiKey = new System.Windows.Forms.TextBox();
            this.lblModel = new System.Windows.Forms.Label();
            this.cboModel = new System.Windows.Forms.ComboBox();
            this.lblLanguage = new System.Windows.Forms.Label();
            this.cboLanguage = new System.Windows.Forms.ComboBox();
            this.lblInputFile = new System.Windows.Forms.Label();
            this.txtInputFile = new System.Windows.Forms.TextBox();
            this.btnBrowseInput = new System.Windows.Forms.Button();
            this.lblOutputFile = new System.Windows.Forms.Label();
            this.txtOutputFile = new System.Windows.Forms.TextBox();
            this.btnBrowseOutput = new System.Windows.Forms.Button();
            this.lblCustomPrompt = new System.Windows.Forms.Label();
            this.txtCustomPrompt = new System.Windows.Forms.TextBox();
            this.btnBrowseCustomPrompt = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.btnTranslate = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblApiKey
            // 
            this.lblApiKey.AutoSize = true;
            this.lblApiKey.Location = new System.Drawing.Point(12, 15);
            this.lblApiKey.Name = "lblApiKey";
            this.lblApiKey.Size = new System.Drawing.Size(48, 13);
            this.lblApiKey.TabIndex = 0;
            this.lblApiKey.Text = "API Key:";
            // 
            // txtApiKey
            // 
            this.txtApiKey.Location = new System.Drawing.Point(150, 12);
            this.txtApiKey.Name = "txtApiKey";
            this.txtApiKey.PasswordChar = '*';
            this.txtApiKey.Size = new System.Drawing.Size(420, 20);
            this.txtApiKey.TabIndex = 1;
            this.txtApiKey.TextChanged += new System.EventHandler(this.txtApiKey_TextChanged);
            // 
            // lblModel
            // 
            this.lblModel.AutoSize = true;
            this.lblModel.Location = new System.Drawing.Point(12, 55);
            this.lblModel.Name = "lblModel";
            this.lblModel.Size = new System.Drawing.Size(39, 13);
            this.lblModel.TabIndex = 2;
            this.lblModel.Text = "Model:";
            // 
            // cboModel
            // 
            this.cboModel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboModel.FormattingEnabled = true;
            this.cboModel.Location = new System.Drawing.Point(150, 52);
            this.cboModel.Name = "cboModel";
            this.cboModel.Size = new System.Drawing.Size(420, 21);
            this.cboModel.TabIndex = 3;
            this.cboModel.SelectedIndexChanged += new System.EventHandler(this.cboModel_SelectedIndexChanged);
            // 
            // lblLanguage
            // 
            this.lblLanguage.AutoSize = true;
            this.lblLanguage.Location = new System.Drawing.Point(12, 95);
            this.lblLanguage.Name = "lblLanguage";
            this.lblLanguage.Size = new System.Drawing.Size(58, 13);
            this.lblLanguage.TabIndex = 4;
            this.lblLanguage.Text = "Language:";
            // 
            // cboLanguage
            // 
            this.cboLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLanguage.FormattingEnabled = true;
            this.cboLanguage.Location = new System.Drawing.Point(150, 92);
            this.cboLanguage.Name = "cboLanguage";
            this.cboLanguage.Size = new System.Drawing.Size(420, 21);
            this.cboLanguage.TabIndex = 5;
            this.cboLanguage.SelectedIndexChanged += new System.EventHandler(this.cboLanguage_SelectedIndexChanged);
            // 
            // lblInputFile
            // 
            this.lblInputFile.AutoSize = true;
            this.lblInputFile.Location = new System.Drawing.Point(12, 135);
            this.lblInputFile.Name = "lblInputFile";
            this.lblInputFile.Size = new System.Drawing.Size(53, 13);
            this.lblInputFile.TabIndex = 6;
            this.lblInputFile.Text = "Input File:";
            // 
            // txtInputFile
            // 
            this.txtInputFile.Location = new System.Drawing.Point(150, 132);
            this.txtInputFile.Name = "txtInputFile";
            this.txtInputFile.ReadOnly = true;
            this.txtInputFile.Size = new System.Drawing.Size(341, 20);
            this.txtInputFile.TabIndex = 7;
            // 
            // btnBrowseInput
            // 
            this.btnBrowseInput.Location = new System.Drawing.Point(497, 132);
            this.btnBrowseInput.Name = "btnBrowseInput";
            this.btnBrowseInput.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseInput.TabIndex = 8;
            this.btnBrowseInput.Text = "Browse...";
            this.btnBrowseInput.UseVisualStyleBackColor = true;
            this.btnBrowseInput.Click += new System.EventHandler(this.btnBrowseInput_Click);
            // 
            // lblOutputFile
            // 
            this.lblOutputFile.AutoSize = true;
            this.lblOutputFile.Location = new System.Drawing.Point(12, 175);
            this.lblOutputFile.Name = "lblOutputFile";
            this.lblOutputFile.Size = new System.Drawing.Size(61, 13);
            this.lblOutputFile.TabIndex = 9;
            this.lblOutputFile.Text = "Output File:";
            // 
            // txtOutputFile
            // 
            this.txtOutputFile.Location = new System.Drawing.Point(150, 172);
            this.txtOutputFile.Name = "txtOutputFile";
            this.txtOutputFile.ReadOnly = true;
            this.txtOutputFile.Size = new System.Drawing.Size(341, 20);
            this.txtOutputFile.TabIndex = 10;
            // 
            // btnBrowseOutput
            // 
            this.btnBrowseOutput.Location = new System.Drawing.Point(497, 172);
            this.btnBrowseOutput.Name = "btnBrowseOutput";
            this.btnBrowseOutput.Size = new System.Drawing.Size(75, 27);
            this.btnBrowseOutput.TabIndex = 11;
            this.btnBrowseOutput.Text = "Browse...";
            this.btnBrowseOutput.UseVisualStyleBackColor = true;
            this.btnBrowseOutput.Click += new System.EventHandler(this.btnBrowseOutput_Click);
            // 
            // lblCustomPrompt
            // 
            this.lblCustomPrompt.AutoSize = true;
            this.lblCustomPrompt.Location = new System.Drawing.Point(12, 215);
            this.lblCustomPrompt.Name = "lblCustomPrompt";
            this.lblCustomPrompt.Size = new System.Drawing.Size(85, 13);
            this.lblCustomPrompt.TabIndex = 12;
            this.lblCustomPrompt.Text = "Custom Prompt:";
            // 
            // txtCustomPrompt
            // 
            this.txtCustomPrompt.Location = new System.Drawing.Point(150, 212);
            this.txtCustomPrompt.Name = "txtCustomPrompt";
            this.txtCustomPrompt.ReadOnly = true;
            this.txtCustomPrompt.Size = new System.Drawing.Size(341, 20);
            this.txtCustomPrompt.TabIndex = 13;
            // 
            // btnBrowseCustomPrompt
            // 
            this.btnBrowseCustomPrompt.Location = new System.Drawing.Point(497, 212);
            this.btnBrowseCustomPrompt.Name = "btnBrowseCustomPrompt";
            this.btnBrowseCustomPrompt.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseCustomPrompt.TabIndex = 14;
            this.btnBrowseCustomPrompt.Text = "Browse...";
            this.btnBrowseCustomPrompt.UseVisualStyleBackColor = true;
            this.btnBrowseCustomPrompt.Click += new System.EventHandler(this.btnBrowseCustomPrompt_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(15, 255);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(555, 23);
            this.progressBar.TabIndex = 15;
            this.progressBar.Visible = false;
            // 
            // btnTranslate
            // 
            this.btnTranslate.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTranslate.Location = new System.Drawing.Point(15, 295);
            this.btnTranslate.Name = "btnTranslate";
            this.btnTranslate.Size = new System.Drawing.Size(555, 40);
            this.btnTranslate.TabIndex = 16;
            this.btnTranslate.Text = "Translate";
            this.btnTranslate.UseVisualStyleBackColor = true;
            this.btnTranslate.Click += new System.EventHandler(this.btnTranslate_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 350);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(37, 13);
            this.lblStatus.TabIndex = 17;
            this.lblStatus.Text = "Status";
            this.lblStatus.Visible = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(582, 380);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnTranslate);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.btnBrowseCustomPrompt);
            this.Controls.Add(this.txtCustomPrompt);
            this.Controls.Add(this.lblCustomPrompt);
            this.Controls.Add(this.btnBrowseOutput);
            this.Controls.Add(this.txtOutputFile);
            this.Controls.Add(this.lblOutputFile);
            this.Controls.Add(this.btnBrowseInput);
            this.Controls.Add(this.txtInputFile);
            this.Controls.Add(this.lblInputFile);
            this.Controls.Add(this.cboLanguage);
            this.Controls.Add(this.lblLanguage);
            this.Controls.Add(this.cboModel);
            this.Controls.Add(this.lblModel);
            this.Controls.Add(this.txtApiKey);
            this.Controls.Add(this.lblApiKey);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ASS Subtitle Translator";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblApiKey;
        private System.Windows.Forms.TextBox txtApiKey;
        private System.Windows.Forms.Label lblModel;
        private System.Windows.Forms.ComboBox cboModel;
        private System.Windows.Forms.Label lblLanguage;
        private System.Windows.Forms.ComboBox cboLanguage;
        private System.Windows.Forms.Label lblInputFile;
        private System.Windows.Forms.TextBox txtInputFile;
        private System.Windows.Forms.Button btnBrowseInput;
        private System.Windows.Forms.Label lblOutputFile;
        private System.Windows.Forms.TextBox txtOutputFile;
        private System.Windows.Forms.Button btnBrowseOutput;
        private System.Windows.Forms.Label lblCustomPrompt;
        private System.Windows.Forms.TextBox txtCustomPrompt;
        private System.Windows.Forms.Button btnBrowseCustomPrompt;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Button btnTranslate;
        private System.Windows.Forms.Label lblStatus;
    }
}