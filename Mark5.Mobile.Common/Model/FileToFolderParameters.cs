using System;
using System.Collections.Generic;

namespace Mark5.Mobile.Common.Model
{
    public class FileToFolderParameters
    {
        public FileToFolderType FileToFolderType { get; set; }
        public int? FileToFolderId { get; set; }
        public List<int> CopyToWorktrayForUsers{ get; set; } = new List<int>();
    }

}
