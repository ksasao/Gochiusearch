using System;
using Foundation;
using AppKit;
using System.Linq;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using Mpga.ImageSearchEngine;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;

namespace Gochiusearch.Mac
{
    public partial class MainWindowController : NSWindowController
    {
        public MainWindowController(IntPtr handle)
            : base(handle)
        {
        }

        [Export("initWithCoder:")]
        public MainWindowController(NSCoder coder)
            : base(coder)
        {
        }

        public MainWindowController()
            : base("MainWindow")
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
        }

        public new MainWindow Window
        {
            get { return (MainWindow)base.Window; }
        }

        /// <summary>
        /// Shows the alert on this window.
        /// </summary>
        /// <param name="msg">Message.</param>
        /// <param name="title">Title.</param>
        /// <param name="style">Style.</param>
        private void ShowAlertOnWindow(string msg, string title, NSAlertStyle style = NSAlertStyle.Informational)
        {
            var alert = new NSAlert
            {
                InformativeText = msg,
                MessageText = title,
                AlertStyle = style
            };
            alert.RunSheetModal(Window);
        }

        /// <summary>
        /// Clears the present logs.
        /// </summary>
        private void ClearLog()
        {
            var stor = LogField.TextStorage;
            var wholeRange = new NSRange(0, stor.Length);

            stor.BeginEditing();
            stor.DeleteRange(wholeRange);
            stor.EndEditing();
        }

        /// <summary>
        /// Outputs the log.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <param name="attributes">Attributes.</param>
        private void OutputLog(string text, NSStringAttributes attributes = null)
        {
            var textToAdd = attributes == null
                ? new NSAttributedString(text)
                : new NSAttributedString(text, attributes);
            var stor = LogField.TextStorage;
            stor.BeginEditing();
            stor.Append(textToAdd);
            stor.EndEditing();
        }

        public override void WindowDidLoad()
        {
            base.WindowDidLoad();

            InitializeUserInterfaces();

            LoadImageInfo();
            LoadStoryInfo();
        }

        private void InitializeUserInterfaces()
        {
            openNiconicoUrlOnSuccess = false;
            searchLevel = 3;
            // init search levels
            var levelMenu = new NSMenu();
            foreach (var n in Enumerable.Range(0, 11))
            {
                var item = new NSMenuItem(n.ToString());
                item.Activated += (sender, e) =>
                {
                    searchLevel = n;
                    if (!string.IsNullOrEmpty(lastUri))
                        FindImage(lastUri);
                };
                levelMenu.AddItem(item);
            }
            SearchLevelSelector.Menu = levelMenu;
            SearchLevelSelector.SelectItem(3);

            TargetImageView.FileDropped += (sender, e) => FindImage(e.Payload.Path);
            TargetImageView.ImageUrlDropped += (sender, e) => FindImageByUrl(e.Payload.AbsoluteString);
        }

        /// <summary>
        /// Gets or sets the search level.
        /// </summary>
        /// <value>The search level.</value>
        private int searchLevel;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Gochiusearch.Mac.MainWindowController"/> open
        /// niconico URL on finished searching successfully.
        /// </summary>
        /// <value><c>true</c> if open niconico URL on success; otherwise, <c>false</c>.</value>
        private bool openNiconicoUrlOnSuccess;

        partial void OnOpenNiconicoChanged(NSObject sender)
        {
            var btn = sender as NSButton;
            if (btn == null)
                return;
            openNiconicoUrlOnSuccess = btn.State == NSCellStateValue.On;
        }

        /// <summary>
        /// The image search library.
        /// </summary>
        private ImageSearch imageSearch;

        /// <summary>
        /// The story informations.
        /// </summary>
        private List<StoryInfo> stories = new List<StoryInfo>();

        /// <summary>
        /// Loads the image information from index.db.
        /// </summary>
        private void LoadImageInfo()
        {
            var fn = NSBundle.MainBundle.PathForResource("index", "db");
            if (!File.Exists(fn))
            {
                ShowAlertOnWindow("index.db was not found.", "Unable to load", NSAlertStyle.Critical);
                NSApplication.SharedApplication.Terminate(this);
            }

            try
            {
                imageSearch = new ImageSearch();
                imageSearch.LoadFromDb(fn);
            }
            catch
            {
                ShowAlertOnWindow("index.db is corrupted.", "Unable to load", NSAlertStyle.Critical);
                NSApplication.SharedApplication.Terminate(this);
            }
        }

        /// <summary>
        /// Loads the story information from index.txt.
        /// </summary>
        private void LoadStoryInfo()
        {
            var fn = NSBundle.MainBundle.PathForResource("index", "txt");
            if (!File.Exists(fn))
            {
                ShowAlertOnWindow("index.txt was not found.", "Unable to load", NSAlertStyle.Critical);
                NSApplication.SharedApplication.Terminate(this);
            }

            try
            {
                var lines = File.ReadAllLines(fn);
                var storyData = lines
                    .Select(x => x.Split(','))
                    .Where(x => x.Length == 5)
                    .Select(data => new StoryInfo
                    {
                        TitleId = int.Parse(data[0]),
                        EpisodeId = int.Parse(data[1]),
                        FrameRate = float.Parse(data[2]),
                        Title = data[3],
                        Url = data[4]
                    });
                stories.AddRange(storyData);
            }
            catch
            {
                ShowAlertOnWindow("index.txt is corrupted.", "Unable to load", NSAlertStyle.Critical);
                NSApplication.SharedApplication.Terminate(this);
            }
        }

        private void FindImageByUrl(string url)
        {
            if (!IsImage(url))
                return;
            var fn = Path.Combine(NSBundle.MainBundle.ResourcePath, "Browser");
            try
            {
                using (var wc = new WebClient())
                {
                    wc.DownloadFile(url, fn);
                }
            }
            catch
            {
                ShowAlertOnWindow("画像のダウンロードに失敗しました", "エラー", NSAlertStyle.Critical);
                return;
            }
            FindImage(fn);
        }

        private static string[] imageExts = { "jpg", "jpeg", "gif", "png", "bmp" };

        private static bool IsImage(string url)
        {
            url = url.ToLower();
            return imageExts.Any(url.EndsWith);
        }

        /// <summary>
        /// The URI last used to search.
        /// </summary>
        private string lastUri;

        /// <summary>
        /// Finds the related story from image.
        /// </summary>
        /// <param name="file">File.</param>
        private void FindImage(string file)
        {
            lastUri = file;
            ClearLog();
            OutputLog($"検索画像: {file}{Environment.NewLine}");

            var sw = Stopwatch.StartNew();
            var vector = imageSearch.GetVector(file);

            var ret = imageSearch.GetSimilarImage(vector, searchLevel);
            sw.Stop();
            OutputLog($"検索時間: {sw.ElapsedMilliseconds} ms{Environment.NewLine}{Environment.NewLine}");

            if (ret.Length < 1)
            {
                OutputLog("見つかりませんでした。");
                return;
            }

            var results = ret
                .Select(scene =>
                {
                    var titleId = scene[0].TitleId;
                    var episodeId = scene[0].EpisodeId;
                    var storyInfo = stories.First(c => c.TitleId == titleId && c.EpisodeId == episodeId);
                    var title = storyInfo.Title;
                    int second = (int)(1.0 * scene[0].Frame / storyInfo.FrameRate);
                    string time = string.Format("{0}:{1:00}", (int)(second / 60), (second % 60));

                    // ニコニコ動画の頭出し付きリンクを生成
                    second -= 6; // 6秒手前から(動画のキーフレームの位置によりずれる)
                    second = second < 0 ? 0 : second;
                    var url = new NSUrl(storyInfo.Url + "?from=" + second);

                    return new{summary = title + time + "付近",url};
                })
                .ToArray();

            foreach (var result in results)
            {
                OutputLog(result.summary + Environment.NewLine);
                OutputLog(result.url.AbsoluteString + Environment.NewLine, new NSStringAttributes{ LinkUrl = result.url });
            }
            if (openNiconicoUrlOnSuccess)
            {
                NSWorkspace.SharedWorkspace.OpenUrl(results[0].url);
            }
        }
    }
}