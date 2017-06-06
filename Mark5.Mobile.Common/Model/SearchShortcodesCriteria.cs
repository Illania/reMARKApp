using System.Collections.Generic;

namespace Mark5.Mobile.Common.Model
{
    public class SearchShortcodesCriteria
    {
        public string SavedSearchFilterHash { get; set; }
        public int MaxToFetch { get; set; } = -1;

        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public FiledInFolderType FiledInFolderType { get; set; }
        public FiledInFolderFolderType FiledInFolderFolderType { get; set; }
        List<int> filedInFolderIds;

        public List<int> FiledInFolderIds
        {
            get
            {
                if (filedInFolderIds == null)
                    filedInFolderIds = new List<int>();
                return filedInFolderIds;
            }
            set => filedInFolderIds = value;
        }
    }
}