using System;
using System.Collections.Generic;

namespace Mark5.Mobile.Common.Model
{
    public class ModuleFavoriteFoldersCollection
    {
        public List<ModuleFavoriteFolders> ModuleFavoriteFolders { get; set; }

        public DateTime UpdatedAt { get; set; }
    }

    public class ModuleFavoriteFolders
    {
        public List<Folder> Folders { get; set; } = new List<Folder>();

        public ModuleType ModuleType { get; set; }
    }
}


