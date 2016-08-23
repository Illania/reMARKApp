using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common
{
    public class DownloadItemInfo //TODO find a better name
    {
        public int Id { get; set; }
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
}

