using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mpga.ImageSearchEngine;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace GochiusearchCmd
{
    /// <summary>
    /// 動画ファイルの処理中の状況を報告する
    /// </summary>
    public class ProgressEventArgs : EventArgs
    {
        /// <summary>
        /// 処理が終わったフレーム数
        /// </summary>
        public uint Current { get; set; }
        /// <summary>
        /// 処理対象全体のフレーム数
        /// </summary>
        public uint Total { get; set; }
        /// <summary>
        /// 処理の完了状況(%)
        /// </summary>
        public float ProgressPercentage { get { return 100f * Current / Total; } }
        /// <summary>
        /// 処理中の状況
        /// </summary>
        /// <param name="current">現在処理が終了したフレーム番号</param>
        /// <param name="total">全体のフレーム数</param>
        public ProgressEventArgs(uint current, uint total)
        {
            Current = current;
            Total = total;
        }
    }
    public class Decoder : IDisposable
    {
        private bool disposedValue;
        private VideoCapture capture;
        private ImageSearch imageSearch;
        private List<ImageInfo> info = new List<ImageInfo>();
        public UInt16 TitleId { get; private set; }
        public UInt16 EpisodeId { get; private set; }
        private UInt32 frame = 0;

        public string Path {get; private set;}
        public event EventHandler<ProgressEventArgs> ProgressChanged;
        public event EventHandler<ProgressEventArgs> Completed;
        public Decoder(string path, UInt16 titleId, UInt16 episodeId)
        {
            Path = path;
            TitleId = titleId;
            EpisodeId = episodeId;

            capture = new VideoCapture(Path);
            imageSearch = new ImageSearch();
        }

        public void Save(string filename, ImageInfo[] info)
        {
            var sortedInfo = info.OrderBy(c => c.Hash).ThenBy(c => c.TitleId).ThenBy(c => c.EpisodeId).ThenBy(c => c.Frame);
            using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                // .gz 形式で圧縮
                using (GZipStream gzipStream = new GZipStream(fs, CompressionMode.Compress))
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
        }

        public ImageInfo[] CreateImageInfo()
        {
            // Initialize
            if(info.Count > 0)
            {
                info.Clear();
            }
            frame = 0;

            int len = capture.FrameCount;
            int threads = Environment.ProcessorCount;
            Mat[] mats = new Mat[threads];
            Bitmap[] bitmaps = new Bitmap[threads];
            int width = capture.FrameWidth;
            int height = capture.FrameHeight;
            
            // Init for multi threading
            for(int i=0; i<threads; i++)
            {
                mats[i] = new Mat();
                bitmaps[i] = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            }
            ulong[] vecs = new ulong[threads];
            int mod = len % threads;
            len = len - mod;

            for(int i=0; i<mod; i++)
            {
                capture.Read(mats[0]);
                BitmapConverter.ToBitmap(mats[0], bitmaps[0]);
                vecs[0] = imageSearch.GetVector(bitmaps[0]);
                AddInfo(vecs[0]);
            }

            // Execute parallel
            for(int i=0; i<len; i += threads)
            {
                for(int j=0; j<threads; j++)
                {
                    capture.Read(mats[j]);
                }
                Parallel.For(0, threads, j =>
                {
                    BitmapConverter.ToBitmap(mats[j], bitmaps[j]);
                    vecs[j] = imageSearch.GetVector(bitmaps[j]);
                });
                for(int j=0; j<threads; j++)
                {
                    AddInfo(vecs[j]);
                }
                OnProgressChanged(new ProgressEventArgs(frame, (uint)len));
            }

            // Release resources
            for (int i = 0; i < threads; i++)
            {
                mats[i].Dispose();
                bitmaps[i].Dispose();
            }
            OnProgressChanged(new ProgressEventArgs(frame, (uint)len));
            OnCompleted(new ProgressEventArgs(frame, (uint)len));
            return info.ToArray();
        }
        protected virtual void OnProgressChanged(ProgressEventArgs args)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<ProgressEventArgs> raiseEvent = ProgressChanged;

            if (raiseEvent != null)
            {
                raiseEvent(this, args);
            }
        }
        protected virtual void OnCompleted(ProgressEventArgs args)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<ProgressEventArgs> raiseEvent = Completed;

            if (raiseEvent != null)
            {
                raiseEvent(this, args);
            }
        }
        private void AddInfo(ulong hash)
        {
            info.Add(new ImageInfo
            {
                Hash = hash,
                TitleId = this.TitleId,
                EpisodeId = this.EpisodeId,
                Frame = frame
            });
            frame++;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if(capture != null)
                    {
                        capture.Dispose();
                        capture = null;
                    }
                }
                info.Clear();
                info = null;

                disposedValue = true;
            }
        }

        void IDisposable.Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
