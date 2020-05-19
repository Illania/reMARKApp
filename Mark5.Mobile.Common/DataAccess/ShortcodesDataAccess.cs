using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess.Exceptions;
using Mark5.Mobile.Common.Database;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Containers;
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

    }
}