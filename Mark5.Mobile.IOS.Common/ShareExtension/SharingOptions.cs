using System.Collections.Generic;
using Foundation;

namespace Mark5.Mobile.IOS.Common.ShareExtension
{
    public class SharingOptions
    {
        public SharedContentInsertType SharedContentInsertType => contentInsertType;
        public List<NSUrl> UrlList => urlList;

        private readonly SharedContentInsertType contentInsertType;
        private readonly List<NSUrl> urlList;

        public SharingOptions(SharedContentInsertType contentInsertType, List<NSUrl> urlList)
        {
            this.contentInsertType = contentInsertType;
            this.urlList = urlList;
        }
    }

    public enum SharedContentInsertType
    {
        Text = 0,
        File = 1
    }
}
