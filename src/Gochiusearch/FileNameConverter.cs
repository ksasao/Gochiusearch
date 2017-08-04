using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mpga.Gochiusearch
{
    public class FileNameConverter
    {
        Dictionary<string, string> _keywords;
        string _rule;
        const string DefaultRule = "$(fullpath).$(title).$(mm)m$(ss)s.$(ext)";
        /// <summary>
        /// ファイル名の変換ルール
        /// </summary>
        public string Rule { get { return _rule; } set { SetRule(value); } }
        public bool CopyFile { get; set; }

        public FileNameConverter()
        {
            Initialize();
        }
        private void SetRule(string rule)
        {
            rule = rule.Trim();
            // ファイル名として使用できない文字を置換
            rule = rule.Replace('/', '／');
            rule = rule.Replace(':', '：');
            rule = rule.Replace('*', '＊');
            rule = rule.Replace('?', '？');
            rule = rule.Replace('"', '”');
            rule = rule.Replace('<', '＜');
            rule = rule.Replace('>', '＞');
            rule = rule.Replace('|', '｜');
            _rule = rule;
        }
        private void Initialize()
        {
            Rule = DefaultRule;
            _keywords = new Dictionary<string, string>();

            foreach (string name in Enum.GetNames(typeof(Environment.SpecialFolder)))
            {
                var val = (Environment.SpecialFolder)Enum.Parse(typeof(Environment.SpecialFolder), name);
                var path = System.Environment.GetFolderPath(val);
                _keywords.Add(name, path);
            }

        }

        /// <summary>
        /// 変換ルールに基づきファイル名を変更します。
        /// </summary>
        /// <param name="targetFileName">変換元のファイルのフルパス</param>
        /// <returns>変換後のファイルのフルパス</returns>
        public string GetNewFilename(string targetFileName,string title, TimeSpan time)
        {
            string fullpath = Path.GetFullPath(targetFileName);
            string filename = Path.GetFileName(fullpath);
            string ext = Path.GetExtension(fullpath);
            string h = time.TotalHours.ToString("0");
            string hh = time.Hours.ToString("00");
            string m = time.Minutes.ToString();
            string mm = time.Minutes.ToString("00");
            string mmm = time.TotalMinutes.ToString("000");
            string mmmm = time.TotalMinutes.ToString("0000");
            string s = time.Seconds.ToString();
            string ss = time.Seconds.ToString("00");
            string sss = time.TotalSeconds.ToString("000");
            string ssss = time.TotalSeconds.ToString("0000");
            string sssss = time.TotalSeconds.ToString("00000");

            Dictionary<string, string> current = new Dictionary<string, string>();
            current.Add("fullpath", fullpath);
            current.Add("filename", filename);
            current.Add("ext", ext);
            current.Add("h", hh);
            current.Add("hh", hh);
            current.Add("m", m);
            current.Add("mm", mm);
            current.Add("mmm", mmm);
            current.Add("mmmm", mmmm);
            current.Add("s", s);
            current.Add("ss", ss);
            current.Add("sss", sss);
            current.Add("ssss", ssss);
            current.Add("sssss", sssss);
            current.Add("title", title);

            string result = Rule;
            MatchCollection results = Regex.Matches(result, @"\$\((.+?)\)");
            foreach (Match mat in results)
            {
                string value = mat.Groups[1].Value;
                if (_keywords.ContainsKey(value))
                {
                    result = result.Replace(mat.Groups[0].Value, _keywords[value]);
                }
                if (current.ContainsKey(value))
                {
                    result = result.Replace(mat.Groups[0].Value, current[value]);
                }
            }
            return result;
        }
    }
}
