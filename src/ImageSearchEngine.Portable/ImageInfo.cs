using System;
using System.Collections.Generic;
using System.Linq;

namespace Mpga.ImageSearchEngine
{
    public struct ImageInfo : IComparer<ImageInfo>
    {
        public ulong Hash;
        public ushort TitleId;
        public ushort EpisodeId;
        public uint Frame;

        public int Compare(ImageInfo x, ImageInfo y) => x.Hash.CompareTo(y.Hash);
    }
}
