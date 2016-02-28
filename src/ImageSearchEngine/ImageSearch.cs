using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Mpga.ImageSearchEngine
{
    public class ImageSearch
    {
        string _basePath;
        ImageInfo[] _info;

        public object StopWatch { get; private set; }

        public ImageSearch(string basePath)
        {
            _basePath = ImageSearch.GetDirectory(basePath);
        }
        public ImageSearch(ImageInfo[] info)
        {
            _info = info;
        }
        public ImageSearch()
        {

        }

        public ImageInfo[] LoadFromDb(string dbFileName)
        {
            using (FileStream fs = new FileStream(dbFileName, FileMode.Open, FileAccess.Read))
            {
                using (GZipStream gz = new GZipStream(fs, CompressionMode.Decompress))
                {
                    using (BinaryReader br = new BinaryReader(gz))
                    {
                        long count = br.ReadInt32();
                        _info = new ImageInfo[count];

                        for (int i = 0; i < count; i++)
                        {
                            ulong hash = br.ReadUInt64();
                            UInt16 titleId = br.ReadUInt16();
                            UInt16 episodeId = br.ReadUInt16();
                            UInt32 frame = br.ReadUInt32();
                            _info[i] = new ImageInfo
                            {
                                Hash = hash,
                                TitleId = titleId,
                                EpisodeId = episodeId,
                                Frame = frame
                            };
                        }
                    }

                }
            }
            return _info;
        }

        /// <summary>
        /// 画像ファイルから類似画像が近い値を持つようなハッシュ値を計算します
        /// </summary>
        /// <param name="filename">画像ファイル</param>
        /// <returns>ハッシュ値</returns>
        public ulong GetVector(string filename)
        {
            // dHash (difference hash) を計算
            // http://www.hackerfactor.com/blog/?/archives/529-Kind-of-Like-That.html

            // 入力した画像を 9x8 の領域にスケールする
            Bitmap bmp = new Bitmap(filename);
            byte[] data = GetSmallImageData(bmp, 9, 8);
            bmp.Dispose();

            // モノクロ化
            int[] mono = new int[data.Length / 4];
            for (int i = 0; i < mono.Length; i++)
            {
                mono[i] = 29 * data[i * 4] + 150 * data[i * 4 + 1] + 77 * data[i * 4 + 2];
            }

            // 横方向で隣接するピクセル間の輝度差をビットベクトルとする
            ulong result = 0;
            int p = 0;
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    result = (result << 1) | (mono[p] > mono[p + 1] ? (uint)1 : 0);
                    p++;
                }
                p++;
            }
            return result;
        }

        private byte[] GetSmallImageData(Bitmap bmp, int width, int height)
        {
            // 入力画像は32bitARGBではないことがあるので
            // 以下の処理の簡素化のため32bitARGBに統一
            byte[] bmp32Data;
            using (Bitmap bmp32 = new Bitmap(bmp.Width, bmp.Height))
            {
                using (Graphics g = Graphics.FromImage(bmp32))
                {
                    g.DrawImage(bmp,
                        new Rectangle(0, 0, bmp32.Width, bmp32.Height),
                        new Rectangle(0, 0, bmp.Width, bmp.Height),
                        GraphicsUnit.Pixel);
                }
                bmp32Data = BitmapToByteArray(bmp32);
            }

            // 等間隔にサンプリングして平均をとる
            byte[] result = new byte[width * height * 4];
            int s = 12; // s*s = サンプリング数
            int pos = 0;
            for (int y = 0; y < height; y++)
            {
                for(int x = 0; x < width; x++)
                {
                    int srcX0 = x * bmp.Width / width;
                    int srcY0 = y * bmp.Height / height;

                    int r = 0;
                    int g = 0;
                    int b = 0;
                    int a = 0;

                    // 縮小した画素に対して縮小元から s * s 画素を取得し
                    // 平均値を計算
                    for (int yy = 0; yy < s; yy++)
                    {
                        for (int xx = 0; xx < s; xx++)
                        {
                            int dx = xx * bmp.Width / width / s;
                            int dy = yy * bmp.Height / height / s;
                            int p = ((srcX0 + dx) + (srcY0 + dy) * bmp.Width) * 4;
                            b += bmp32Data[p];
                            g += bmp32Data[p + 1];
                            r += bmp32Data[p + 2];
                            a += bmp32Data[p + 3];
                        }
                    }

                    result[pos++] = (byte)(b / s / s);
                    result[pos++] = (byte)(g / s / s);
                    result[pos++] = (byte)(r / s / s);
                    result[pos++] = (byte)(a / s / s);
                }
            }
            return result;
        }

        /// <summary>
        /// 類似画像を検索します
        /// </summary>
        /// <param name="vec">検索対象の画像</param>
        /// <param name="level">類似度(0が最も厳密)</param>
        /// <returns>連続する画像毎にまとめられた画像情報</returns>
        public ImageInfo[][] GetSimilarImage(ulong vec,int level)
        {
            List<ImageInfo> info = new List<ImageInfo>();
            if(level <= 3)
            {
                GetSimilarImage(vec, level, info); // 二分探索
            }
            else
            {
                GetSimilarImage2(vec, level, info); // popcntで全件検索
            }
            return GroupByScene(info);
        }


        private void GetSimilarImage2(ulong vec, int level, List<ImageInfo> info)
        {
            // 全件検索
            for(int i=0; i<_info.Length; i++)
            {
                if(PopulationCount(vec ^ _info[i].Hash) <= level)
                {
                    info.Add(_info[i]);
                }
            }
        }

        // 1となっているビットの数を数える
        private int PopulationCount(ulong bits)
        {
            bits = (bits & 0x5555555555555555) + (bits >> 1 & 0x5555555555555555);
            bits = (bits & 0x3333333333333333) + (bits >> 2 & 0x3333333333333333);
            bits = (bits & 0x0f0f0f0f0f0f0f0f) + (bits >> 4 & 0x0f0f0f0f0f0f0f0f);
            bits = (bits & 0x00ff00ff00ff00ff) + (bits >> 8 & 0x00ff00ff00ff00ff);
            bits = (bits & 0x0000ffff0000ffff) + (bits >> 16 & 0x0000ffff0000ffff);
            bits = (bits & 0x00000000ffffffff) + (bits >> 32 & 0x00000000ffffffff);
            return (int)bits;
        }

        private void GetSimilarImage(ulong vec, int level, List<ImageInfo> info)
        {
            // 完全一致のみ
            if (level >= 0)
            {
                var p = GetImageInfo(vec);
                if (p.Length > 0)
                {
                    info.AddRange(p);
                }
            }
            // ハミング距離1の探索
            if (level >= 1)
            {
                for (int i = 0; i < 1 * 64; i++)
                {
                    ulong v = vec ^ (ulong)(1UL << i);
                    var p = GetImageInfo(v);
                    if (p.Length > 0)
                    {
                        info.AddRange(p);
                    }

                }
            }
            // ハミング距離2の探索
            if (level >= 2)
            {
                for (int i = 0; i < 63; i++)
                {
                    for (int j = i + 1; j < 64; j++)
                    {
                        ulong v = vec ^ (ulong)(1UL << i) ^ (ulong)(1UL << j);
                        var p = GetImageInfo(v);
                        if (p.Length > 0)
                        {
                            info.AddRange(p);
                        }
                    }
                }
            }

            // ハミング距離3の探索
            if (level >= 3)
            {
                for (int i = 0; i < 62; i++)
                {
                    for (int j = i + 1; j < 63; j++)
                    {
                        for (int k = j + 1; k < 64; k++)
                        {
                            ulong v = vec ^ (ulong)(1UL << i) ^ (ulong)(1UL << j) ^ (ulong)(1UL << k);
                            var p = GetImageInfo(v);
                            if (p.Length > 0)
                            {
                                info.AddRange(p);
                            }
                        }
                    }
                }
            }

            // ハミング距離4の探索
            if (level >= 4)
            {
                for (int i = 0; i < 61; i++)
                {
                    for (int j = i + 1; j < 62; j++)
                    {
                        for (int k = j + 1; k < 63; k++)
                        {
                            for (int l = k + 1; l < 64; l++)
                            {
                                ulong v = vec ^ (ulong)(1UL << i) ^ (ulong)(1UL << j) ^ (ulong)(1UL << k) ^ (ulong)(1UL << l);
                                var p = GetImageInfo(v);
                                if (p.Length > 0)
                                {
                                    info.AddRange(p);
                                }
                            }
                        }
                    }
                }
            }
        }

        private ImageInfo[][] GroupByScene(List<ImageInfo> log)
        {
            if(log.Count == 0)
            {
                return new ImageInfo[0][];
            }
            // 作品名ID,話数ID,フレーム番号の順でソート
            var group = from c in log
                       orderby c.TitleId, c.EpisodeId, c.Frame
                       select c;

            List<ImageInfo[]> scene = new List<ImageInfo[]>();
            List<ImageInfo> s = new List<ImageInfo>();

            // シーンごとに切り分ける
            var old = group.First();
            foreach (var g in group)
            {
                if (g.TitleId != old.TitleId || g.EpisodeId != old.EpisodeId || g.Frame - old.Frame > 100)
                {
                    scene.Add(s.ToArray());
                    s.Clear();
                }
                s.Add(g);
                old = g;
            }
            if (s.Count > 0)
            {
                scene.Add(s.ToArray());
            }

            return scene.ToArray();
        }


        /// <summary>
        /// 該当する画像ベクトルを持つ画像の情報を返します
        /// </summary>
        /// <param name="vector">画像ベクトル</param>
        /// <returns>同じ画像ベクトルを持つ画像群</returns>
        public ImageInfo[] GetImageInfo(ulong vector)
        {
            // _info内がHashでソートされていることを仮定して二分探索
            // TODO: Array.BinarySearchとパフォーマンスを比較
            int min = 0;
            int max = _info.Length - 1;
            int mid;
            int p = 0;
            bool found = false;
            while (min <= max)
            {
                mid = min  + (max - min) / 2;
                if(_info[mid].Hash == vector)
                {
                    p = mid;
                    found  = true;
                    break;
                }
                else if(_info[mid].Hash < vector)
                {
                    min = mid + 1;
                }
                else if(_info[mid].Hash > vector)
                {
                    max = mid - 1;
                }
            }
            // 見つからなければ空の配列を返す
            if (!found)
            {
                return new ImageInfo[] { };
            }
            // 見つかったら同じvectorを持つ画像情報をすべて返す
            // (_info内は Hash をキーにしてソート済み)
            min = max = p;
            while(min >= 0 && _info[min].Hash == vector)
            {
                min--;
            }
            while (max < _info.Length && _info[max].Hash == vector)
            {
                max++;
            }
            ImageInfo[] result = new ImageInfo[max - min - 1];
            for(int i= min+1; i< max; i++)
            {
                result[i-min-1] = _info[i];
            }
            return result;
        }
        /// <summary>
        /// 指定した画像ファイルを画像のハッシュ値に相当するフォルダにコピーします
        /// </summary>
        /// <param name="target">画像ファイルのパス</param>
        /// <returns>格納先パス</returns>
        public string AddImage(string target)
        {
            string path = GetImageDirectory(target);
            Directory.CreateDirectory(path);
            string result = path + Path.GetFileName(target);
            File.Copy(target, result, true);
            return result;
        }
        /// <summary>
        /// ハッシュ値からフォルダ名を返します
        /// </summary>
        /// <param name="vec">画像のハッシュ値</param>
        /// <returns>ハッシュ値に相当するフォルダ</returns>
        public string GetImageDirectory(ulong vec)
        {
            string path = _basePath + GetPath(vec);
            return path;
        }

        /// <summary>
        /// ハッシュ値からパス名を返します
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        private string GetPath(ulong vec)
        {
            // 上位から8ビットずつ取り出しPath名とする
            int i0 = (int)(vec & 0xFF);
            vec >>= 8;
            int i8 = (int)(vec & 0xFF);
            vec >>= 8;
            int i16 = (int)(vec & 0xFF);
            vec >>= 8;
            int i24 = (int)(vec & 0xFF);
            vec >>= 8;
            int i32 = (int)(vec & 0xFF);
            vec >>= 8;
            int i40 = (int)(vec & 0xFF);
            vec >>= 8;
            int i48 = (int)(vec & 0xFF);
            vec >>= 8;
            int i56 = (int)(vec & 0xFF);
            return string.Format(@"{0:X2}\{1:X2}\{2:X2}\{3:X2}\{4:X2}\{5:X2}\{6:X2}\{7:X2}\",
                i56, i48, i40, i32, i24, i16, i8, i0);
        }

        private string GetImageDirectory(string target)
        {
            ulong vec = GetVector(target);
            string path = _basePath + GetPath(vec);
            return path;
        }


        /// <summary>
        /// Bitmapをbyte[]に変換する
        /// </summary>
        /// <param name="bitmap">変換元のBitmap</param>
        /// <returns>1 pixel = 4 byte (+3:A, +2:R, +1:G, +0:B) に変換したbyte配列</returns>
        private byte[] BitmapToByteArray(Bitmap bmp)
        {
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData =
                bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            // Bitmapの先頭アドレスを取得
            IntPtr ptr = bmpData.Scan0;

            // 32bppArgbフォーマットで値を格納
            int bytes = bmp.Width * bmp.Height * 4;
            byte[] rgbValues = new byte[bytes];

            // Bitmapをbyte[]へコピー
            Marshal.Copy(ptr, rgbValues, 0, bytes);

            bmp.UnlockBits(bmpData);
            return rgbValues;
        }

        /// <summary>
        /// 入力されたpathをフルパスかつ末尾がパス区切り文字となるように
        /// 変換します。
        /// </summary>
        /// <param name="path">ディレクトリ名</param>
        /// <returns>フルパス</returns>
        public static string GetDirectory(string path)
        {
            var c = Path.DirectorySeparatorChar.ToString();
            path = Path.GetFullPath(path);
            if (!path.EndsWith(c))
            {
                path += c;
            }
            return path;
        }

    }
}
