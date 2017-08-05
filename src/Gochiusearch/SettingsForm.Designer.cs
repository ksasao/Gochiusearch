namespace Mpga.Gochiusearch
{
    partial class SettingsForm
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.groupBoxRename = new System.Windows.Forms.GroupBox();
            this.textBoxExample = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxRule = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxEnableRename = new System.Windows.Forms.CheckBox();
            this.panel1.SuspendLayout();
            this.groupBoxRename.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.buttonCancel);
            this.panel1.Controls.Add(this.buttonOk);
            this.panel1.Controls.Add(this.groupBoxRename);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1118, 288);
            this.panel1.TabIndex = 0;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(979, 241);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(129, 35);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "キャンセル";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(867, 241);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(97, 35);
            this.buttonOk.TabIndex = 1;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            // 
            // groupBoxRename
            // 
            this.groupBoxRename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxRename.Controls.Add(this.textBoxExample);
            this.groupBoxRename.Controls.Add(this.label2);
            this.groupBoxRename.Controls.Add(this.textBoxRule);
            this.groupBoxRename.Controls.Add(this.label1);
            this.groupBoxRename.Controls.Add(this.checkBoxEnableRename);
            this.groupBoxRename.Location = new System.Drawing.Point(12, 12);
            this.groupBoxRename.Name = "groupBoxRename";
            this.groupBoxRename.Size = new System.Drawing.Size(1094, 197);
            this.groupBoxRename.TabIndex = 0;
            this.groupBoxRename.TabStop = false;
            this.groupBoxRename.Text = "ファイル名";
            // 
            // textBoxExample
            // 
            this.textBoxExample.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxExample.BackColor = System.Drawing.SystemColors.Control;
            this.textBoxExample.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxExample.Location = new System.Drawing.Point(117, 102);
            this.textBoxExample.Multiline = true;
            this.textBoxExample.Name = "textBoxExample";
            this.textBoxExample.Size = new System.Drawing.Size(961, 61);
            this.textBoxExample.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 102);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 18);
            this.label2.TabIndex = 3;
            this.label2.Text = "変換例：";
            // 
            // textBoxRule
            // 
            this.textBoxRule.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxRule.Location = new System.Drawing.Point(117, 65);
            this.textBoxRule.Name = "textBoxRule";
            this.textBoxRule.Size = new System.Drawing.Size(961, 25);
            this.textBoxRule.TabIndex = 2;
            this.textBoxRule.Text = "* Settings.settings の Rule で設定 *";
            this.textBoxRule.TextChanged += new System.EventHandler(this.textBoxRule_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 68);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 18);
            this.label1.TabIndex = 1;
            this.label1.Text = "変換ルール：";
            // 
            // checkBoxEnableRename
            // 
            this.checkBoxEnableRename.AutoSize = true;
            this.checkBoxEnableRename.Location = new System.Drawing.Point(16, 25);
            this.checkBoxEnableRename.Name = "checkBoxEnableRename";
            this.checkBoxEnableRename.Size = new System.Drawing.Size(446, 22);
            this.checkBoxEnableRename.TabIndex = 0;
            this.checkBoxEnableRename.Text = "ドラッグ＆ドロップされたファイルのコピーを自動的に保存する";
            this.checkBoxEnableRename.UseVisualStyleBackColor = true;
            this.checkBoxEnableRename.CheckedChanged += new System.EventHandler(this.checkBoxEnableRename_CheckedChanged);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1118, 288);
            this.Controls.Add(this.panel1);
            this.Name = "SettingsForm";
            this.Text = "設定";
            this.panel1.ResumeLayout(false);
            this.groupBoxRename.ResumeLayout(false);
            this.groupBoxRename.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBoxRename;
        private System.Windows.Forms.TextBox textBoxRule;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBoxEnableRename;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.TextBox textBoxExample;
        private System.Windows.Forms.Label label2;
    }
}