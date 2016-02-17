using Mpga.ImageSearchEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateIndex
{
    class Program
    {
        static ImageSearch _iv = null;

        static void Main(string[] args)
        {
            if(args.Length < 2)
            {
                Console.WriteLine("usage: createindex input_path output_path");
                return;
            }
            _iv = new ImageSearch(args[1]);
            var files = SetFiles(args[0]);
            Console.WriteLine("Finished.");
        }

        static private int SetFiles(string path)
        {
            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(path, "*.png", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(path, "*.jpg", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(path, "*.gif", SearchOption.AllDirectories));
            Console.WriteLine(files.Count + " files.");
            int count = 0;
            foreach (var f in files)
            {
                var fi = new FileInfo(f);
                if (fi.Length > 0)
                {
                    string img = _iv.AddImage(f);
                    
                    count++;
                    if (count % 1000 == 0)
                    {
                        Console.WriteLine("\r {0}/{1} {2}", count, files.Count, f);
                    }
                }

            }
            return files.Count;
        }
    }
}
