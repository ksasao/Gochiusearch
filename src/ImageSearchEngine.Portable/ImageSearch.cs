using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using PCLStorage;

namespace Mpga.ImageSearchEngine
{
    public abstract class ImageSearch
    {
        private readonly string _basePath;

        private ImageInfo[] _info;

        protected ImageSearch()
        {
        }

        protected ImageSearch(string basePath)
        {
            _basePath = basePath;
        }

        protected ImageSearch(ImageInfo[] info)
        {
            _info = info;
        }

        public async Task<ImageInfo[]> LoadFromDbAsync(string dbFileName)
        {
            var file = await FileSystem.Current.GetFileFromPathAsync(dbFileName).ConfigureAwait(false);
            if (file == null)
                throw new FileNotFoundException();
            using (var fs = await file.OpenAsync(FileAccess.Read).ConfigureAwait(false))
            using (var gz = new GZipStream(fs, CompressionMode.Decompress))
            using (var br = new BinaryReader(gz))
            {
                long count = br.ReadInt32();
                _info = new ImageInfo[count];

                for (var i = 0; i < count; i++)
                {
                    var hash = br.ReadUInt64();
                    var titleId = br.ReadUInt16();
                    var episodeId = br.ReadUInt16();
                    var frame = br.ReadUInt32();
                    _info[i] = new ImageInfo
                    {
                        Hash = hash,
                        TitleId = titleId,
                        EpisodeId = episodeId,
                        Frame = frame
                    };
                }
            }
            return _info;
        }

        /// <summary>
        /// 画像ファイルから類似画像が近い値を持つようなハッシュ値を計算します
        /// </summary>
        /// <param name="fileName">画像ファイル</param>
        /// <returns>ハッシュ値</returns>
        public ulong GetVector(string fileName)
        {
            // dHash (difference hash) を計算
            // http://www.hackerfactor.com/blog/?/archives/529-Kind-of-Like-That.html

            // 入力した画像を 9x8 の領域にスケールする
            var data = GetSmallImageData(GetImageData(fileName), 9, 8);
            // モノクロ化
            var mono = new int[data.Length / 4];
            for (var i = 0; i < mono.Length; i++)
            {
                mono[i] = 29 * data[i * 4] + 150 * data[i * 4 + 1] + 77 * data[i * 4 + 2];
            }

            var result = 0UL;
            var p = 0;
            for (var y = 0; y < 8; y++)
            {
                for (var x = 0; x < 8; x++)
                {
                    result = (result << 1) | (mono[p] > mono[p + 1] ? (uint)1 : 0);
                    p++;
                }
                p++;
            }

            return result;
        }

        protected abstract Argb32DataWithSize GetImageData(string targetFile);

        private byte[] GetSmallImageData(Argb32DataWithSize source, int width, int height)
        {
            // 等間隔にサンプリングして平均をとる
            byte[] result = new byte[width * height * 4];
            int s = 12; // s*s = サンプリング数
            int pos = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int srcX0 = x * source.Width / width;
                    int srcY0 = y * source.Height / height;

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
                            int dx = xx * source.Width / width / s;
                            int dy = yy * source.Height / height / s;
                            int p = ((srcX0 + dx) + (srcY0 + dy) * source.Width) * 4;
                            b += source.Payload[p];
                            g += source.Payload[p + 1];
                            r += source.Payload[p + 2];
                            a += source.Payload[p + 3];
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
        public ImageInfo[][] GetSimilarImage(ulong vec, int level)
        {
            var info = new List<ImageInfo>();
            if (level <= 3)
            {
                GetSimilarImage(vec, level, info); // 二分探索
            }
            else
            {
                GetSimilarImage2(vec, level, info); // popcntで全件検索
            }
            return GroupByScene(info);
        }

        /// <summary>
        /// 1となっているビットの数を数える
        /// </summary>
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
                if (p.Length > 0) info.AddRange(p);
            }

            // ハミング距離1の探索
            if (level >= 1)
            {
                for (var i = 0; i < 64; i++)
                {
                    var v = vec ^ 1UL << i;
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
                for (var i = 0; i < 63; i++)
                {
                    for (var j = i + 1; j < 64; j++)
                    {
                        var v = vec ^ 1UL << i ^ 1UL << j;
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
                for (var i = 0; i < 62; i++)
                {
                    for (var j = i + 1; j < 63; j++)
                    {
                        for (var k = j + 1; k < 64; k++)
                        {
                            var v = vec ^ 1UL << i ^ 1UL << j ^ 1UL << k;
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
                for (var i = 0; i < 61; i++)
                {
                    for (var j = i + 1; j < 62; j++)
                    {
                        for (var k = j + 1; k < 63; k++)
                        {
                            for (var l = k + 1; l < 64; l++)
                            {
                                var v = vec ^ 1UL << i ^ 1UL << j ^ 1UL << k ^ 1UL << l;
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

        private void GetSimilarImage2(ulong vec, int level, ICollection<ImageInfo> info)
        {
            // 全件検索
            for (var i = 0; i < _info.Length; i++)
            {
                if (PopulationCount(vec ^ _info[i].Hash) <= level)
                {
                    info.Add(_info[i]);
                }
            }
        }

        private ImageInfo[][] GroupByScene(IReadOnlyCollection<ImageInfo> log)
        {
            if (log.Count == 0)
                return new ImageInfo[0][];

            // 作品名ID,話数ID,フレーム番号の順でソート
            var group = from c in log
                        orderby c.TitleId, c.EpisodeId, c.Frame
                        select c;

            var scene = new List<ImageInfo[]>();
            var s = new List<ImageInfo>();

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
            int min = 0;
            int max = _info.Length - 1;
            int mid;
            int p = 0;
            bool found = false;
            while (min <= max)
            {
                mid = min + (max - min) / 2;
                if (_info[mid].Hash == vector)
                {
                    p = mid;
                    found = true;
                    break;
                }
                else if (_info[mid].Hash < vector)
                {
                    min = mid + 1;
                }
                else if (_info[mid].Hash > vector)
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
            while (min >= 0 && _info[min].Hash == vector)
            {
                min--;
            }
            while (max < _info.Length && _info[max].Hash == vector)
            {
                max++;
            }
            ImageInfo[] result = new ImageInfo[max - min - 1];
            for (int i = min + 1; i < max; i++)
            {
                result[i - min - 1] = _info[i];
            }
            return result;
        }

        /// <summary>
        /// ハッシュ値からフォルダ名を返します
        /// </summary>
        /// <param name="vec">画像のハッシュ値</param>
        /// <returns>ハッシュ値に相当するフォルダ</returns>
        public string GetImageDirectory(ulong vec) => Path.Combine(_basePath, GetPathFromHash(vec));

        /// <summary>
        /// ハッシュ値からパス名を返します
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        private static string GetPathFromHash(ulong vec)
        {
            var bits = new int[8];
            for (var i = bits.Length - 1; i >= 0; i--)
            {
                bits[i] = (int)(vec & 0xFF);
                vec >>= 8;
            }
            return string.Join("\\", bits.Select(x => $"{x:X2}"));
        }
    }
}