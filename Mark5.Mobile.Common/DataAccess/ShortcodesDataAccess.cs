//
// Project: Mark5.Mobile.Common
// File: ShortcodesDataAccess.cs
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
using Mark5.Mobile.Common.Model.Links;

namespace Mark5.Mobile.Common.DataAccess
{

    class ShortcodesDataAccess : IShortcodesDataAccess
    {

        readonly DatabaseConnectionProvider shortcodesDatabase;

        public ShortcodesDataAccess(DatabaseConnectionProvider shortcodesDatabase)
        {
            this.shortcodesDatabase = shortcodesDatabase;
        }
        public async Task SaveShortcodePreviewsAsync(Folder folder, List<ShortcodePreview> shortcodePreviews, bool clean)
        {
            try
            {
                await shortcodesDatabase.RunInConnectionAsync(c =>
                {
                    if (clean)
                    {
                        c.Table<FolderShortcodeLink>()
                         .Delete(fdl => fdl.FolderId == folder.Id);
                    }

                    c.InsertOrReplaceAll(shortcodePreviews.Select(cp => new FolderShortcodeLink { FolderId = folder.Id, ShortcodeId = cp.Id }));
                    c.InsertOrReplaceAll(shortcodePreviews);
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error saving shortcodes.", ex);
            }
        }
        public async Task<List<ShortcodePreview>> GetShortcodePreviewsAsync(Folder folder, int startRowId, int maxItems)
        {
            try
            {
                List<ShortcodePreview> shortcodePreviews = null;

                await shortcodesDatabase.RunInConnectionAsync(c =>
                {
                    var query = c.Table<FolderShortcodeLink>()
                                 .Where(fdl => fdl.FolderId == folder.Id)
                                 .Join(c.Table<ShortcodePreview>(), fdl => fdl.ShortcodeId, cp => cp.Id, (fdl, cp) => cp)
                                 .OrderBy(cp => cp.Name);

                    if (startRowId > 0)
                    {
                        query = query.Skip(startRowId);
                    }

                    if (maxItems > 0)
                    {
                        query = query.Take(maxItems);
                    }

                    var result = query.ToList();

                    if (result == null || result.Count < 1)
                    {
                        throw new DataNotFoundException("Shortcode previews could not be found.");
                    }

                    startRowId = startRowId < 1 ? 1 : startRowId;
                    foreach (var shortcodePreview in shortcodePreviews)
                    {
                        shortcodePreview.RowId = startRowId++;
                    }

                    shortcodePreviews = result;
                });

                return shortcodePreviews;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting shortcodes.", ex);
            }
        }

        public async Task SaveShortcodeAsync(Shortcode shortcode)
        {
            try
            {
                await shortcodesDatabase.RunInConnectionAsync(c =>
                {
                    c.InsertOrReplace(shortcode);
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error saving shortcode.", ex);
            }
        }

        public async Task<Shortcode> GetShortcodeAsync(int shortcodeId)
        {
            try
            {
                Shortcode shortcode = null;

                await shortcodesDatabase.RunInConnectionAsync(c =>
                {
                    var result = c.Find<Shortcode>(shortcodeId);

                    if (result == null)
                    {
                        throw new DataNotFoundException("Shortcode could not be found.");
                    }

                    shortcode = result;
                });

                return shortcode;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting shortcode.", ex);
            }
        }
        public async Task RemoveFromFolderAsync(List<ShortcodePreview> shortcodePreviews, Folder folder)
        {
            var ids = shortcodePreviews.Select(sp => sp.Id).Distinct().ToList();
            await RemoveFromFolderAsync(ids, folder.Id);
        }

        public async Task RemoveFromFolderAsync(List<Shortcode> shortcodes, Folder folder)
        {
            var ids = shortcodes.Select(s => s.Id).Distinct().ToList();
            await RemoveFromFolderAsync(ids, folder.Id);
        }

        async Task RemoveFromFolderAsync(List<int> ids, int folderId)
        {
            try
            {
                await shortcodesDatabase.RunInConnectionAsync(c =>
                {
                    foreach (var id in ids)
                    {
                        var linksCount = c.Table<FolderShortcodeLink>().Count(fdl => fdl.ShortcodeId == id);
                        if (linksCount == 1)
                        {
                            c.Table<ShortcodePreview>().Delete(sp => sp.Id == id);
                            c.Table<Shortcode>().Delete(s => s.Id == id);
                        }

                        c.Table<FolderShortcodeLink>().Delete(fsl => fsl.ShortcodeId == id && fsl.FolderId == folderId);
                    }
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error removing shortcodes from folder.", ex);
            }
        }

        public async Task DeleteAsync(List<ShortcodePreview> shortcodePreviews)
        {
            var ids = shortcodePreviews.Select(sp => sp.Id).Distinct().ToList();
            await DeleteAsync(ids);
        }

        public async Task DeleteAsync(List<Shortcode> shortcodes)
        {
            var ids = shortcodes.Select(s => s.Id).Distinct().ToList();
            await DeleteAsync(ids);
        }

        async Task DeleteAsync(List<int> ids)
        {
            try
            {
                await shortcodesDatabase.RunInConnectionAsync(c =>
                {
                    c.Table<FolderShortcodeLink>().Delete(fsl => ids.Contains(fsl.ShortcodeId));
                    c.Table<ShortcodePreview>().Delete(sp => ids.Contains(sp.Id));
                    c.Table<Shortcode>().Delete(s => ids.Contains(s.Id));
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error deleting shortcodes.", ex);
            }
        }

        public async Task<IEnumerable<ShortcodeDownloadInfo>> GetUnsavedShortcodesIds(int? folderId)
        {
            try
            {
                var infos = new List<ShortcodeDownloadInfo>();

                await shortcodesDatabase.RunInConnectionAsync(c =>
                {

                    var folderCondition = folderId.HasValue ? $"and { nameof(FolderShortcodeLink.FolderId)} = ? " : "";
                    var queryString = $"select * from {nameof(FolderShortcodeLink)} where  {nameof(FolderShortcodeLink.ShortcodeId)}  " +
                        $" not in (select {nameof(Shortcode.Id)} from {nameof(Shortcode)}) {folderCondition}";

                    var result = c.Query<FolderShortcodeLink>(queryString, folderId.Value);

                    infos = result.Select(fcl => new ShortcodeDownloadInfo { FolderId = fcl.FolderId, Id = fcl.ShortcodeId }).ToList();
                });

                return infos;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error while getting not cached shortcodes.", ex);
            }
        }

        public async Task<bool> IsShortcodeCached(int shortcodeId)
        {
            try
            {
                bool found = false;

                await shortcodesDatabase.RunInConnectionAsync(c =>
                {
                    var query = $"select count(*) from {nameof(Shortcode)} where {nameof(Shortcode.Id)} = ?  ";
                    var result = c.ExecuteScalar<int>(query, shortcodeId);

                    found = result >= 1;
                });

                return found;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error while checking shortcode existence.", ex);
            }
        }
    }
}

