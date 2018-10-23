using System;
using System.Collections.Generic;
using System.Linq;
using Mark5.Mobile.Common.Model.Converters;
using Mark5.ServiceReference.DataContract;

namespace Mark5.Mobile.Common.Model
{
    public class ModuleFavorites
    {
        public List<ModuleFavorite> ModuleFovoritesList { get; set; }

        public DateTime UpdatedAt { get; set; }

        public ModuleFavorites() 
        {
            ModuleFovoritesList = new List<ModuleFavorite>();
        }
    }

    public class ModuleFavorite
    {
        public List<Folder> Folders { get; set; } = new List<Folder>();

        public ModuleType ModuleType { get; set; }
    }
}


