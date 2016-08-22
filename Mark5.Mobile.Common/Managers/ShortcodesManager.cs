//
// Project: Mark5.Mobile.Common
// File: ShortcodesManager.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Converters;
using Mark5.ServiceReference.AppService;
using DataContract = Mark5.ServiceReference.DataContract;

namespace Mark5.Mobile.Common.Managers
{

    class ShortcodesManager : AbstractManager, IShortcodesManager
    {
        const int NumbeOfShortcodesToFetchPerCall = 250;
        readonly IShortcodesDataAccess shortcodesDataAccess;

        public ShortcodesManager(ConnectionInfo connectionInfo, IAppServiceProxy appServiceProxy, IShortcodesDataAccess shortcodesDataAccess)
            : base(connectionInfo, appServiceProxy)
        {
            this.shortcodesDataAccess = shortcodesDataAccess;
        }

        public async Task<List<ShortcodePreview>> GetShortcodePreviewsAsync(Folder folder, int startRowId = -1, int maxItems = 500, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetShortcodePreviewsAsync(new DataContract.GetShortcodePreviewsParameters
                {
                    Token = Token,
                    FolderId = folder.Id,
                    StartRowId = startRowId,
                    MaxToFetch = maxItems
                });

                var shortcodePreviews = result.ShortcodePreviews.WhereNotNull().OrderBy(cp => cp.RowId).Select(sp => sp.Convert()).ToList();

                await shortcodesDataAccess.SaveShortcodePreviewsAsync(folder, shortcodePreviews, startRowId == -1);

                return shortcodePreviews;
            }

            if (sourceType == SourceType.Local)
            {
                return await shortcodesDataAccess.GetShortcodePreviewsAsync(folder, startRowId, maxItems);
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task GetAllShortcodePreviewsAsync(Folder folder, Func<List<ShortcodePreview>, Task> handler, CancellationToken ct = default(CancellationToken), SourceType sourceType = SourceType.Auto)
        {
            var startId = 0;
            var stopLoop = false;

            while (!stopLoop && !ct.IsCancellationRequested)
            {
                var previews = await GetShortcodePreviewsAsync(folder, startId, NumbeOfShortcodesToFetchPerCall, sourceType);
                await handler(previews);

                startId += NumbeOfShortcodesToFetchPerCall;
                stopLoop = previews.Count < NumbeOfShortcodesToFetchPerCall;
            }
        }

        public async Task<Shortcode> GetShortcodeAsync(Folder folder, int shortcodeId, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetShortcodeAsync(new DataContract.GetShortcodeParameters
                {
                    Token = Token,
                    FolderId = folder.Id,
                    ShortcodeId = shortcodeId,
                    IncludePreview = false
                });

                var shortcode = result.Shortcode.Convert();

                await shortcodesDataAccess.SaveShortcodeAsync(shortcode);

                return shortcode;
            }

            if (sourceType == SourceType.Local)
            {
                return await shortcodesDataAccess.GetShortcodeAsync(shortcodeId);
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }
    }
}

