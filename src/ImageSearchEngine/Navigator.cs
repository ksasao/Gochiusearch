using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mpga.ImageSearchEngine
{
    public class Navigator
    {
        /// <summary>
        /// 表示名
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// 再生時刻のオフセット(秒)
        /// </summary>
        public int Offset { get; private set; }
        /// <summary>
        /// シーク時のURL書式
        /// </summary>
        public string SeekFormat { get; private set; }
        public string IndexTextPath { get; private set; }
        public StoryInfo[] Stories { get; private set; }
        public Navigator(string indexTextPath)
        {
            IndexTextPath = indexTextPath;
            LoadStoryInfo(indexTextPath);
        }
        public string GetTimeString(StoryInfo storyInfo, uint frame, int offsetSecond, string format)
        {
            int second = GetTotalSecond(storyInfo, frame) + offsetSecond;
            second = second > 0 ? second : 0;

            string sss = second.ToString();
            string s = (second % 60).ToString();
            string ss = string.Format("{0:00}", second % 60);
            string m = (second / 60).ToString();

            string result = format;
            result = result.Replace("<sss>", sss);
            result = result.Replace("<ss>", ss);
            result = result.Replace("<s>", s);
            result = result.Replace("<m>", m);
            return result;
        }
        public string GetSeekUrl(StoryInfo storyInfo, uint frame)
        {
            string postUrl = GetTimeString(storyInfo, frame, Offset, SeekFormat);
            return storyInfo.Url + postUrl;
        }
        public int GetTotalSecond(StoryInfo storyInfo, uint frame)
        {
            return (int)(1.0 * frame / storyInfo.FrameRate);
        }
        private void LoadStoryInfo(string path)
        {
            var cwd = AppDomain.CurrentDomain.BaseDirectory;
            string[] lines = File.ReadAllLines(Path.Combine(cwd, path));
            List<StoryInfo> info = new List<StoryInfo>();

            // ヘッダ読み込み
            string[] header = lines[0].Split(',');
            Name = header[0];
            Offset = Convert.ToInt32(header[1]);
            SeekFormat = header[2];

            // 各話データ読み込み
            for (int i = 1; i< lines.Length; i++)
            {
                string line = lines[i];
                string[] data = line.Split(',');
                if (data.Length == 5)
                {
                    info.Add(new StoryInfo
                    {
                        TitleId = Convert.ToInt16(data[0]),
                        EpisodeId = Convert.ToInt16(data[1]),
                        FrameRate = (float)Convert.ToDouble(data[2]),
                        Title = data[3],
                        Url = data[4]
                    });
                }
            }
            Stories = info.ToArray();
        }
    }
}
