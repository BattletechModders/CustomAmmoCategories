namespace SoundBanksPostprocessor {
  partial class MainForm {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.button1 = new System.Windows.Forms.Button();
      this.tbFile = new System.Windows.Forms.TextBox();
      this.button2 = new System.Windows.Forms.Button();
      this.label1 = new System.Windows.Forms.Label();
      this.tbKey = new System.Windows.Forms.TextBox();
      this.tbIV = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.button3 = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // button1
      // 
      this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button1.Location = new System.Drawing.Point(618, 168);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(75, 23);
      this.button1.TabIndex = 0;
      this.button1.Text = "process";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // tbFile
      // 
      this.tbFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbFile.Location = new System.Drawing.Point(5, 26);
      this.tbFile.Name = "tbFile";
      this.tbFile.Size = new System.Drawing.Size(659, 20);
      this.tbFile.TabIndex = 1;
      // 
      // button2
      // 
      this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.button2.Location = new System.Drawing.Point(670, 26);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(23, 20);
      this.button2.TabIndex = 2;
      this.button2.Text = ">";
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(2, 9);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(20, 13);
      this.label1.TabIndex = 3;
      this.label1.Text = "file";
      // 
      // tbKey
      // 
      this.tbKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbKey.Location = new System.Drawing.Point(5, 66);
      this.tbKey.Name = "tbKey";
      this.tbKey.Size = new System.Drawing.Size(659, 20);
      this.tbKey.TabIndex = 4;
      // 
      // tbIV
      // 
      this.tbIV.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbIV.Location = new System.Drawing.Point(5, 93);
      this.tbIV.Name = "tbIV";
      this.tbIV.Size = new System.Drawing.Size(659, 20);
      this.tbIV.TabIndex = 5;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(2, 50);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(82, 13);
      this.label2.TabIndex = 6;
      this.label2.Text = "Process params";
      // 
      // openFileDialog
      // 
      this.openFileDialog.FileName = "openFileDialog";
      this.openFileDialog.Filter = "soundbank|*.bnk";
      // 
      // button3
      // 
      this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.button3.Location = new System.Drawing.Point(5, 168);
      this.button3.Name = "button3";
      this.button3.Size = new System.Drawing.Size(75, 23);
      this.button3.TabIndex = 7;
      this.button3.Text = "unprocess";
      this.button3.UseVisualStyleBackColor = true;
      this.button3.Click += new System.EventHandler(this.button3_Click);
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(697, 194);
      this.Controls.Add(this.button3);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.tbIV);
      this.Controls.Add(this.tbKey);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.tbFile);
      this.Controls.Add(this.button1);
      this.Name = "MainForm";
      this.Text = "BT soundbanks post processor";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.TextBox tbFile;
    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox tbKey;
    private System.Windows.Forms.TextBox tbIV;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.OpenFileDialog openFileDialog;
    private System.Windows.Forms.Button button3;
  }
}

