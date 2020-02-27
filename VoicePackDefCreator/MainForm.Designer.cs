namespace VoicePackDefCreator {
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
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.button = new System.Windows.Forms.Button();
      this.checkBox = new System.Windows.Forms.CheckBox();
      this.SuspendLayout();
      // 
      // openFileDialog
      // 
      this.openFileDialog.Filter = "Txt files|*.txt";
      // 
      // button
      // 
      this.button.Location = new System.Drawing.Point(161, 0);
      this.button.Name = "button";
      this.button.Size = new System.Drawing.Size(75, 23);
      this.button.TabIndex = 0;
      this.button.Text = "open txt";
      this.button.UseVisualStyleBackColor = true;
      this.button.Click += new System.EventHandler(this.button_Click);
      // 
      // checkBox
      // 
      this.checkBox.AutoSize = true;
      this.checkBox.Location = new System.Drawing.Point(12, 4);
      this.checkBox.Name = "checkBox";
      this.checkBox.Size = new System.Drawing.Size(134, 17);
      this.checkBox.TabIndex = 1;
      this.checkBox.Text = "create voice pack also";
      this.checkBox.UseVisualStyleBackColor = true;
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(238, 22);
      this.Controls.Add(this.checkBox);
      this.Controls.Add(this.button);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "MainForm";
      this.ShowIcon = false;
      this.Text = "VoicePackDefCreator";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion
    private System.Windows.Forms.OpenFileDialog openFileDialog;
    private System.Windows.Forms.Button button;
    private System.Windows.Forms.CheckBox checkBox;
  }
}

