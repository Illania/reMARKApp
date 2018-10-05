using System;
using System.Collections.Generic;
using System.Linq;
using Mark5.Mobile.Common.Model.Converters;
using Mark5.ServiceReference.DataContract;

namespace Mark5.Mobile.Common.Model
{
    public class ModuleFavoritesWrapper
    {
        public List<ModuleFavorite> ModuleFovorites { get; set; }

        public DateTime UpdatedAt { get; set; }

        public ModuleFavoritesWrapper() 
        {
            ModuleFovorites = new List<ModuleFavorite>();
        }

        public ModuleFavoritesWrapper(GetModuleFavoritesResult getFavoriteFoldersResult)
        {
            UpdatedAt = getFavoriteFoldersResult.UpdatedAt;

            if (getFavoriteFoldersResult.ModuleFavorites != null) 
            {
                ModuleFovorites = new List<ModuleFavorite>();

                foreach(var fav in getFavoriteFoldersResult.ModuleFavorites) 
                {
                    var newFav = new ModuleFavorite() { ModuleType = (ModuleType)fav.ModuleType };

                    foreach (var folder in fav.Folders)
                    {
                        newFav.Folders.Add(folder.Convert());
                    }

                    ModuleFovorites.Add(newFav);
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


