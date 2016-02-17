using Mpga.ImageSearchEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mpga.Gochiusearch
{
    public partial class MainForm : Form
    {
        private Process _p = new System.Diagnostics.Process();
        private string _lastFile = "";

        ImageSearchEngine.ImageSearch _iv = null;
        Bitmap _bmpCache = null;
        List<StoryInfo> _story = new List<StoryInfo>();
        string _currentTarget = "";

        public MainForm()
        {
            InitializeComponent();
            InitializeUserComponent();

            LoadStoryInfo();
            LoadImageInfo();
        }

        private void InitializeUserComponent()
        {
            // Drag & Drop の受け入れ設定
            this.richTextBox1.AllowDrop = true;
            this.richTextBox1.DragEnter += Form1_DragEnter;
            this.richTextBox1.DragDrop += Form1_DragDrop;

            // アプリケーションのタイトル表示
            var asm = Assembly.GetExecutingAssembly();
            var name = asm.GetName();
            this.Text = name.Name + " v" + name.Version;

            // 検索レベルの初期化
            List<int> item = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            foreach (var i in item)
            {
                this.comboBox1.Items.Add(i);
            }
            this.comboBox1.SelectedIndex = 3;
        }

        private void LoadImageInfo()
        {
            try
            {
                _iv = new ImageSearch();
                _iv.LoadFromDb("index.db");
            }
            catch
            {
                MessageBox.Show("index.db ファイルの読み込みに失敗しました", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }
        private void LoadStoryInfo()
        {
            try
            {
                string[] lines = File.ReadAllLines("index.txt");
                for (int i = 0; i < lines.Length; i++)
                {
                    string[] data = lines[i].Split(',');
                    if (data.Length == 4)
                    {
                        _story.Add(new StoryInfo
                        {
                            TitleId = Convert.ToInt16(data[0]),
                            EpisodeId = Convert.ToInt16(data[1]),
                            Title = data[2],
                            Url = data[3]
                        });
                    }
                }

            }
            catch
            {
                MessageBox.Show("index.txt ファイルの読み込みに失敗しました", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }


        bool _isUrl = false;
        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            _isUrl = false;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {

                // ドラッグ中のファイルやディレクトリの取得
                string[] drags = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string d in drags)
                {
                    if (!System.IO.File.Exists(d))
                    {
                        // ファイル以外であればイベント・ハンドラを抜ける
                        return;
                    }
                }
                e.Effect = DragDropEffects.Copy;
            }
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                string url = e.Data.GetData("Text") as string;
                if(url == null)
                {
                    return;
                }
                url = url.ToLower();
                if(url.EndsWith("jpg") || url.EndsWith("jpeg")
                    || url.EndsWith("gif") || url.EndsWith("png"))
                {
                    _isUrl = true;
                    e.Effect = DragDropEffects.Copy;
                }
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string file = "";
            if (_isUrl)
            {
                string url = e.Data.GetData("Text") as string;
                using (System.Net.WebClient wc = new System.Net.WebClient())
                {
                    file = "Browser";
                    try
                    {
                        wc.DownloadFile(url, file);
                        _currentTarget = url;
                    }
                    catch
                    {
                        MessageBox.Show("画像のダウンロードに失敗しました", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                file = files[0];
                _currentTarget = file;
            }
            FindImage(file);
        }

        private void FindImage(string file)
        {
            _lastFile = file;
            // ドラッグされた画像を表示
            try
            {
                using (Bitmap queryImage = new Bitmap(file))
                {
                    if (_bmpCache != null)
                    {
                        _bmpCache.Dispose();
                    }
                    // queryImageのファイルをロックしないように
                    // メモリ上に複製＆32bitフォーマット化
                    _bmpCache = new Bitmap(queryImage);
                    this.pictureBox1.Image = _bmpCache;
                }
            }
            catch
            {
                _lastFile = "";
                this.pictureBox1.Image = new Bitmap(1, 1);
            }

            Stopwatch watch = new Stopwatch();

            watch.Start();
            ulong vec = _iv.GetVector(file);
            this.richTextBox1.Text = ""
            + "検索画像: " + _currentTarget + "\r\n";

            List<string> data = new List<string>();
            ImageInfo[][] log = _iv.GetSimilarImage(vec, (int)this.comboBox1.SelectedItem);
            watch.Stop();
            this.richTextBox1.Text += "検索時間: " + watch.ElapsedMilliseconds + " ms\r\n\r\n";

            if (log.Length == 0)
            {
                this.richTextBox1.Text += "見つかりませんでした。\r\n";
                return;
            }

            for(int i = 0; i < log.Length; i++)
            {
                var scene = log[i];
                int titleId = scene[0].TitleId;
                int episodeId = scene[0].EpisodeId;

                var storyInfo = (from c in _story
                         where c.TitleId == titleId && c.EpisodeId == episodeId
                         select c).First();

                string title = storyInfo.Title;
                int second = (int)(1.0 * scene[0].Frame / 29.907);
                string time = string.Format("{0}:{1:00}", (int)(second / 60), (second % 60));
                data.Add(title + time + "付近");

                // ニコニコ動画の頭出し付きリンクを生成
                second -= 6; // 6秒手前から(動画のキーフレームの位置によりずれる)
                second = second < 0 ? 0 : second;
                string url = storyInfo.Url + "?from=" + second;
                data.Add(url);
                if (i == 0)
                {
                    if (this.checkBox1.Checked)
                    {
                        OpenUrl(url);
                    }
                }
            }

            this.richTextBox1.Text += string.Join("\r\n", data.ToArray());
        }

        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            OpenUrl(e.LinkText);
        }
        private void OpenUrl(string url)
        {
            _p = Process.Start(url);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_lastFile != "")
            {
                FindImage(_lastFile);
            }
        }
    }
}
