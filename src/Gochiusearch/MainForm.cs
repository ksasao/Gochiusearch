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
        private string _currentFile = "";
        private string _currentUri = "";
        private string _lastUri = "";

        ImageSearchEngine.ImageSearch _iv = null;
        Bitmap _bmpCache = null;
        List<StoryInfo> _story = new List<StoryInfo>();

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

            // 検索レベル(探索するハミング距離の範囲)の初期化
            // 0はハッシュ値が完全一致(ハミング距離0)の場合
            List<int> item = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            foreach (var i in item)
            {
                this.comboBox1.Items.Add(i);
            }
            this.comboBox1.SelectedIndex = 3;

            // OSを判別しフォントを変更
            var version = Environment.OSVersion.ToString();
            if (version.StartsWith("Unix")) // Mac OS X
            {
                foreach (Control c in this.Controls)
                {
                    var size = c.Font.Size;
                    c.Font = new Font("Hiragino Kaku Gothic ProN", size);
                }
            }
        }

        private void LoadImageInfo()
        {
            try
            {
                _iv = new ImageSearch();
                var cwd = AppDomain.CurrentDomain.BaseDirectory;
                _iv.LoadFromDb(Path.Combine(cwd, "index.db"));
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
                var cwd = AppDomain.CurrentDomain.BaseDirectory;
                string[] lines = File.ReadAllLines(Path.Combine(cwd, "index.txt"));
                for (int i = 0; i < lines.Length; i++)
                {
                    string[] data = lines[i].Split(',');
                    if (data.Length == 5)
                    {
                        _story.Add(new StoryInfo
                            {
                                TitleId = Convert.ToInt16(data[0]),
                                EpisodeId = Convert.ToInt16(data[1]),
                                FrameRate = (float)Convert.ToDouble(data[2]),
                                Title = data[3],
                                Url = data[4]
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
                if (url == null)
                {
                    return;
                }
                url = url.ToLower();
                if (url.EndsWith("jpg") || url.EndsWith("jpeg")
                    || url.EndsWith("gif") || url.EndsWith("png") || url.EndsWith("bmp"))
                {
                    _isUrl = true;
                    e.Effect = DragDropEffects.Copy;
                }
            }
        }

        public virtual void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (_isUrl)
            {
                string url = e.Data.GetData("Text") as string;
                using (System.Net.WebClient wc = new System.Net.WebClient())
                {
                    _currentUri = url;
                    _currentFile = "Browser";
                    try
                    {
                        wc.DownloadFile(url, _currentFile);
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
                _currentFile = files[0];
                _currentUri = _currentFile;
            }
            FindImage(_currentFile);

        }

        private void FindImage(string file)
        {
            if (_lastUri != _currentUri)
            {
                // ドラッグされた画像を表示
                GC.Collect();
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
                        _lastUri = _currentFile;
                    }
                }
                catch
                {
                    _lastUri = "";
                    this.pictureBox1.Image = new Bitmap(1, 1);
                }
            }

            Stopwatch watch = new Stopwatch();

            watch.Start();
            ulong vec = _iv.GetVector(file);
            this.richTextBox1.Text = ""
            + "検索画像: " + _currentUri + "\r\n";

            List<string> data = new List<string>();
            ImageInfo[][] log = _iv.GetSimilarImage(vec, (int)this.comboBox1.SelectedItem);
            watch.Stop();
            this.richTextBox1.Text += "検索時間: " + watch.ElapsedMilliseconds + " ms\r\n\r\n";

            if (log.Length == 0)
            {
                this.richTextBox1.Text += "見つかりませんでした。\r\n";
                return;
            }
        
            for (int i = 0; i < log.Length; i++)
            {
                var scene = log[i];
                int titleId = scene[0].TitleId;
                int episodeId = scene[0].EpisodeId;

                var storyInfo = (from c in _story
                                             where c.TitleId == titleId && c.EpisodeId == episodeId
                                             select c).First();

                string title = storyInfo.Title;
                int second = (int)(1.0 * scene[0].Frame / storyInfo.FrameRate);
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
            if (_lastUri != "")
            {
                FindImage(_currentFile);
            }
        }
    }
}
