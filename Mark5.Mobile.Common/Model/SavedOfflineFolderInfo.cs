using System;
using System.Collections.Generic;

namespace Mark5.Mobile.Common.Model
{
    public class SavedOfflineFolderInfo : IEqualityComparer<SavedOfflineFolderInfo>
    {
        public int FolderId { get; set; } = -1;
        public Guid FolderGuid { get; set; } = Guid.Empty;
        public string FolderName { get; set; }
        public ModuleType Module { get; set; }
        public long LastDownloaded { get; set; } = -1;

        public bool Equals(SavedOfflineFolderInfo x, SavedOfflineFolderInfo y)
        {
            if (x is null || y is null)
                return false;
            
            return x.FolderId == x.FolderId;
        }

        public int GetHashCode(SavedOfflineFolderInfo folder)
        {
            //Check whether the object is null 
            if (folder is null) return 0;

            //Get hash code for the Numf field if it is not null. 
            int hashNumf = folder.FolderId == null ? 0 : folder.FolderId.GetHashCode();

            return hashNumf;
        }
    }
}
