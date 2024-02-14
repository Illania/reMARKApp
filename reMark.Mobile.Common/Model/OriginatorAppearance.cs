using System;
namespace reMark.Mobile.Common.Model
{
    public class OriginatorAppearance
    {
        public bool Enable { get; set; }
        public Guid OriginatorGid { get; set; }
        public string OriginatorName { get; set; }
        public int BackgroundColor { get; set; }
        public int FontColor { get; set; } = -16777216;
        public int UnreadFontColor { get; set; } = -16777216;
        public bool FontColorEnable { get; set; }
        public bool UnreadFontColorEnable { get; set; }
        public bool OriginatorColumnOnly { get; set; }
    }

}
