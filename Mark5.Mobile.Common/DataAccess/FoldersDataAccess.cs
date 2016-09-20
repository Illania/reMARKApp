//
// Project: Mark5.Mobile.Common
// File: FoldersDataAccess.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess.Exceptions;
using Mark5.Mobile.Common.Database;
using Mark5.Mobile.Common.Model;
using SQLite;

namespace Mark5.Mobile.Common.DataAccess
{

    class FoldersDataAccess : IFoldersDataAccess
    {

        readonly Func<ModuleType, DatabaseConnectionProvider> databaseForModuleType;

        public FoldersDataAccess(Func<ModuleType, DatabaseConnectionProvider> databaseForModuleType)
        {
            this.databaseForModuleType = databaseForModuleType;
        }

        public async Task InsertOrReplaceRecursively(ModuleType moduleType, List<Folder> folders, Folder parentFolder = null)
        {
            try
            {
                await databaseForModuleType(moduleType).RunInConnectionAsync(c =>
                {
                    DeleteRecursively(c, parentFolder?.Id ?? 0);
                    InsertOrReplaceRecursively(c, folders);
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error inserting folders.", ex);
            }
        }

        public async Task<List<Folder>> GetRecursively(ModuleType moduleType, Folder parentFolder = null, int depth = 2)
        {
            try
            {
                List<Folder> list = null;

                await databaseForModuleType(moduleType).RunInConnectionAsync(c =>
                {
                    list = GetRecursively(c, parentFolder?.Id ?? 0, depth);
                });

                if (parentFolder != null)
                {
                    parentFolder.SubFolders = list;
                }

                if (list == null || list.Count < 1)
                {
                    throw new DataNotFoundException("Folders could not be found.");
                }

                return list;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting folders.", ex);
            }
        }

        public async Task SetSubscribed(ModuleType moduleType, List<Folder> folders, bool subscribed)
        {
            try
            {
                await databaseForModuleType(moduleType).RunInConnectionAsync(c =>
                {
                    var cmd = c.CreateCommand($"update \"{nameof(Folder)}\"" +
                                              $"set \"{nameof(Folder.Subscribed)}\" = @subscribed " +
                                              $"where \"{nameof(Folder.Id)}\" in ({string.Join(",", folders.Select(f => f.Id))})");
                    cmd.Bind("@subscribed", subscribed);
                    cmd.ExecuteNonQuery();
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error setting folders as subscribed.", ex);
            }
        }

        public async Task SetAllSubscribed(bool subscribed)
        {
            try
            {
                foreach (var moduleType in new[] { ModuleType.Documents, ModuleType.Contacts, ModuleType.Shortcodes })
                {
                    await databaseForModuleType(moduleType).RunInConnectionAsync(c =>
                    {
                        var cmd = c.CreateCommand($"update \"{nameof(Folder)}\"" +
                                                  $"set \"{nameof(Folder.Subscribed)}\" = @subscribed");
                        cmd.Bind("@subscribed", false);
                        cmd.ExecuteNonQuery();
                    });
                }
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error setting all folders as subscribed.", ex);
            }
        }

        #region Helper methods

        void DeleteRecursively(SQLiteConnection c, int parentFolderId)
        {
            if (parentFolderId == 0)
            {
                c.DeleteAll<Folder>();
            }
            else
            {
                var cmd = c.CreateCommand($"select \"{nameof(Folder.Id)}\" " +
                                          $"from \"{nameof(Folder)}\" " +
                                          $"where \"{nameof(Folder.ParentFolderId)}\" = @parentFolderId");
                cmd.Bind("@parentFolderId", parentFolderId);
                var subFolderIds = cmd.ExecuteQuery<IdValue>().Select(id => id.Id);

                foreach (var subFolderId in subFolderIds)
                {
                    DeleteRecursively(c, subFolderId);
                    c.Delete<Folder>(subFolderId);
                }
            }
        }

        void InsertOrReplaceRecursively(SQLiteConnection c, List<Folder> folders)
        {
            foreach (var folder in folders)
            {
                c.InsertOrReplace(folder);
                InsertOrReplaceRecursively(c, folder.SubFolders);
            }
        }

        List<Folder> GetRecursively(SQLiteConnection c, int parentFolderId, int depth)
        {
            var subFolders = c.Table<Folder>().Where(f => f.ParentFolderId == parentFolderId).OrderBy(f => f.Position).ToList();

            if (depth > 0)
            {
                foreach (var subFolder in subFolders)
                {
                    subFolder.SubFolders = GetRecursively(c, subFolder.Id, depth - 1);
                }
            }

            return subFolders;
        }

        #endregion

    }
}

