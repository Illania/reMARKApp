using System.IO;

namespace Mark5.Mobile.Common
{
    public class Attachment
    {
        public string Filename { get; set; }
        public string Extension { get; set; }
        public int Size { get; set; }
        public string Md5 { get; set; }
        public Stream Stream { get; set; }
    }
}