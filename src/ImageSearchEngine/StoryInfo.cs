using System;
using System.Linq;

namespace Mpga.ImageSearchEngine
{
    public struct StoryInfo
    {
        public int TitleId;
        public int EpisodeId;
        public float FrameRate;
        public string Title;
        public Chapter[] Chapter;
    }
    public struct Chapter
    {
        public string Subtitle;
        public uint StartFrame;
        public string Url;
    }
}
