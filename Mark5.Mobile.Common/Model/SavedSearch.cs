//
// File: SavedSearch.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

namespace Mark5.Mobile.Common.Model
{
    public class SavedSearch
    {
        public string Name { get; set; }
        public ObjectType ObjectType { get; set; }
        public string SavedSearchFilterHash { get; set; }
    }
}