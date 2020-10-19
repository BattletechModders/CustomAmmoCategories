using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;

namespace SoundBanksPostprocessor {
  public partial class MainForm : Form {
    Aes aes;
    public MainForm() {
      InitializeComponent();
      aes = Aes.Create();
      aes.Mode = CipherMode.CBC;
      aes.KeySize = 256;
      aes.BlockSize = 128;
      aes.FeedbackSize = 128;
      aes.Padding = PaddingMode.PKCS7;
      aes.GenerateIV();
      aes.GenerateKey();
      tbKey.Text = Convert.ToBase64String(aes.Key);
      tbIV.Text = Convert.ToBase64String(aes.IV);
    }

    private void button2_Click(object sender, EventArgs e) {
      if (this.openFileDialog.ShowDialog() == DialogResult.OK) {
        tbFile.Text = openFileDialog.FileName;
      }
    }

    private void button1_Click(object sender, EventArgs e) {
      if (File.Exists(openFileDialog.FileName) == false) { return; }
      aes.Key = Convert.FromBase64String(tbKey.Text);
      aes.IV = Convert.FromBase64String(tbIV.Text);
      ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
      byte[] Data = File.ReadAllBytes(this.openFileDialog.FileName);
      byte[] result = null;
      using (MemoryStream msEncrypt = new MemoryStream()) {
        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)) {
          csEncrypt.Write(Data,0,Data.Length);
        }
        result = msEncrypt.ToArray();
      }
      string dir = Path.GetDirectoryName(this.openFileDialog.FileName);
      string name = Path.GetFileNameWithoutExtension(this.openFileDialog.FileName) + "_processed";
      string ext = Path.GetExtension(this.openFileDialog.FileName);
      string procFilename = Path.Combine(dir, name + ext);
      File.WriteAllBytes(procFilename, result);
      string keys = "{\"param1\":\"" + Convert.ToBase64String(aes.Key) + "\",\"param2\":\"" + Convert.ToBase64String(aes.IV) + "\"}";
      File.WriteAllText(Path.Combine(dir, name + ".key"), keys);
      MessageBox.Show("Success");
    }

    private void button3_Click(object sender, EventArgs e) {
      if (File.Exists(openFileDialog.FileName) == false) { return; }
      aes.Key = Convert.FromBase64String(tbKey.Text);
      aes.IV = Convert.FromBase64String(tbIV.Text);
      ICryptoTransform encryptor = aes.CreateDecryptor(aes.Key, aes.IV);
      byte[] Data = File.ReadAllBytes(this.openFileDialog.FileName);
      byte[] result = null;
      using (MemoryStream msEncrypt = new MemoryStream()) {
        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)) {
          csEncrypt.Write(Data, 0, Data.Length);
        }
        result = msEncrypt.ToArray();
      }
      string dir = Path.GetDirectoryName(this.openFileDialog.FileName);
      string name = Path.GetFileNameWithoutExtension(this.openFileDialog.FileName) + "_unprocessed";
      string ext = Path.GetExtension(this.openFileDialog.FileName);
      string procFilename = Path.Combine(dir, name + ext);
      File.WriteAllBytes(procFilename, result);
      MessageBox.Show("Success");
    }
  }
}
