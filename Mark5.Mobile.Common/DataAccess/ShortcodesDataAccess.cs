using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess.Exceptions;
using Mark5.Mobile.Common.DataAccess.Interfaces;
using Mark5.Mobile.Common.Database;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Containers;
using Mark5.Mobile.Common.Model.Links;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.Common.DataAccess
{
    class ShortcodesDataAccess : IShortcodesDataAccess, ICommonActionsDataAccess
    {
        readonly DatabaseConnectionProvider shortcodesDatabase;
        readonly IRestorationDataAccess restorationDataAccess;

        public ShortcodesDataAccess(DatabaseConnectionProvider shortcodesDatabase, IRestorationDataAccess restorationDataAccess)
        {
            this.shortcodesDatabase = shortcodesDatabase;
            this.restorationDataAccess = restorationDataAccess;
        }

        public async Task SaveShortcodePreviewsAsync(Folder folder, List<ShortcodePreview> shortcodePreviews, bool clean)
        {
            try
            {
                await shortcodesDatabase.RunInConnectionAsync(c =>
                {
                    if (clean)
                        c.Table<FolderShortcodeLink>().Delete(fdl => fdl.FolderId == folder.Id);
                    c.InsertOrReplaceAll(shortcodePreviews.Select(cp => new FolderShortcodeLink
                    {
                        FolderId = folder.Id,
                        ShortcodeId = cp.Id
                    }));
                    c.InsertOrReplaceAll(shortcodePreviews);
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error saving shortcodes with folder.", ex);
            }
        }

        public async Task SaveShortcodePreviewsAsync(List<ShortcodePreview> shortcodePreviews)
        {
            try
            {
                await shortcodesDatabase.RunInConnectionAsync(c =>
                {
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
                    var query = $"select * " + $"from {nameof(ShortcodePreview)}, {nameof(FolderShortcodeLink)} " + $"where {nameof(FolderShortcodeLink.FolderId)} = {folder.Id} " + $"     and {nameof(ShortcodePreview)}.{nameof(ShortcodePreview.Id)} = {nameof(FolderShortcodeLink)}.{nameof(FolderShortcodeLink.ShortcodeId)} ";

                    query += $"order by {nameof(ShortcodePreview.Name)} ";

                    if (maxItems > 0)
                        query += $"limit {maxItems - 1} ";
                    if (startRowId > 0)
                        query += $"offset {startRowId} ";
                    var result = c.Query<ShortcodePreview>(query);

                    if (result == null || result.Count < 1)
                        throw new DataNotFoundException("Shortcode previews could not be found.");

                    shortcodePreviews = result;

                    startRowId = startRowId < 1 ? 1 : startRowId;
                    foreach (var shortcodePreview in shortcodePreviews)
                        shortcodePreview.RowId = startRowId++;
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
                await shortcodesDatabase.RunInConnectionAsync(c => { c.InsertOrReplace(shortcode); });
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
                    shortcode = result ?? throw new DataNotFoundException("Shortcode could not be found.");
                });

                return shortcode;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting shortcode.", ex);
            }
        }

        public async Task SaveShortcodeWithPreviewAsync(ShortcodeContainer container)
        {
            try
            {
                await shortcodesDatabase.RunInConnectionAsync(c =>
                {
                    c.InsertOrReplace(container.ShortcodePreview);
                    c.InsertOrReplace(container.Shortcode);
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error saving shortcode with preview.", ex);
            }
        }

        public async Task<ShortcodeContainer> GetShortcodeWithPreviewAsync(int shortcodeId)
        {
            try
            {
                ShortcodeContainer container = null;

                await shortcodesDatabase.RunInConnectionAsync(c =>
                {
                    var shortcodePreview = c.Find<ShortcodePreview>(shortcodeId);
                    if (shortcodePreview == null)
                        throw new DataNotFoundException("ShortcodePreview could not be found.");

                    var shortcode = c.Find<Shortcode>(shortcodeId);
                    if (shortcode == null)
                        throw new DataNotFoundException("Shortcode could not be found.");

                    container = new ShortcodeContainer(shortcodePreview, shortcode);
                });

                return container;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting shortcode with preview.", ex);
            }
        }

        public async Task RemoveFromFolderAsync(List<ShortcodePreview> shortcodePreviews, int folderId, bool saveBeforeDeletion = false)
        {
            try
            {
                var deletedPreviews = new List<ShortcodePreview>();
                await shortcodesDatabase.RunInConnectionAsync(c =>
                {
                    foreach (var sc in shortcodePreviews)
                    {
                        var id = sc.Id;
                        var linksCount = c.Table<FolderShortcodeLink>().Count(fdl => fdl.ShortcodeId == id);
                        if (linksCount == 1)
                        {
                            if (saveBeforeDeletion)
                            {
                                deletedPreviews.Add(sc);
                            }
                            c.Table<ShortcodePreview>().Delete(dp => dp.Id == id);
                            c.Table<Shortcode>().Delete(d => d.Id == id);
                        }
                        c.Table<FolderShortcodeLink>().Delete(fdl => fdl.ShortcodeId == id && fdl.FolderId == folderId);
                    }
                });

                if (deletedPreviews.Any())
                    await SaveDeletedObjectsAsync(deletedPreviews);
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error removing shortcodes from folder.", ex);
            }
        }

        public async Task RemoveFromFolderAsync(List<Shortcode> shortcodes, int folderId, bool saveBeforeDeletion = false)
        {
            try
            {
                var deletedShortcodesToSave = new List<Shortcode>();
                await shortcodesDatabase.RunInConnectionAsync(c =>
                {
                    foreach (var shortcode in shortcodes)
                    {
                        var id = shortcode.Id;
                        var linksCount = c.Table<FolderShortcodeLink>().Count(fdl => fdl.ShortcodeId == id);
                        if (linksCount == 1)
                        {
                            if (saveBeforeDeletion)
                            {
                                deletedShortcodesToSave.Add(shortcode);
                            }
                            c.Table<ShortcodePreview>().Delete(dp => dp.Id == id);
                            c.Table<Shortcode>().Delete(d => d.Id == id);
                        }
                        c.Table<FolderShortcodeLink>().Delete(fdl => fdl.ShortcodeId == id && fdl.FolderId == folderId);
                    }
                });

                if (deletedShortcodesToSave.Any())
                    await SaveDeletedObjectsAsync(deletedShortcodesToSave);
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error removing shortcodes from folder.", ex);
            }
        }

        public async Task RemoveFromFolderAsync(List<IBusinessEntity> businessEntities, int folderId, bool saveBeforeDeletion = false)
        {
            if (businessEntities.FirstOrDefault() is Shortcode shortcode)
                await RemoveFromFolderAsync(businessEntities.Select(be => (Shortcode)be).ToList(), folderId, saveBeforeDeletion);

            else if (businessEntities.FirstOrDefault() is ShortcodePreview shortcodePreview)
                await RemoveFromFolderAsync(businessEntities.Select(be => (ShortcodePreview)be).ToList(), folderId, saveBeforeDeletion);
        }

        public async Task RemoveFromFolderAsync(List<int> ids, int folderId)
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

        public async Task DeleteAsync(List<ShortcodePreview> shortcodePreviews, bool saveBeforeDeletion = false)
        {
            if (saveBeforeDeletion)
            {
                await ((IRestorable)this).SaveDeletedObjectsAsync(shortcodePreviews);
            }
            var ids = shortcodePreviews.Select(sp => sp.Id).Distinct().ToList();
            await DeleteAsync(ids);
        }

        public async Task DeleteAsync(List<Shortcode> shortcodes, bool saveBeforeDeletion = false)
        {
            if (saveBeforeDeletion)
            {
                await ((IRestorable)this).SaveDeletedObjectsAsync(shortcodes);
            }
            var ids = shortcodes.Select(s => s.Id).Distinct().ToList();
            await DeleteAsync(ids);
        }

        public async Task DeleteAsync(List<int> shortocodesIds)
        {
            try
            {
                await shortcodesDatabase.RunInConnectionAsync(c =>
                {
                    c.Table<FolderShortcodeLink>().Delete(fsl => shortocodesIds.Contains(fsl.ShortcodeId));
                    c.Table<ShortcodePreview>().Delete(sp => shortocodesIds.Contains(sp.Id));
                    c.Table<Shortcode>().Delete(s => shortocodesIds.Contains(s.Id));
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error deleting shortcodes.", ex);
            }
        }

        public async Task DeleteAsync(List<IBusinessEntity> businessEntities, bool saveBeforeDeletion = false)
        {
            if (businessEntities.FirstOrDefault() is Shortcode shortcode)
                await DeleteAsync(businessEntities.Select(be => (Shortcode)be).ToList(), saveBeforeDeletion);

            else if (businessEntities.FirstOrDefault() is ShortcodePreview shortcodePreview)
                await DeleteAsync(businessEntities.Select(be => (ShortcodePreview)be).ToList(), saveBeforeDeletion);
        }

        public async Task CopyToFolderAsync(int folderId, List<int> shortcodesIds)
        {
            try
            {
                await shortcodesDatabase.RunInConnectionAsync(c =>
                {
                    c.InsertOrReplaceAll(shortcodesIds.Select(sp => new FolderShortcodeLink
                    {
                        FolderId = folderId,
                        ShortcodeId = sp
                    }));
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException($"Error filing shortcode previews to folder with Id={folderId}.", ex);
            }
        }

        public async Task RemoveOrphans()
        {
            try
            {
                await shortcodesDatabase.RunInConnectionAsync(c =>
                {
                    var innerSelectQueryText = $"select {nameof(FolderShortcodeLink.ShortcodeId)} from {nameof(FolderShortcodeLink)}";

                    var outerDeleteQueryShortcodePreview = $"delete from {nameof(ShortcodePreview)} where {nameof(ShortcodePreview.Id)} not in ({innerSelectQueryText}) ";
                    var cmd = c.CreateCommand(outerDeleteQueryShortcodePreview);
                    cmd.ExecuteNonQuery();

                    var outerDeleteQueryShortcode = $"delete from {nameof(Shortcode)} where {nameof(Shortcode.Id)} not in ({innerSelectQueryText}) ";
                    var cmd2 = c.CreateCommand(outerDeleteQueryShortcode);
                    cmd2.ExecuteNonQuery();
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error removing orphan shortcodes and shortcode previews.", ex);
            }
        }

        public async Task DeleteAllAsync()
        {
            await shortcodesDatabase.RunInConnectionAsync(c =>
            {
                c.DeleteAll<FolderShortcodeLink>();
                c.DeleteAll<ShortcodePreview>();
                c.DeleteAll<Shortcode>();
            });
        }

        public async Task<List<Recipient>> GetSuggestions(string phrase)
        {
            try
            {
                List<Recipient> suggestions = null;

                await shortcodesDatabase.RunInConnectionAsync(c =>
                {

                    var commandString = $"select SCP.{nameof(ShortcodePreview.Name)} as {nameof(Recipient.Name)},"
                         + $" SC.{nameof(Shortcode.AddressString)} as {nameof(Recipient.Address)},"
                         + $" {(int)RecipientType.Shortcode} as {nameof(Recipient.Type)}"
                         + $" from {nameof(ShortcodePreview)} SCP"
                         + $" inner join {nameof(Shortcode)} SC"
                         + $" on SCP.{nameof(ShortcodePreview.Id)} = SC.{nameof(Shortcode.Id)} "
                         + $" where (SCP.{nameof(ShortcodePreview.Name)} like @phrase)"
                         + "  limit 100"
                         + "  collate Nocase";

                    var cmd = c.CreateCommand(commandString);
                    cmd.Bind("@phrase", $"%{phrase}%");
                    var result = cmd.ExecuteQuery<Recipient>();

                    suggestions = result;
                });


                return suggestions;
            }
            catch
            {
                return new List<Recipient>();
            }
        }

        public async Task<List<int>> GetLinkedFoldersIds(int shortcodeId)
        {
            try
            {
                List<int> linkedFoldersId = null;

                await shortcodesDatabase.RunInConnectionAsync(c =>
                {
                    var query = $"select {nameof(FolderShortcodeLink.FolderId)} " + $"from {nameof(FolderShortcodeLink)} "
                    + $"where {nameof(FolderShortcodeLink.ShortcodeId)} = {shortcodeId} ";

                    var result = c.Query<int>(query);

                    if (result == null || result.Count < 1)
                        throw new DataNotFoundException($"Linked folders for shortccode {shortcodeId} could not be found.");

                    linkedFoldersId = result;
                });

                return linkedFoldersId;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException($"Error getting linked folders for shortcode {shortcodeId}.", ex);
            }
        }

        #region IRestorable

        public async Task RestoreDeletedObjectsAsync(List<int> ids)
        {
            try
            {
                var deletedShortcodePreviews = await restorationDataAccess.GetDeletedObjectsAsync(ids, DeletedObjectType.ShortcodePreview);
                var shortcodePreviews = deletedShortcodePreviews.Select(dd => Serializer.Deserialize<ShortcodePreview>(dd.SerializedObject)).ToList();

                var deletedShortcodes = await restorationDataAccess.GetDeletedObjectsAsync(ids, DeletedObjectType.Shortcode);
                var shortcodes = deletedShortcodes.Select(dd => Serializer.Deserialize<Shortcode>(dd.SerializedObject)).ToList();

                await shortcodesDatabase.RunInConnectionAsync(c =>
                {
                    c.InsertOrReplaceAll(shortcodePreviews);
                    c.InsertOrReplaceAll(shortcodes);
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error while restoring deleted shortcodes.", ex);
            }
        }

        public async Task SaveDeletedObjectsAsync<T>(List<T> businessEntities) where T : IBusinessEntity
        {
            try
            {
                await restorationDataAccess.SaveDeletedObjects(businessEntities);

                foreach (var be in businessEntities)
                {
                    var linkedFoldersIds = await GetLinkedFoldersIds(be.Id);
                    await restorationDataAccess.SaveDeletedObjectLinkedFolders(be.Id, linkedFoldersIds);
                }
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error while saving deleted shortcodes.", ex);
            }
        }

        #endregion


    }
}