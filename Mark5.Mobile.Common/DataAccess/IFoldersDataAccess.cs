//
// Project: Mark5.Mobile.Common
// File: IFoldersDataAccess.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.DataAccess
{

    interface IFoldersDataAccess
    {

        Task InsertOrReplaceRecursively(ModuleType moduleType, List<Folder> folders, Folder parentFolder = null);

        Task<List<Folder>> GetRecursively(ModuleType moduleType, Folder parentFolder = null, int depth = 2);

        Task SetSubscribed(ModuleType moduleType, List<Folder> folders, bool subscribed);

        Task SetAllSubscribed(bool subscribed);
    }
}

