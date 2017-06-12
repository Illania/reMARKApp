using System;

namespace Mark5.Mobile.Common.Model
{
    public class FolderOfflineInfo
    {
        public int FolderId { get; set; } = -1;
        public int FolderName { get; set; }
        public int LastDownloaded { get; set; } = -1;
    }
}
