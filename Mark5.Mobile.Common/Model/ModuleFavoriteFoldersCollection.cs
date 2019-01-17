using System;
using System.Collections.Generic;

namespace Mark5.Mobile.Common.Model
{
    public class ModuleFavorites
    {
        public List<ModuleFavorite> ModuleFavoritesList { get; set; } = new List<ModuleFavorite>();

        public DateTime UpdatedAt { get; set; }
    }

    public class ModuleFavorite
    {
        public List<Folder> Folders { get; set; } = new List<Folder>();

        public ModuleType ModuleType { get; set; }
    }
}


