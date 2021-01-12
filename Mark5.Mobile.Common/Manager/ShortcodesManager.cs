using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Containers;
using Mark5.Mobile.Common.Model.Converters;
using Mark5.Mobile.Common.Model.Exceptions;
using Mark5.Mobile.Common.Model.HubMessages;
using Mark5.Mobile.Common.Utilities;
using Mark5.ServiceReference.AppService;
using DataContract = Mark5.ServiceReference.DataContract;

namespace Mark5.Mobile.Common.Manager
{
    class ShortcodesManager : AbstractManager, IShortcodesManager
    {
        public int MaxToFetch { get; set; } = 100;

        readonly IShortcodesDataAccess shortcodesDataAccess;

        public ShortcodesManager(ConnectionInfo connectionInfo, IAppServiceProxy appServiceProxy, IShortcodesDataAccess shortcodesDataAccess)
            : base(connectionInfo, appServiceProxy)
        {
            this.shortcodesDataAccess = shortcodesDataAccess;
        }

        public async Task<List<ShortcodePreview>> GetShortcodePreviewsAsync(Folder folder, int startRowId = -1, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetShortcodePreviewsAsync(new DataContract.GetShortcodePreviewsParameters
                {
                    Token = Token,
                    FolderId = folder.Id,
                    StartRowId = startRowId,
                    MaxToFetch = MaxToFetch
                });

                var shortcodePreviews = result.ShortcodePreviews.WhereNotNull().OrderBy(cp => cp.RowId).Select(sp => sp.Convert()).ToList();

                await shortcodesDataAccess.SaveShortcodePreviewsAsync(folder, shortcodePreviews, startRowId == -1);

                return shortcodePreviews;
            }

            if (sourceType == SourceType.Local)
                return await shortcodesDataAccess.GetShortcodePreviewsAsync(folder, startRowId, MaxToFetch);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public void GetAllShortcodePreviews(Folder folder, Action<List<ShortcodePreview>> callback, Action finishedCallback, Action<Exception> errorCallback, int startRowId = -1, CancellationToken ct = default(CancellationToken), SourceType sourceType = SourceType.Auto)
        {
            Task.Run(async () =>
                {
                    var stopLoop = false;

                    while (!stopLoop && !ct.IsCancellationRequested)
                    {
                        var previews = await GetShortcodePreviewsAsync(folder, startRowId, sourceType);

                        if (ct.IsCancellationRequested)
                            continue;

                        callback(previews);

                        if (previews.Count > 0)
                            startRowId = previews.LastOrDefault()?.RowId + 1 ?? -1;
                        stopLoop = previews.Count < MaxToFetch;
                    }
                })
                .ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                            errorCallback(t.Exception.InnerException);
                        finishedCallback();
                    },
                    TaskScheduler.FromCurrentSynchronizationContext());
        }

        public async Task<Shortcode> GetShortcodeAsync(Folder folder, int shortcodeId, SourceType sourceType = SourceType.Auto)
        {
            return await GetShortcodeAsync(folder?.Id, shortcodeId, sourceType);
        }

        public async Task<Shortcode> GetShortcodeAsync(int? folderId, int shortcodeId, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetShortcodeAsync(new DataContract.GetShortcodeParameters
                {
                    Token = Token,
                    FolderId = folderId ?? -1,
                    ShortcodeId = shortcodeId,
                    IncludePreview = false
                });

                var shortcode = result.Shortcode.Convert();

                await shortcodesDataAccess.SaveShortcodeAsync(shortcode);

                return shortcode;
            }

            if (sourceType == SourceType.Local)
                return await shortcodesDataAccess.GetShortcodeAsync(shortcodeId);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<ShortcodeContainer> GetShortcodeWithPreviewAsync(Folder folder, int shortcodeId, SourceType sourceType = SourceType.Auto)
        {
            return await GetShortcodeWithPreviewAsync(folder?.Id, shortcodeId, sourceType);
        }

        public async Task<ShortcodeContainer> GetShortcodeWithPreviewAsync(int? folderId, int shortcodeId, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetShortcodeAsync(new DataContract.GetShortcodeParameters
                {
                    Token = Token,
                    FolderId = folderId ?? -1,
                    ShortcodeId = shortcodeId,
                    IncludePreview = true
                });

                var shortcodePreview = result.ShortcodePreview.Convert();
                var shortcode = result.Shortcode.Convert();

                var container = new ShortcodeContainer(shortcodePreview, shortcode);

                await shortcodesDataAccess.SaveShortcodeWithPreviewAsync(container);

                return container;
            }

            if (sourceType == SourceType.Local)
                return await shortcodesDataAccess.GetShortcodeWithPreviewAsync(shortcodeId);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<bool> CreateOrUpdateShortcodeAsync(Shortcode shortcode, ShortcodePreview shortcodePreview, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.CreateOrUpdateShortcodeAsync(new DataContract.CreateOrUpdateShortcodeParameters
                {
                    Token = Token,
                    Shortcode = shortcode.Convert(),
                    ShortcodePreview = shortcodePreview.Convert(),
                });

                shortcode.Id = shortcodePreview.Id = result.Id;
                shortcode.Guid = shortcodePreview.Guid = result.Guid;

                if (result.Updated)
                {
                    CommonConfig.UsageAnalytics.LogEvent(new EditShortcodeEvent());
                    CommonConfig.MessengerHub.Publish(new EntityPreviewChangedMessage(this, shortcodePreview));
                }
                else
                {
                    CommonConfig.UsageAnalytics.LogEvent(new AddShortcodeEvent());
                }

                return result.Updated;
            }

            if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided");
        }

        public async Task<List<Recipient>> GetSuggestions(string phrase)
        {
            var result = await shortcodesDataAccess.GetSuggestions(phrase);
            return result;
        }
    }
}