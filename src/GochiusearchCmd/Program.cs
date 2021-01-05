using CommandLine;
using Mpga.ImageSearchEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GochiusearchCmd
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = @"ご注文はうさぎですか？ BLOOM 第1羽「にっこりカフェの魔法使い」.mp4";
            ushort titleId = 0;
            ushort episodeId = 1;
            string inputDb = "index.db";
            string outputDb = "index.db";

            var results = Parser.Default.ParseArguments<Options>(args)
            .WithParsed(opt =>
            {
                if(opt.EpisodeId < ushort.MaxValue && opt.TitleId < ushort.MaxValue)
                {
                    titleId = opt.TitleId;
                    episodeId = opt.EpisodeId;
                }
                if (opt.MovieFileName != null)
                {
                    path = opt.MovieFileName;
                }
                if (opt.InputFileName != null)
                {
                    inputDb = opt.InputFileName;
                }
                if(opt.OutputFileName != null)
                {
                    outputDb = opt.OutputFileName;
                }
            });


            ImageInfo[] currentDb = new ImageInfo[] { };
            if (File.Exists(inputDb))
            {
                ImageSearch imageSearch = new ImageSearch();
                currentDb = imageSearch.LoadFromDb(inputDb);
            }
            var count = currentDb.Where(c => c.TitleId == titleId && c.EpisodeId == episodeId).Count();

            if (count > 0)
            {
                Console.WriteLine($"TitleId={titleId}, EpisodeId={episodeId} の組み合わせがすでに {count} 件存在しているため続行できません。");
                return;
            }
            Decoder decoder = new Decoder(path, titleId, episodeId);
            decoder.ProgressChanged += Decoder_ProgressChanged;

            decoder.Completed += Decoder_Completed;
            var outputInfo = decoder.CreateImageInfo();

            // 2つの db を結合(TODO: 挿入ソートにする)
            Array.Resize(ref outputInfo, outputInfo.Length + currentDb.Length);
            currentDb.CopyTo(outputInfo, outputInfo.Length - currentDb.Length);

            decoder.Save(outputDb, outputInfo);
        }

        private static void Decoder_Completed(object sender, ProgressEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private static void Decoder_ProgressChanged(object sender, ProgressEventArgs e)
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write($"{e.ProgressPercentage:f1} %");
        }
    }
}
