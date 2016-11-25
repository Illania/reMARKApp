//
// Project: Mark5.Mobile.Droid
// File: DocumentsListFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Common.BusMesseges;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{

    public class DocumentsListFragment : RetainableStateFragment, ActionMode.ICallback, MenuItemCompat.IOnActionExpandListener, SearchView.IOnQueryTextListener
    {

        const int AutoRefreshIntervalMs = 5 * 1000; // 5 seconds

        public Folder Folder { get; set; }
        public Action CloseRequest { get; set; }

        DocumentsListAdapter CurrentAdapter
        {
            get { return (DocumentsListAdapter)recyclerView.GetAdapter(); }
        }

        bool refreshing;

        CoordinatorLayout coordinatorLayout;
        SwipeRefreshLayout refreshLayout;
        RecyclerView recyclerView;
        DocumentsListAdapter adapter;
        DocumentsListAdapter searchAdapter;
        ActionMode actionMode;
        SearchView searchView;

        bool shouldNotifyAdapter;
        bool shouldNotifySearchAdapter;

        readonly Handler searchHandler = new Handler();

        AutoRefreshWorker autoRefreshWorker;

        #region Fragment overrides

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(DocumentsListFragment)} [folder.id={Folder?.Id}, folder.name={Folder?.Name}]...");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            coordinatorLayout = (CoordinatorLayout)container.Parent.Parent;

            refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.SetColorSchemeResources(Resource.Color.lightbrown, Resource.Color.brown);
            refreshLayout.Refresh += async (sender, e) =>
            {
                actionMode?.Finish();
                actionMode = null;

                await RefreshData(force: true);
            };

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));

            adapter = new DocumentsListAdapter(Activity, async (startId) => await RefreshData(startId));
            adapter.ItemClicked += Adapter_ItemClicked;
            adapter.ItemLongClicked += Adapter_ItemLongClicked;
            recyclerView.SetAdapter(adapter);

            searchAdapter = new DocumentsListAdapter(Activity);
            searchAdapter.ItemClicked += Adapter_ItemClicked;
            searchAdapter.ItemLongClicked += Adapter_ItemLongClicked;

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = Folder?.Name;
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = GetString(Resource.String.documents);

            CommonConfig.Logger.Info($"Created {nameof(DocumentsListFragment)} [folder.id={Folder?.Id}, folder.name={Folder?.Name}]");
        }

        public override async void OnResume()
        {
            base.OnResume();

            CommonConfig.Logger.Info($"Resuming {nameof(DocumentsListFragment)} [folder.id={Folder?.Id}, folder.name={Folder?.Name}]...");

            if (adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"No elements - will refresh...");

                await RefreshData();
            }

            if (shouldNotifyAdapter)
            {
                shouldNotifyAdapter = false;
                actionMode?.Finish();
                actionMode = null;
                adapter.NotifyDataSetChanged();
            }
            if (shouldNotifySearchAdapter)
            {
                shouldNotifySearchAdapter = false;
                actionMode?.Finish();
                actionMode = null;
                searchAdapter.NotifyDataSetChanged();
            }

            if (!IsAdded || IsDetached || IsRemoving) return;

            CommonConfig.Logger.Info($"Starting automatic refresh...");

            autoRefreshWorker?.Stop();
            autoRefreshWorker = new AutoRefreshWorker(AutoRefreshData, () => { return adapter?.Items?.FirstOrDefault(); }, AutoRefreshIntervalMs);
            autoRefreshWorker.Start();

            CommonConfig.Logger.Info($"Started automatic refresh");
        }

        public override void OnPause()
        {
            base.OnPause();

            CommonConfig.Logger.Info($"Pausing {nameof(DocumentsListFragment)} [folder.id={Folder?.Id}, folder.name={Folder?.Name}]...");

            CommonConfig.Logger.Info($"Stopping automatic refresh...");

            autoRefreshWorker?.Stop();

            CommonConfig.Logger.Info($"Stopped automatic refresh");
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.menu_main, menu);

            var searchItem = menu.FindItem(Resource.Id.action_search);
            MenuItemCompat.SetOnActionExpandListener(searchItem, this);
            searchView = (SearchView)MenuItemCompat.GetActionView(searchItem);
            searchView.QueryHint = GetString(Resource.String.filter);
            searchView.SetOnQueryTextListener(this);
        }

        #endregion

        #region RetainableStateFragment overrides

        public override IRetainableState OnRetainInstanceState()
        {
            CommonConfig.Logger.Info($"Retaining state [folder.id={Folder?.Id}, folder.name={Folder?.Name}, documentPreviews.Count={adapter?.ItemCount}/{adapter?.SelectedItemCount}]...");

            return new DocumentsListFragmentState
            {
                Folder = Folder,
                DocumentPreviews = adapter.Items,
                SelectedDocumentPreviews = adapter.SelectedItems
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var dlfs = restoredState as DocumentsListFragmentState;
            if (dlfs != null)
            {
                CommonConfig.Logger.Info($"Restoring state [dlfs.folder.id={dlfs.Folder?.Id}, dlfs.items.count={dlfs.DocumentPreviews?.Count}, dlfs.selectedItems.count={dlfs.SelectedDocumentPreviews?.Count}]...");

                Folder = dlfs.Folder;
                adapter.AppendItems(dlfs.DocumentPreviews);

                if (dlfs.SelectedDocumentPreviews.Count > 0)
                {
                    actionMode?.Finish();
                    actionMode = Activity.StartActionMode(this);

                    adapter.SetSelected(dlfs.SelectedDocumentPreviews, true);
                    actionMode.Title = adapter.SelectedItemCount.ToString();
                    actionMode.Invalidate();
                }
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(DocumentsListFragment)} [folder.id={Folder.Id}, folder.name={Folder.Name}]";
        }

        #endregion

        #region Refreshing

        async Task AutoRefreshData(int endId)
        {
            try
            {
                CommonConfig.Logger.Debug($"Attempting automatic refresh [endId={endId}, !isAdded={!IsAdded}, isDetached={IsDetached}, isRemoving={IsRemoving}, refreshing={refreshing}]...");

                if (!IsAdded || IsDetached || IsRemoving) return;
                if (refreshing) return;

                refreshing = true;

                CommonConfig.Logger.Debug($"Automatic refresh running...");

                var documents = await Managers.DocumentsManager.GetDocumentPreviewsAsync(Folder, endId: endId);

                if (documents.Count > 0)
                {
                    CommonConfig.Logger.Info($"Received {documents?.Count} new documents");

                    Snackbar.Make(coordinatorLayout, Resources.GetQuantityString(Resource.Plurals.new_documents_received, documents.Count, documents.Count), Snackbar.LengthShort).Show();

                    Managers.DownloadManager.Notify(ObjectType.Document, Folder.Id);

                    Activity?.RunOnUiThread(() =>
                    {
                        adapter?.PrependItems(documents);
                    });
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Automatic refresh failed [endId={endId}]", ex);
            }
            finally
            {
                refreshing = false;

                CommonConfig.Logger.Debug($"Automatic refresh finished");
            }
        }

        async Task RefreshData(int startId = -1, int endId = -1, bool force = false)
        {
            try
            {
                CommonConfig.Logger.Info($"Attempting refresh [startId={startId}, endId={endId}, force={force}]...");

                if (refreshing) return;

                refreshing = true;
                refreshLayout.Refreshing = true;

                CommonConfig.Logger.Info($"Refresh running...");

                if (force)
                {
                    adapter.Clear();
                }

                var documentPreviews = await Managers.DocumentsManager.GetDocumentPreviewsAsync(Folder, startId, endId);

                Managers.DownloadManager.Notify(ObjectType.Document, Folder.Id);
                adapter.AppendItems(documentPreviews);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Downloading documents failed [folder.name={Folder?.Name}, folder.id={Folder?.Id}, startId={startId}, endId={endId}, force={force}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);

                if (CloseRequest != null) CloseRequest();
            }
            finally
            {
                refreshLayout.Refreshing = false;
                refreshing = false;

                CommonConfig.Logger.Info($"Refresh finished");
            }
        }

        #endregion

        #region Adapter callbacks

        void Adapter_ItemClicked(object sender, DocumentPreview documentPreview)
        {
            if (actionMode == null)
            {
                var i = new Intent(Activity, typeof(DocumentActivity));
                i.PutExtra(DocumentActivity.FolderIntentKey, SerializationUtils.Serialize(Folder));
                i.PutExtra(DocumentActivity.DocumentPreviewIntentKey, SerializationUtils.Serialize(documentPreview));
                StartActivity(i);
            }
            else
            {
                var currentAdapter = (DocumentsListAdapter)recyclerView.GetAdapter();
                currentAdapter.SetSelected(documentPreview, !currentAdapter.IsSelected(documentPreview));

                if (currentAdapter.SelectedItemCount < 1)
                {
                    actionMode.Finish();
                }
                else
                {
                    actionMode.Title = currentAdapter.SelectedItemCount.ToString();
                    actionMode.Invalidate();
                }
            }
        }

        void Adapter_ItemLongClicked(object sender, DocumentPreview documentPreview)
        {
            if (actionMode == null)
            {
                actionMode = Activity.StartActionMode(this);
            }

            Adapter_ItemClicked(sender, documentPreview);
        }

        #endregion

        #region Action mode

        static class MenuItemActions
        {
            public const int MarkAsRead = 10;
            public const int MarkAsUnread = 11;
            public const int Reply = 20;
            public const int ReplyAll = 21;
            public const int Forward = 22;
            public const int CopyToWorktray = 30;
            public const int CopyToFolder = 40;
            public const int MoveToFolder = 41;
            public const int SetPriority = 50;
            public const int Categories = 60;
            public const int DeleteFromFolder = 70;
            public const int Delete = 71;
        }

        bool ActionMode.ICallback.OnPrepareActionMode(ActionMode mode, IMenu menu)
        {
            menu.Clear();

            var currentAdapter = (DocumentsListAdapter)recyclerView.GetAdapter();

            if (currentAdapter.SelectedItems.Any(dp => !dp.IsReadByCurrent))
            {
                menu.Add(Menu.None, MenuItemActions.MarkAsRead, MenuItemActions.MarkAsRead, Resource.String.mark_as_read);
            }

            if (currentAdapter.SelectedItems.Any(dp => dp.IsReadByCurrent))
            {
                menu.Add(Menu.None, MenuItemActions.MarkAsUnread, MenuItemActions.MarkAsUnread, Resource.String.marks_as_unread);
            }

            if (currentAdapter.SelectedItemCount == 1)
            {
                menu.Add(Menu.None, MenuItemActions.Reply, MenuItemActions.Reply, Resource.String.reply);
                menu.Add(Menu.None, MenuItemActions.ReplyAll, MenuItemActions.ReplyAll, Resource.String.reply_all);
                menu.Add(Menu.None, MenuItemActions.Forward, MenuItemActions.Forward, Resource.String.forward);
            }

            menu.Add(Menu.None, MenuItemActions.CopyToWorktray, MenuItemActions.CopyToWorktray, Resource.String.copy_to_worktray);
            menu.Add(Menu.None, MenuItemActions.CopyToFolder, MenuItemActions.CopyToFolder, Resource.String.copy_to_folder);

            if (Folder.InternalType == FolderInternalType.FilterView
                || Folder.InternalType == FolderInternalType.Static
                || Folder.InternalType == FolderInternalType.Worktray)
            {
                menu.Add(Menu.None, MenuItemActions.MoveToFolder, MenuItemActions.MoveToFolder, Resource.String.move_to_folder);
            }

            menu.Add(Menu.None, MenuItemActions.SetPriority, MenuItemActions.SetPriority, Resource.String.set_priority);

            if (currentAdapter.SelectedItemCount == 1)
            {
                menu.Add(Menu.None, MenuItemActions.Categories, MenuItemActions.Categories, Resource.String.categories);
            }

            if (Folder.InternalType == FolderInternalType.FilterView
                || Folder.InternalType == FolderInternalType.Static
                || Folder.InternalType == FolderInternalType.Worktray)
            {
                menu.Add(Menu.None, MenuItemActions.DeleteFromFolder, MenuItemActions.DeleteFromFolder, Resource.String.delete_from_folder);
            }

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator
                || ServerConfig.SystemSettings.DocumentsModuleInfo.Permissions.DeleteAllowed)
            {
                menu.Add(Menu.None, MenuItemActions.Delete, MenuItemActions.Delete, Resource.String.delete);
            }

            return true;
        }

        bool ActionMode.ICallback.OnCreateActionMode(ActionMode mode, IMenu menu)
        {
            return true;
        }

        bool ActionMode.ICallback.OnActionItemClicked(ActionMode mode, IMenuItem item)
        {
            var selectedDocumentPreviews = CurrentAdapter.SelectedItems;

            if (item.ItemId == MenuItemActions.Categories)
            {
                var i = new Intent(Activity, typeof(CategoriesListActivity));
                i.PutExtra(CategoriesListActivity.BusinessEntityPreviewIntentKey, SerializationUtils.Serialize(selectedDocumentPreviews.First()));
                StartActivity(i);

                return true;
            }
            if (item.ItemId == MenuItemActions.CopyToFolder)
            {
                var i = new Intent(Activity, typeof(FolderListSelectionActivity));
                i.PutExtra(FolderListSelectionActivity.ModeIntentKey, (int)FolderListSelectionActivity.ModeType.CopyToFolderMode);
                i.PutExtra(FolderListSelectionActivity.ModuleIntentKey, SerializationUtils.Serialize(ModuleType.Documents));
                i.PutExtra(FolderListSelectionActivity.BusinessEntitiesIntentKey, SerializationUtils.Serialize(selectedDocumentPreviews.Select(sp => sp).Cast<IBusinessEntity>().ToList()));
                StartActivity(i);

                return true;
            }
            if (item.ItemId == MenuItemActions.MoveToFolder)
            {
                var i = new Intent(Activity, typeof(FolderListSelectionActivity));
                i.PutExtra(FolderListSelectionActivity.ModeIntentKey, (int)FolderListSelectionActivity.ModeType.MoveToFolderMode);
                i.PutExtra(FolderListSelectionActivity.ModuleIntentKey, SerializationUtils.Serialize(ModuleType.Documents));
                i.PutExtra(FolderListSelectionActivity.BusinessEntitiesIntentKey, SerializationUtils.Serialize(selectedDocumentPreviews.Select(sp => sp).Cast<IBusinessEntity>().ToList()));
                i.PutExtra(FolderListSelectionActivity.FromFolderIntentKey, SerializationUtils.Serialize(Folder));
                StartActivity(i);

                return true;
            }
            if (item.ItemId == MenuItemActions.CopyToWorktray)
            {
                CopyToWorktrayAction();
            }

            return base.OnOptionsItemSelected(item);
        }

        void ActionMode.ICallback.OnDestroyActionMode(ActionMode mode)
        {
            var currentAdapter = (DocumentsListAdapter)recyclerView.GetAdapter();
            currentAdapter.ClearSelections();
            actionMode = null;
        }

        async void CopyToWorktrayAction()
        {
            var option = await Dialogs.ShowListDialog(Context, Resource.String.copy_to_worktray, Resource.Array.copy_to_worktray_options);

            if (option == 0)
            {

            }

            if (option == 1)
            {

                StartActivity(CopyToUserWorktrayActivity.CreateIntent(Activity, CurrentAdapter.SelectedItems.Cast<IBusinessEntity>().ToList()));
            }
        }

        #endregion

        #region Filtering

        bool MenuItemCompat.IOnActionExpandListener.OnMenuItemActionExpand(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_search)
            {
                refreshLayout.Enabled = false;
                adapter.ClearSelections();
                recyclerView.SwapAdapter(searchAdapter, true);
                return true;
            }

            return false;
        }

        bool MenuItemCompat.IOnActionExpandListener.OnMenuItemActionCollapse(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_search)
            {
                searchHandler.RemoveCallbacksAndMessages(null);
                searchAdapter.Clear();
                recyclerView.SwapAdapter(adapter, true);
                refreshLayout.Enabled = true;
                return true;
            }

            return false;
        }

        bool SearchView.IOnQueryTextListener.OnQueryTextChange(string newText)
        {
            searchHandler.RemoveCallbacksAndMessages(null);
            searchHandler.PostDelayed(() =>
            {
                if (string.IsNullOrWhiteSpace(newText))
                {
                    searchAdapter.Clear();
                }
                else
                {
                    searchAdapter.ReplaceItems(adapter.Items.Where(dp => MatchesQuery(dp, newText)).ToList());
                }
            }, 500);
            return false;
        }

        bool SearchView.IOnQueryTextListener.OnQueryTextSubmit(string query)
        {
            return false;
        }

        static bool MatchesQuery(DocumentPreview dp, string query)
        {
            if (dp.Subject.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                return true;
            }
            if (dp.Preview.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                return true;
            }
            if (dp.Addresses.Any(da => da.Name.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0))
            {
                return true;
            }
            if (dp.Addresses.Any(da => da.Address.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0))
            {
                return true;
            }
            if (dp.Categories.Any(da => da.Name.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0))
            {
                return true;
            }

            return false;
        }

        #endregion

        #region State

        class DocumentsListFragmentState : IRetainableState
        {

            public Folder Folder { get; set; }

            public List<DocumentPreview> DocumentPreviews { get; set; }

            public List<DocumentPreview> SelectedDocumentPreviews { get; set; }
        }

        #endregion

        #region Messenger hub related

        public void UpdateReadStatus(DocumentPreviewReadStatusChangedMessage m)
        {
            var position = adapter.GetPosition(m.DocumentPreviewId);
            if (position >= 0)
            {
                shouldNotifyAdapter = true;
                var dp = adapter.Items[position];
                dp.IsReadByCurrent = m.IsReadByCurrent;
                dp.IsReadByAnyone = m.IsReadByAnyone;
            }

            position = searchAdapter.GetPosition(m.DocumentPreviewId);
            if (position >= 0)
            {
                shouldNotifySearchAdapter = true;
                var dp = searchAdapter.Items[position];
                dp.IsReadByCurrent = m.IsReadByCurrent;
                dp.IsReadByAnyone = m.IsReadByAnyone;
            }
        }

        public void UpdateCategories(DocumentPreviewCategoriesChangedMessage m)
        {
            var position = adapter.GetPosition(m.DocumentPreviewId);
            if (position >= 0)
            {
                shouldNotifyAdapter = true;
                var dp = adapter.Items[position];
                dp.Categories.Clear();
                dp.Categories.AddRange(m.Categories);
            }

            position = searchAdapter.GetPosition(m.DocumentPreviewId);
            if (position >= 0)
            {
                shouldNotifySearchAdapter = true;
                var dp = searchAdapter.Items[position];
                dp.Categories.Clear();
                dp.Categories.AddRange(m.Categories);
            }
        }

        public void UpdateCommentsCount(DocumentPreviewCommentCountChangedMessage m)
        {
            var position = adapter.GetPosition(m.DocumentPreviewId);
            if (position >= 0)
            {
                shouldNotifyAdapter = true;
                var dp = adapter.Items[position];
                dp.CommentsCount = m.CommentsCount;
            }

            position = searchAdapter.GetPosition(m.DocumentPreviewId);
            if (position >= 0)
            {
                shouldNotifySearchAdapter = true;
                var dp = searchAdapter.Items[position];
                dp.CommentsCount = m.CommentsCount;
            }
        }

        public void RemoveMovedEntities(EntityMovedFromFolderMessage m)
        {
            foreach (var entityId in m.EntitiesId)
            {
                var position = adapter.GetPosition(entityId);
                if (position >= 0)
                {
                    shouldNotifyAdapter = true;
                    adapter.RemoveItemsAtIndex(position);
                    adapter.ClearSelections(false);
                }

                position = searchAdapter.GetPosition(entityId);
                if (position >= 0)
                {
                    shouldNotifySearchAdapter = true;
                    searchAdapter.RemoveItemsAtIndex(position);
                    adapter.ClearSelections(false);
                }
            }

        }

        #endregion

        #region RecyclerView Adapter/ViewHolder

        class DocumentsListAdapter : RecyclerView.Adapter
        {

            public static class ViewType
            {
                public const int DocumentView = 0;
                public const int ExternalDocumentView = 1;
            }

            public List<DocumentPreview> Items
            {
                get
                {
                    return documentPreviewsInView.ToList();
                }
            }

            public List<DocumentPreview> SelectedItems
            {
                get
                {
                    return selectedDocumentsInView.Values.ToList();
                }
            }

            public override int ItemCount
            {
                get
                {
                    return documentPreviewsInView.Count;
                }
            }

            public int SelectedItemCount
            {
                get
                {
                    return selectedDocumentsInView.Count;
                }
            }

            readonly List<DocumentPreview> documentPreviewsInView = new List<DocumentPreview>(1000);
            readonly Dictionary<int, DocumentPreview> selectedDocumentsInView = new Dictionary<int, DocumentPreview>();
            readonly Action<int> loadMoreAction;
            readonly Context context;

            public event EventHandler<DocumentPreview> ItemClicked = delegate { };
            public event EventHandler<DocumentPreview> ItemLongClicked = delegate { };

            bool unreadIndicatorMe = PlatformConfig.Preferences.UnreadIndicatorMe;
            bool compactList = PlatformConfig.Preferences.CompactDocumentsList;

            public DocumentsListAdapter(Context context)
            {
                this.context = context;
            }

            public DocumentsListAdapter(Context context, Action<int> loadMoreAction)
            {
                this.context = context;
                this.loadMoreAction = loadMoreAction;
            }

            public override int GetItemViewType(int position)
            {
                return documentPreviewsInView[position].Direction == DocumentDirection.External ? ViewType.ExternalDocumentView : ViewType.DocumentView;
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                if (holder is DocumentPreviewViewHolder)
                {
                    var dpvh = holder as DocumentPreviewViewHolder;
                    var dp = documentPreviewsInView[position];

                    dpvh.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, dp)));
                    dpvh.ItemView.SetOnLongClickListener(new ActionOnLongClickListener(() => ItemLongClicked(this, dp)));

                    if (dp.Direction == DocumentDirection.Incoming)
                    {
                        var address = dp.Addresses.FirstOrDefault(da => da.AddressType == DocumentAddressType.From);
                        dpvh.Recipent = address == null ? string.Empty : string.IsNullOrWhiteSpace(address.Name) ? address.Address : address.Name;
                    }
                    else
                    {
                        var address = dp.Addresses.Where(da => da.AddressType == DocumentAddressType.To || da.AddressType == DocumentAddressType.Cc || da.AddressType == DocumentAddressType.Bcc).OrderBy(da => da.AddressType).FirstOrDefault();
                        dpvh.Recipent = address == null ? string.Empty : string.IsNullOrWhiteSpace(address.Name) ? address.Address : address.Name;
                    }

                    dpvh.Subject = string.IsNullOrWhiteSpace(dp.Subject) ? context.GetString(Resource.String.no_subject) : dp.Subject;
                    dpvh.Date = dp.DateReceivedTimestamp
                        .ConvertTimestampMillisecondsToDateTime()
                        .ConvertUtcToServerTime()
                        .ConvertDateTimeToTimestampMilliseconds()
                        .FormatServerTimestampAsCompactShortDateTimeString(context);
                    dpvh.Preview = string.IsNullOrWhiteSpace(dp.Preview) ? context.GetString(Resource.String.no_content) : Regex.Replace(dp.Preview, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
                    dpvh.Categories = dp.Categories;
                    dpvh.IncomingIndicator = dp.Direction == DocumentDirection.Incoming;
                    dpvh.OutgoingIndicator = dp.Direction == DocumentDirection.Outgoing;
                    dpvh.DraftIndicator = dp.Direction == DocumentDirection.Draft;
                    dpvh.UnreadIndicator = unreadIndicatorMe ? !dp.IsReadByCurrent : !dp.IsReadByAnyone;
                    dpvh.AttachmentIndicator = dp.AttachmentsCount > 0;
                    dpvh.CommentIndicator = dp.CommentsCount > 0;

                    if (compactList)
                    {
                        dpvh.Preview = null;
                        dpvh.AttachmentIndicator = false;
                        dpvh.CommentIndicator = false;
                    }

                    dpvh.Selected = selectedDocumentsInView.ContainsKey(dp.Id);

                    if (loadMoreAction != null && position == ItemCount - 1)
                    {
                        loadMoreAction(dp.Id);
                    }
                }
                else if (holder is ExternalDocumentPreviewViewHolder)
                {
                    var edpvh = holder as ExternalDocumentPreviewViewHolder;
                    var dp = documentPreviewsInView[position];

                    edpvh.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, dp)));
                    edpvh.ItemView.SetOnLongClickListener(new ActionOnLongClickListener(() => ItemLongClicked(this, dp)));

                    edpvh.Date = dp.DateReceivedTimestamp
                    .ConvertTimestampMillisecondsToDateTime()
                    .ConvertUtcToServerTime()
                    .ConvertDateTimeToTimestampMilliseconds()
                    .FormatServerTimestampAsCompactShortDateTimeString(context);
                    edpvh.Name = string.IsNullOrWhiteSpace(dp.Subject) ? context.GetString(Resource.String.no_subject) : dp.Subject;
                    edpvh.Preview = string.IsNullOrWhiteSpace(dp.Preview) ? context.GetString(Resource.String.no_content) : Regex.Replace(dp.Preview, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
                    edpvh.Categories = dp.Categories;
                    edpvh.CommentIndicator = dp.CommentsCount > 0;

                    edpvh.Selected = selectedDocumentsInView.ContainsKey(dp.Id);

                    if (loadMoreAction != null && position == ItemCount - 1)
                    {
                        loadMoreAction(dp.Id);
                    }
                }
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                if (viewType == ViewType.DocumentView)
                {
                    var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_documents, parent, false);
                    return new DocumentPreviewViewHolder(itemView);
                }

                if (viewType == ViewType.ExternalDocumentView)
                {
                    var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_documents_external, parent, false);
                    return new ExternalDocumentPreviewViewHolder(itemView);
                }

                return null;
            }

            public void PrependItems(List<DocumentPreview> items)
            {
                var count = items.Count;
                documentPreviewsInView.InsertRange(0, items);
                NotifyItemRangeInserted(0, count);
            }

            public void AppendItems(List<DocumentPreview> items)
            {
                var count = documentPreviewsInView.Count;
                documentPreviewsInView.AddRange(items);
                NotifyItemRangeInserted(count, items.Count);
            }

            public void RemoveItemsAtIndex(int index)
            {
                documentPreviewsInView.RemoveAt(index);
            }

            public void ReplaceItems(List<DocumentPreview> items)
            {
                Clear();
                AppendItems(items);
            }

            public void Clear()
            {
                var size = documentPreviewsInView.Count;
                documentPreviewsInView.Clear();
                selectedDocumentsInView.Clear();
                NotifyItemRangeRemoved(0, size);
            }

            public bool IsSelected(DocumentPreview documentPreview)
            {
                return selectedDocumentsInView.ContainsKey(documentPreview.Id);
            }

            public void SetSelected(List<DocumentPreview> documentPreviews, bool selected)
            {
                foreach (var document in documentPreviews)
                {
                    SetSelected(document, selected);
                }
            }

            public void SetSelected(DocumentPreview documentPreview, bool selected)
            {
                var position = GetPosition(documentPreview);
                if (position < 0) return;

                if (selected)
                {
                    selectedDocumentsInView[documentPreview.Id] = documentPreview;
                }
                else
                {
                    selectedDocumentsInView.Remove(documentPreview.Id);
                }
                NotifyItemChanged(position);
            }

            public void ClearSelections(bool notify = true)
            {
                var documents = selectedDocumentsInView.Values.ToArray();
                selectedDocumentsInView.Clear();
                if (notify)
                {
                    foreach (var document in documents)
                    {
                        NotifyItemChanged(GetPosition(document));
                    }
                }
            }

            public int GetPosition(int documentPreviewId)
            {
                var position = -1;
                for (var i = 0; i < documentPreviewsInView.Count; i++)
                {
                    if (documentPreviewsInView[i].Id == documentPreviewId)
                    {
                        position = i;
                        break;
                    }
                }
                return position;
            }

            public int GetPosition(DocumentPreview documentPreview)
            {
                return GetPosition(documentPreview.Id);
            }
        }

        class DocumentPreviewViewHolder : RecyclerView.ViewHolder
        {

            public string Recipent
            {
                set
                {
                    recipentTextView.Text = value;
                }
            }

            public string Date
            {
                set
                {
                    dateTextView.Text = value;
                }
            }

            public string Subject
            {
                set
                {
                    subjectTextView.Text = value;
                }
            }

            public string Preview
            {
                set
                {
                    if (value == null)
                    {
                        previewTextView.Text = string.Empty;
                        previewTextView.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        previewTextView.Text = value;
                        previewTextView.Visibility = ViewStates.Visible;
                    }
                }
            }

            public List<Category> Categories
            {
                set
                {
                    categoriesLayout.RemoveAllViews();

                    foreach (var hexColor in value.Select(c => c.HexColor))
                    {
                        var view = new View(ItemView.Context)
                        {
                            LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent, 1f),
                            Background = new ColorDrawable(Color.ParseColor(hexColor))
                        };
                        categoriesLayout.AddView(view);
                    }
                }
            }

            public bool IncomingIndicator
            {
                set
                {
                    incomingImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
                }
            }

            public bool OutgoingIndicator
            {
                set
                {
                    outgoingImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
                }
            }

            public bool DraftIndicator
            {
                set
                {
                    draftImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
                }
            }

            public bool UnreadIndicator
            {
                set
                {
                    unreadImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
                }
            }

            public bool AttachmentIndicator
            {
                set
                {
                    attachmentImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
                }
            }

            public bool CommentIndicator
            {
                set
                {
                    commentImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
                }
            }

            public bool Selected
            {
                set
                {
                    selectedOverlay.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
                }
            }

            readonly AppCompatTextView recipentTextView;
            readonly AppCompatTextView dateTextView;
            readonly AppCompatTextView subjectTextView;
            readonly AppCompatTextView previewTextView;
            readonly LinearLayoutCompat categoriesLayout;
            readonly AppCompatImageView incomingImageView;
            readonly AppCompatImageView outgoingImageView;
            readonly AppCompatImageView draftImageView;
            readonly AppCompatImageView unreadImageView;
            readonly AppCompatImageView attachmentImageView;
            readonly AppCompatImageView commentImageView;
            readonly View selectedOverlay;

            public DocumentPreviewViewHolder(View itemView)
                    : base(itemView)
            {
                recipentTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_document_recipent);
                dateTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_document_date);
                subjectTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_document_subject);
                previewTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_document_preview);
                categoriesLayout = itemView.FindViewById<LinearLayoutCompat>(Resource.Id.list_item_document_categories);
                incomingImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_direction_incoming);
                outgoingImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_direction_outgoing);
                draftImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_direction_draft);
                unreadImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_unread);
                attachmentImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_attachment);
                commentImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_comment);
                selectedOverlay = itemView.FindViewById<View>(Resource.Id.selected_overlay);
            }
        }

        class ExternalDocumentPreviewViewHolder : RecyclerView.ViewHolder
        {

            public string Name
            {
                set
                {
                    nameTextView.Text = value;
                }
            }

            public string Date
            {
                set
                {
                    dateTextView.Text = value;
                }
            }

            public string Preview
            {
                set
                {
                    previewTextView.Text = value;
                }
            }

            public List<Category> Categories
            {
                set
                {
                    categoriesLayout.RemoveAllViews();

                    foreach (var hexColor in value.Select(c => c.HexColor))
                    {
                        var view = new View(ItemView.Context)
                        {
                            LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent, 1f),
                            Background = new ColorDrawable(Color.ParseColor(hexColor))
                        };
                        categoriesLayout.AddView(view);
                    }
                }
            }

            public bool CommentIndicator
            {
                set
                {
                    commentImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
                }
            }

            public bool Selected
            {
                set
                {
                    selectedOverlay.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
                }
            }

            readonly AppCompatTextView nameTextView;
            readonly AppCompatTextView dateTextView;
            readonly AppCompatTextView previewTextView;
            readonly LinearLayoutCompat categoriesLayout;
            readonly AppCompatImageView commentImageView;
            readonly View selectedOverlay;

            public ExternalDocumentPreviewViewHolder(View itemView)
                    : base(itemView)
            {
                nameTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_document_external_name);
                dateTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_document_external_date);
                previewTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_document_external_preview);
                categoriesLayout = itemView.FindViewById<LinearLayoutCompat>(Resource.Id.list_item_document_external_categories);
                commentImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_external_comment);
                selectedOverlay = itemView.FindViewById<View>(Resource.Id.selected_overlay);
            }
        }

        #endregion

        class AutoRefreshWorker
        {

            CancellationTokenSource cts;

            readonly Func<int, Task> work;
            readonly Func<DocumentPreview> firstOrDefaultItem;
            readonly int intervalMs;

            readonly object lockObj = new object();

            public AutoRefreshWorker(Func<int, Task> work, Func<DocumentPreview> firstOrDefaultItem, int intervalMs)
            {
                this.work = work;
                this.firstOrDefaultItem = firstOrDefaultItem;
                this.intervalMs = intervalMs;
            }

            public void Start()
            {
                lock (lockObj)
                {
                    cts?.Cancel();
                    cts = new CancellationTokenSource();
                    Task.Run(async () =>
                    {
                        while (true)
                        {
                            await Task.Delay(intervalMs);
                            if (cts.IsCancellationRequested) break;

                            var first = firstOrDefaultItem();
                            if (first != null)
                            {
                                await work(first.Id);
                            }
                        }
                    });
                }
            }

            public void Stop()
            {
                lock (lockObj)
                {
                    cts?.Cancel();
                }
            }
        }
    }
}

