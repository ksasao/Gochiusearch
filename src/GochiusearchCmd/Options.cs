using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GochiusearchCmd
{
    class Options
    {
        [Option('t', "title", Required = true, HelpText = "titleId", Default = ushort.MaxValue)]
        public ushort TitleId { get; set; }
        [Option('e', "episode", Required = true, HelpText = "episodeId", Default = ushort.MaxValue)]
        public ushort EpisodeId { get; set; }
        [Option('i', "input", Required = false, HelpText = "入力dbファイル名(省略時は index.db)", Default = "index.db")]
        public string InputFileName { get; set; }
        [Option('o', "output", Required = false, HelpText = "出力dbファイル名(省略時は index.db。上書き保存。)", Default = "index.db")]
        public string OutputFileName { get; set; }
        [Option('m', "movie", Required = false, HelpText = "入力動画ファイル名(.mp4, .aviなど)")]
        public string MovieFileName { get; set; }

    }
}
