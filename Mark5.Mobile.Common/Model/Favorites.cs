using System;
using System.Collections.Generic;
using System.Linq;
using Mark5.Mobile.Common.Model.Converters;
using Mark5.ServiceReference.DataContract;

namespace Mark5.Mobile.Common.Model
{
    public class FavoriteFolders
    {
        public List<Favorite> Favorites { get; set; }

        public DateTime UpdatedAt { get; set; }

        public FavoriteFolders() 
        {
            Favorites = new List<Favorite>();
        }

        public FavoriteFolders(GetFavoriteFoldersResult getFavoriteFoldersResult)
        {
            UpdatedAt = getFavoriteFoldersResult.UpdatedAt;

            if (getFavoriteFoldersResult.Favorites != null) 
            {
                Favorites = new List<Favorite>();

                foreach(var fav in getFavoriteFoldersResult.Favorites) 
                {
                    var newFav = new Favorite() { ModuleType = (ModuleType)fav.ModuleType };

                    foreach (var folder in fav.Folders)
                    {
                        newFav.Folders.Add(folder.Convert());
                    }

                    Favorites.Add(newFav);
                }
            }
        }
    }

    public class Favorite
    {
        public List<Folder> Folders { get; set; } = new List<Folder>();

        public ModuleType ModuleType { get; set; }
    }
}


