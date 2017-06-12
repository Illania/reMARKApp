using System.Collections.Generic;

namespace Mark5.Mobile.Common
{
    public abstract class DownloadPolicy
    {
    }

    public class DownloadFoldersPolicy : DownloadPolicy
    {
        public List<int> FolderIds { get; } = new List<int>();
    }
}