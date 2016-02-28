using Mpga.ImageSearchEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateDb
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("usage: createdb index_root_path");
                return;
            }
            Stopwatch watch = new Stopwatch();
            watch.Start();
            try
            {
                CreateDb(args[0]);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            watch.Stop();
            Console.WriteLine("{0:0} sec.", watch.ElapsedMilliseconds / 1000);
        }

        static void CreateDb(string path)
        {
            path = ImageSearch.GetDirectory(path);

            List<string> files = new List<string>();
            FindFiles(path, "*.jpg", files);
            List<ImageInfo> info = new List<ImageInfo>();

            Console.WriteLine("Indexing {0} items...", files.Count);
            string sp = Path.DirectorySeparatorChar.ToString();
            foreach (var f in files)
            {
                string hashStr = f.Substring(path.Length, 3 * 8 - 1).Replace(sp, "");
                string filename = Path.GetFileNameWithoutExtension(f);
                string[] data = filename.Split('_');

                UInt64 hash = Convert.ToUInt64(hashStr, 16);
                UInt16 titleId = Convert.ToUInt16(data[0]);
                UInt16 episodeId = Convert.ToUInt16(data[1]);
                UInt32 frame = Convert.ToUInt32(data[2]);

                info.Add(new ImageInfo
                {
                    Hash = hash,
                    TitleId = titleId,
                    EpisodeId = episodeId,
                    Frame = frame
                });
            }

            Console.WriteLine("Sorting...");
            var sortedInfo = info.OrderBy(c => c.Hash).ThenBy(c => c.TitleId).ThenBy(c => c.EpisodeId).ThenBy(c => c.Frame);
            using (FileStream fs = new FileStream("index.db", FileMode.Create, FileAccess.Write))
            {
                // .gz 形式で圧縮
                using(GZipStream gzipStream = new GZipStream(fs, CompressionMode.Compress))
                {
                    using (BinaryWriter bw = new BinaryWriter(gzipStream))
                    {
                        bw.Write(sortedInfo.Count());
                        foreach (var w in sortedInfo)
                        {
                            bw.Write(w.Hash);
                            bw.Write(w.TitleId);
                            bw.Write(w.EpisodeId);
                            bw.Write(w.Frame);
                        }
                    }
                }
            }
            Console.WriteLine("Finished.");
        }

        static void FindFiles(string dir, string file, List<string> items)
        {
            DirectoryInfo di = new DirectoryInfo(dir);
            try
            {
                DirectoryInfo[] subs = di.GetDirectories();

                foreach (DirectoryInfo sub in subs)
                {
                    FindFiles(sub.FullName, file, items);
                }

                FileInfo[] files = di.GetFiles(file);
                foreach (FileInfo info in files)
                {
                    items.Add(info.FullName);
                }
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}
