using System;
using System.Collections.Generic;
using System.Linq;
using Mark5.Mobile.Common.Model.Converters;
using Mark5.ServiceReference.DataContract;

namespace Mark5.Mobile.Common.Model
{
    public class FavoriteFolders
    {
        public List<ModuleFavorite> Favorites { get; set; }

        public DateTime UpdatedAt { get; set; }

        public FavoriteFolders() 
        {
            Favorites = new List<ModuleFavorite>();
        }

        public FavoriteFolders(GetFavoriteFoldersResult getFavoriteFoldersResult)
        {
            UpdatedAt = getFavoriteFoldersResult.UpdatedAt;

            if (getFavoriteFoldersResult.ModuleFavorites != null) 
            {
                Favorites = new List<ModuleFavorite>();

                foreach(var fav in getFavoriteFoldersResult.ModuleFavorites) 
                {
                    var newFav = new ModuleFavorite() { ModuleType = (ModuleType)fav.ModuleType };

                    foreach (var folder in fav.Folders)
                    {
                        newFav.Folders.Add(folder.Convert());
                    }

                    Favorites.Add(newFav);
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


