using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common
{
    public abstract class DownloadItemInfo
    {
        public int FolderId { get; set; }
        virtual public ObjectType Type { get; set; }
    }

    public class ContactDownloadInfo : DownloadItemInfo
    {
        public override ObjectType Type
        {
            get
            {
                return ObjectType.Contact;
            }
        }
    }

    public class ShortcodeDownloadInfo : DownloadItemInfo
    {
        public override ObjectType Type
        {
            get
            {
                return ObjectType.Shortcode;
            }
        }
    }

    public class DocumentDownloadInfo : DownloadItemInfo
    {
        public override ObjectType Type
        {
            get
            {
                return ObjectType.Document;
            }
        }
    }
}

