//
// Project: Mark5.Mobile.Common
// File: ShortcodesDataAccess.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
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
            await shortcodesDatabase.RunInConnectionAsync(c =>
            {
                var shortcodePreviewIds = shortcodePreviews.Select(cp => cp.Id).ToList();

                if (clean)
                {
                    c.Table<FolderShortcodeLink>()
                     .Delete(fdl => fdl.FolderId == folder.Id && shortcodePreviewIds.Contains(fdl.ShortcodeId));
                }

                c.InsertOrReplace(shortcodePreviews.Select(cp => new FolderShortcodeLink { FolderId = folder.Id, ShortcodeId = cp.Id }));
                c.InsertOrReplace(shortcodePreviews);
            });
        }
        public async Task<List<ShortcodePreview>> GetShortcodePreviewsAsync(Folder folder, int startRowId, int maxItems)
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

        public async Task SaveShortcodeAsync(Shortcode shortcode)
        {
            await shortcodesDatabase.RunInConnectionAsync(c =>
            {
                c.InsertOrReplace(shortcode);
            });
        }

        public async Task<Shortcode> GetShortcodeAsync(int shortcodeId)
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
    }
}

