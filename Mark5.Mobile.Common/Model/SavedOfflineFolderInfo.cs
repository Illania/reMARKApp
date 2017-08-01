namespace Mark5.Mobile.Common.Model
{
    public class SavedOfflineFolderInfo
    {
        public int FolderId { get; set; } = -1;
        public string FolderName { get; set; }
        public ModuleType Module { get; set; }
        public long LastDownloaded { get; set; } = -1;
    }
}
