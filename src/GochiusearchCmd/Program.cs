using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GochiusearchCmd
{
    class Program
    {
        static void Main(string[] args)
        {
            // とりあえず動作
            string path = @"ご注文はうさぎですか？ BLOOM\ご注文はうさぎですか？ BLOOM 第1羽「にっこりカフェの魔法使い」.mp4";
            Decoder decoder = new Decoder(path, 0, 0);
            var info = decoder.CreateImageInfo();
            decoder.Save("index.db", info);
        }
    }
}
