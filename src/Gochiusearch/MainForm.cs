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
using System.Text.RegularExpressions;
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
        private int _cacheImageCount = 0;

        private ImageSearchEngine.ImageSearch _iv = null;
        private Bitmap _bmpCache = null;
        private List<StoryInfo> _story = new List<StoryInfo>();

        private const string _tempBrowserImage = "Browser";
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
                MessageBox.Show("index.db is corrupted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }

        private void LoadStoryInfo()
        {
            try
            {
                var cwd = AppDomain.CurrentDomain.BaseDirectory;
                string[] lines = File.ReadAllLines(Path.Combine(cwd, "index.txt"));
                foreach (string line in lines)
                {
                    string[] data = line.Split(',');
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
                MessageBox.Show("index.txt is corrupted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }


        private bool _isUrl = false;

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            _isUrl = false;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else if (e.Data.GetDataPresent(DataFormats.Text))
            {
                _isUrl = true;
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        public virtual void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string b64image = GetBase64ImageFile(e.Data);
            if (b64image != null)
            {
                _isUrl = false;
                _currentFile = b64image;
                _currentUri = _currentFile;
            }
            else if (_isUrl)
            {
                string url = GetImageUrl(e.Data);
                if (string.IsNullOrEmpty(url))
                {
                    return;
                }
                using (System.Net.WebClient wc = new System.Net.WebClient())
                {
                    _currentUri = url;
                    _currentFile = Path.Combine(Path.GetTempPath(),_tempBrowserImage);
                    try
                    {
                        wc.DownloadFile(url, _currentFile);
                        using (Bitmap bmp = new Bitmap(_currentFile)) { } // 画像ファイルとして開けることを確認
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
                if (!File.Exists(files[0]) || !IsImage(files[0]))
                {
                    return;
                }
                _currentFile = files[0];
                _currentUri = _currentFile;
            }
            FindImage(_currentFile);
        }

        private string GetBase64ImageFile(IDataObject obj)
        {
            string html = obj.GetData("HTML Format") as string;
            if(html == null)
            {
                return null;
            }
            Regex b64 = new Regex("<img .*?src=\\\"data:image/([\\w]*?);base64,([\\w/\\+=]*?)\\\"");
            Match m = b64.Match(html);
            if (m.Success && m.Groups.Count > 2)
            {
                byte[] img = Convert.FromBase64String(m.Groups[2].Value);
                string filename = Path.Combine(Path.GetTempPath(),$"image{_cacheImageCount}." + m.Groups[1].Value);
                _cacheImageCount = (_cacheImageCount+1) % 10;
                File.WriteAllBytes(filename, img);
                return filename;
            }
            return null;
        }
        private string GetImageAsFile(Image image)
        {
            if(image == null)
            {
                return null;
            }
            using(Bitmap bmp = new Bitmap(image))
            {
                string filename = Path.GetTempFileName();
                bmp.Save(filename);
                return filename;
            }
        }

        private readonly string[] regList = {
            "\\\"(https://pbs.twimg.com/media/[\\w- ./?%&=]*?)\\\"",
            "\\\"(https://[\\w-]*?.gstatic.com/[\\w- ./?%&=:]*?)\\\"",
            "<img .*?src=\\\"(http(s)?://[\\w- ./?%&=:]*?)\\\""
        };


        private string GetImageUrl(IDataObject obj)
        {
            //string[] formats = obj.GetFormats();
            //for(int i=0; i < formats.Length; i++)
            //{
            //    string data = obj.GetData(formats[i]) as string;
            //}

            string result = null;
            string html = obj.GetData("HTML Format") as string;
            string text = obj.GetData("Text") as string;
            if (html != null)
            {
                html = html.Replace("amp;", "");
                for(int i=0; i<regList.Length; i++)
                {
                    Regex reg = new Regex(regList[i]);
                    Match match = reg.Match(html);
                    if (match.Success && match.Groups.Count > 1)
                    {
                        result = match.Groups[1].Value;
                        return result;
                    }
                }

            }

            if (text != null && IsImage(text))
            {
                result = text;
            }
            return result;
        }

        private static bool IsImage(string url)
        {
            url = url.ToLower();
            return url.EndsWith("jpg") || url.EndsWith("jpeg") ||
                   url.EndsWith("gif") || url.EndsWith("png") ||
                   url.EndsWith("bmp");
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
            string target = _currentFile == _tempBrowserImage ? _currentUri : _currentFile;
            this.richTextBox1.Text = string.Format("検索画像: {0}{1}", target, Environment.NewLine);

            List<string> data = new List<string>();
            ImageInfo[][] log = _iv.GetSimilarImage(vec, (int)this.comboBox1.SelectedItem);
            watch.Stop();
            this.richTextBox1.Text += string.Format("検索時間: {0} ms{1}{1}", watch.ElapsedMilliseconds, Environment.NewLine);

            if (log.Length == 0)
            {
                this.richTextBox1.Text += "見つかりませんでした。" + Environment.NewLine;
                return;
            }

            for (int i = 0; i < log.Length; i++)
            {
                var scene = log[i];
                int titleId = scene[0].TitleId;
                int episodeId = scene[0].EpisodeId;

                var storyInfo = _story.First(c => c.TitleId == titleId && c.EpisodeId == episodeId);

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

            this.richTextBox1.Text += string.Join(Environment.NewLine, data.ToArray());
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


        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.V | Keys.Control) && Clipboard.ContainsImage())
            {
                string filename = GetImageAsFile(Clipboard.GetImage());
                if(filename == null)
                {
                    return;
                }
                _isUrl = false;
                _currentFile = filename;
                _currentUri = _currentFile;
                FindImage(_currentFile);
            }
        }
    }
}
