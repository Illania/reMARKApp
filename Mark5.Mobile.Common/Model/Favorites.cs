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

        public ModuleFavorites(GetModuleFavoritesResult moduleFavoritesResult)
        {
            UpdatedAt = moduleFavoritesResult.UpdatedAt;

            if (moduleFavoritesResult.ModuleFavoritesList != null) 
            {
                ModuleFovoritesList = new List<ModuleFavorite>();

                foreach(var fav in moduleFavoritesResult.ModuleFavoritesList) 
                {
                    var newFav = new ModuleFavorite() { ModuleType = (ModuleType)fav.ModuleType };

                    foreach (var folder in fav.Folders)
                    {
                        newFav.Folders.Add(folder.Convert());
                    }

                    ModuleFovoritesList.Add(newFav);
                }
            }
        }
    }

    public class ModuleFavorite
    {
        public List<Folder> Folders { get; set; } = new List<Folder>();

        public ModuleType ModuleType { get; set; }
    }
}


