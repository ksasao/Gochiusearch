using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mpga.Gochiusearch
{
    public partial class SettingsForm : Form
    {
        Properties.Settings _settings = Properties.Settings.Default;
        FileNameConverter fnc = new FileNameConverter();

        public string Rule { get { return fnc.Rule; } set { fnc.Rule = value; } }
        public bool CopyFile { get { return _settings.CopyFile; } set { _settings.CopyFile = value; } }
        public string GetNewFilename(string filename, string title, TimeSpan time)
        {
            return fnc.GetNewFilename(filename, title, time);
        }
        public SettingsForm()
        {
            InitializeComponent();
            Initialize();
        }

        /// <summary>
        /// 現在の設定を保存します
        /// </summary>
        public void SaveSettings()
        {
            _settings.Rule = Rule;
            _settings.Save();
        }
        /// <summary>
        /// 設定を読み込みます
        /// </summary>
        public void LoadSettings()
        {
            _settings.Reload();
            Initialize();
        }
        private void Initialize()
        {
            Rule = _settings.Rule;
            this.checkBoxEnableRename.Checked = CopyFile;
            this.textBoxRule.Text = Rule;
            UpdateExample();
        }

        private void textBoxRule_TextChanged(object sender, EventArgs e)
        {
            UpdateExample();
        }
        private void UpdateExample()
        {
            string filename = @"C:\Users\ksasao\Desktop\vlcsnap-00129.png";
            Rule = this.textBoxRule.Text;
            string result = fnc.GetNewFilename(filename, "ご注文はうさぎですか？第2羽", new TimeSpan(0, 15, 20));
            this.textBoxExample.Text = result;
            this.textBoxRule.Text = Rule; // ファイル名として利用できない文字は置換される
        }

        private void checkBoxEnableRename_CheckedChanged(object sender, EventArgs e)
        {
            CopyFile = this.checkBoxEnableRename.Checked;
        }
    }
}
