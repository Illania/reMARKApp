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
using Android.Text;
using Android.Util;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.HubMessages;
using Mark5.Mobile.Common.Service;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;
using Android.Content.Res;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.AppCompat.Widget;
using Google.Android.Material.FloatingActionButton;
using AndroidX.AppCompat.App;
using Google.Android.Material.Snackbar;
using AndroidX.Core.Content;
using FastScrollRecycler;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class DocumentsListFragment : BaseFragment, ActionMode.ICallback, IMenuItemOnActionExpandListener, SearchView.IOnQueryTextListener
    {
        public Folder Folder { get; set; }

        DocumentsListAdapter CurrentAdapter => (DocumentsListAdapter)recyclerView.GetAdapter();

        readonly Handler searchHandler = new Handler();

        protected const string FolderBundleKey = "Folder_5ab3effc-9a60-4b26-805e-72a0c3527b0d";
        const string SelectedDocumentPreviewsKey = "SelectedDocumentPreviews_9d33e0b7-9791-4ee9-82bd-73af5c0b5716";
        const string FirstRowIdKey = "FirstRowId_ab73aa33-930f-4139-94b1-b7828d5f4de7";
        const string LastRowIdKey = "LastRowId_a92f8e84-7274-48e3-9296-3d52a9b3231c"; 
        protected const string HideSearchBundleKey = "HideSearchBundle_4ec1a10c-f9e5-43f8-8e73-c555f7679b43";
        protected const string OnlyShowExternalDocumentsBundleKey = "OnlyShowExternalDocuments_119623bc-74c6-4763-898a-319ea8fc9591";


        const int AutoRefreshIntervalMs = 5 * 1000; // 5 seconds

        bool refreshing;
        bool selectEnabled = true;

        protected List<DocumentPreview> savedSelectedDocumentPreviews;

        IMenu menu;
        CoordinatorLayout coordinatorLayout;
        SwipeRefreshLayout refreshLayout;
        RecyclerView recyclerView;
        SwipeHelperCallback swipeHelperCallback;
        ItemTouchHelper itemTouchHelper;
        DocumentsListAdapter adapter;
        DocumentsListAdapter searchAdapter;
        ActionMode actionMode;
        SearchView searchView;
        FloatingActionButton fab;
        Category presetCategory;

        bool shouldNotifyAdapter;
        bool shouldNotifySearchAdapter;

        int savedFirstRowId = -1;
        int savedLastRowId = -1;

        bool hideSearch;
        bool onlyShowExternalDocuments;

        AutoRefreshWorker autoRefreshWorker;

        Action dismissAction;

        public static (DocumentsListFragment fragment, string tag) NewInstance(Folder folder)
        {
            var args = new Bundle();

            if (folder != null)
                args.PutString(FolderBundleKey, Serializer.Serialize(folder));

            var fragment = new DocumentsListFragment
            {
                Arguments = args
            };

            var tag = $"{nameof(DocumentsListFragment)} [folder.id={folder.Id}, folder.name={folder.Name}]";

            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Arguments.ContainsKey(FolderBundleKey))
                Folder = Serializer.Deserialize<Folder>(Arguments.GetString(FolderBundleKey));

            if (savedInstanceState?.ContainsKey(SelectedDocumentPreviewsKey) == true)
                savedSelectedDocumentPreviews = Serializer.Deserialize<List<DocumentPreview>>(savedInstanceState.GetString(SelectedDocumentPreviewsKey));

            if (savedInstanceState?.ContainsKey(FirstRowIdKey) == true)
                savedFirstRowId = savedInstanceState.GetInt(FirstRowIdKey);

            if (savedInstanceState?.ContainsKey(LastRowIdKey) == true)
                savedLastRowId = savedInstanceState.GetInt(LastRowIdKey);

            if (Arguments.ContainsKey(HideSearchBundleKey))
                hideSearch = Arguments.GetBoolean(HideSearchBundleKey);

            if (Arguments.ContainsKey(OnlyShowExternalDocumentsBundleKey))
                onlyShowExternalDocuments = Arguments.GetBoolean(OnlyShowExternalDocumentsBundleKey);
        }

        #region Fragment overrides

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(DocumentsListFragment)} [folder.id={Folder?.Id}, folder.name={Folder?.Name}]...");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            coordinatorLayout = (CoordinatorLayout)container.Parent.Parent;

            var emptyView = rootView.FindViewById<AppCompatTextView>(Resource.Id.empty_view);
            emptyView.SetText(Resource.String.empty_folder);

            refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.SetColorSchemeResources(Resource.Color.blue, Resource.Color.darkerblue);
            refreshLayout.Refresh += async (sender, e) =>
            {
                CommonConfig.UsageAnalytics.LogEvent(new PullToRefreshEvent(false, module: ModuleType.Documents));

                actionMode?.Finish();
                actionMode = null;

                await RefreshData(forceClear: true);
            };

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));


            adapter = new DocumentsListAdapter(Activity, recyclerView, async (startId) => await RefreshData(startId));
            adapter.FolderId = Folder.Id;
            adapter.ItemClicked += Adapter_ItemClicked;
            adapter.ItemLongClicked += Adapter_ItemLongClicked;
            adapter.RegisterAdapterDataObserver(new LambdaEmptyAdapterObserver(() =>
            {
                if (recyclerView.GetAdapter() != adapter)
                    return;

                emptyView.Visibility = adapter.ItemCount < 1 ? ViewStates.Visible : ViewStates.Gone;
                recyclerView.Visibility = adapter.ItemCount > 0 ? ViewStates.Visible : ViewStates.Gone;
                menu?.FindItem(Resource.Id.action_filter)?.SetEnabled(adapter.ItemCount > 0);
                
            }));
            recyclerView.SetAdapter(adapter);


            swipeHelperCallback = new SwipeHelperCallback(Context, this, adapter, refreshLayout, Folder);
            itemTouchHelper = new ItemTouchHelper(swipeHelperCallback);
            itemTouchHelper.AttachToRecyclerView(recyclerView);

            searchAdapter = new DocumentsListAdapter(Activity);
            searchAdapter.ItemClicked += Adapter_ItemClicked;
            searchAdapter.ItemLongClicked += Adapter_ItemLongClicked;

            fab = ((BaseAppCompatActivity)Activity).Fab;
            fab.SetImageResource(Resource.Drawable.action_new);
            fab.SetOnClickListener(new ActionOnClickListener(ComposeDocument));
            fab.Visibility = ViewStates.Visible;

            HasOptionsMenu = true;

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.documents);
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = Folder?.Name;

            CommonConfig.Logger.Info($"Created {nameof(DocumentsListFragment)} [folder.id={Folder?.Id}, folder.name={Folder?.Name}]");
        }

        public override void OnDestroyView()
        {
            dismissAction?.Invoke();
            base.OnDestroyView();
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

            if (!IsAdded || IsDetached || IsRemoving)
                return;

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

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            if (adapter?.SelectedItems != null || savedSelectedDocumentPreviews != null)
                outState.PutString(SelectedDocumentPreviewsKey, Serializer.Serialize(adapter?.SelectedItems ?? savedSelectedDocumentPreviews));

            if (adapter?.Items?.Any() == true)
            {
                outState.PutInt(FirstRowIdKey, adapter.Items.First().Id + 1); //To comply with the DB query we use
                outState.PutInt(LastRowIdKey, adapter.Items.Last().Id - 1);
            }
            else
            {
                outState.PutInt(FirstRowIdKey, savedFirstRowId);
                outState.PutInt(LastRowIdKey, savedLastRowId);
            }
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            this.menu = menu;

            inflater.Inflate(Resource.Menu.menu_main, menu);

            var filterItem = menu.FindItem(Resource.Id.action_filter);
            filterItem.SetOnActionExpandListener(this);
            searchView = (SearchView)filterItem.ActionView;
            searchView.QueryHint = GetString(Resource.String.filter);
            searchView.SetOnQueryTextListener(this);

            if (!hideSearch)
            {
                var searchItem = menu.Add(Menu.None, 10, Menu.None, Resource.String.search);
                searchItem.SetIcon(Resource.Drawable.action_search_server);
                searchItem.SetShowAsAction(ShowAsAction.Always);
            }

            var bookmarkItem = menu.Add(Menu.None, 11, Menu.None, Resource.String.search);
            bookmarkItem.SetIcon(Resource.Drawable.ic_bookmark);
            bookmarkItem.SetShowAsAction(ShowAsAction.Always); 
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == 10)
            {
                StartActivity(SearchActivity.CreateIntent(Context, ModuleType.Documents));
                return true;
            }
            if(item.ItemId == 11)
            {
                GoToBookmark();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        
        #endregion

        #region Actions

        void ComposeDocument()
        {
            if (!ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines.Any())
            {
                Dialogs.ShowConfirmDialog(Activity, Resource.String.no_lines_error_title, Resource.String.no_lines_error_content);
                return;
            }

            StartActivity(ComposeDocumentActivity.CreateIntent(Context, DocumentCreationModeFlag.New, CopyToNewOption.None));
        }

        public void GoToBookmark()
        {
            var bookmarkForFolderId = PlatformConfig.Preferences.GetBookmarkForFolder(Folder.Id);
            if (bookmarkForFolderId > 0)
            {
                var index = adapter.Items.FindIndex(dp => dp.Id == bookmarkForFolderId);

                if (index >= 0)
                {
                    recyclerView.SmoothScrollToPosition(index);
                }
            }

        }

        async void AddBookmark(DocumentPreview documentPreview)
        {

            CommonConfig.Logger.Info($"Attempting to add bookmark for folder Id= {Folder.Id} [documentPreview.Id={documentPreview.Id}]...");

            try
            {
                var previousBookmarkedDocId = PlatformConfig.Preferences.GetBookmarkForFolder(Folder.Id);
                var itemsToUpdate = new List<int> { documentPreview.Id };
                if (previousBookmarkedDocId > 0)
                    itemsToUpdate.Add(previousBookmarkedDocId);
                PlatformConfig.Preferences.SetBookmarkForFolder(Folder.Id, documentPreview.Id);
                adapter.RefreshItems(itemsToUpdate);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Adding bookmark for folder Id= {Folder.Id} [documentPreview.Id={documentPreview.Id}] failed", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);

            }

        }

        async void RemoveBookmark(DocumentPreview documentPreview)
        {

            CommonConfig.Logger.Info($"Attempting to remove bookmark for folder Id= {Folder.Id} [documentPreview.Id={documentPreview.Id}]...");

            try
            {
                PlatformConfig.Preferences.RemoveBookmarkForFolder(Folder.Id);
                adapter.RefreshItems(new List<int> { documentPreview.Id });
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Removing bookmark for folder Id= {Folder.Id} [documentPreview.Id={documentPreview.Id}] failed", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }

        }

        #endregion

        #region Refreshing

        async Task AutoRefreshData(int endId)
        {
            try
            {
                CommonConfig.Logger.Debug($"Attempting automatic refresh [endId={endId}, !isAdded={!IsAdded}, isDetached={IsDetached}, isRemoving={IsRemoving}, refreshing={refreshing}]...");

                if (!IsAdded || IsDetached || IsRemoving)
                    return;
                if (refreshing)
                    return;

                refreshing = true;

                CommonConfig.Logger.Debug($"Automatic refresh running...");

                var documents = await Managers.DocumentsManager.GetDocumentPreviewsAsync(Folder, endId: endId);

                var incomingCount = documents?.Count(d => d.Direction == DocumentDirection.Incoming && d.TransmitStatus != TransmitStatus.Delayed) ?? 0;
                if (incomingCount > 0)
                {
                    CommonConfig.Logger.Info($"Received {incomingCount} new documents");

                    var snackbar = Snackbar.Make(coordinatorLayout, Resources.GetQuantityString(Resource.Plurals.new_documents_received, documents.Count, documents.Count), Snackbar.LengthShort);
                    snackbar.View.SetBackgroundColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.darkerblue)));
                    snackbar.Show();

                    Activity?.RunOnUiThread(() => {
                        adapter?.PrependItems(documents);
                    });

                    Services.DocumentsDownloadService.Notify();
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

        async Task RefreshData(int startId = -1, int endId = -1, bool forceClear = false)
        {
            try
            {
                CommonConfig.Logger.Info($"Attempting refresh [startId={startId}, endId={endId}, force={forceClear}]...");

                if (refreshing)
                    return;

                refreshing = true;
                refreshLayout.Refreshing = true;

                CommonConfig.Logger.Info($"Refresh running...");

                List<DocumentPreview> documentPreviews;

                if (Restored && savedLastRowId != -1 && savedFirstRowId != -1)
                {
                    documentPreviews = await Managers.DocumentsManager.GetDocumentPreviewsAsync(Folder, savedFirstRowId, savedLastRowId, SourceType.Local);
                    savedLastRowId = savedFirstRowId = -1;
                }
                else
                {
                    documentPreviews = await Managers.DocumentsManager.GetDocumentPreviewsAsync(Folder, startId, endId);
                }

                if (onlyShowExternalDocuments)
                    documentPreviews = documentPreviews.FindAll((DocumentPreview dp) => dp.Direction == DocumentDirection.External);

                adapter.EnableLoadMore = documentPreviews.Count >= PlatformConfig.Preferences.DocumentsToDownload;
                CommonConfig.Logger.Info($"Enable load more documents set to {adapter.EnableLoadMore}");

                if (forceClear)
                    adapter.Clear();

                if (PlatformConfig.Preferences.SortByDate)
                    adapter.InsertItems(documentPreviews);
                else
                    adapter.AppendItems(documentPreviews);

                if (savedSelectedDocumentPreviews?.Count > 0)
                {
                    actionMode?.Finish();
                    actionMode = Activity.StartActionMode(this);

                    adapter.SetSelected(savedSelectedDocumentPreviews, true);
                    actionMode.Title = adapter.SelectedItemCount.ToString();
                    actionMode.Invalidate();

                    savedSelectedDocumentPreviews = null;
                }

                Services.DocumentsDownloadService.Notify();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Downloading documents failed [folder.name={Folder?.Name}, folder.id={Folder?.Id}, startId={startId}, endId={endId}, force={forceClear}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);

                Activity?.OnBackPressed();
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

        protected virtual async void Adapter_ItemClicked(object sender, DocumentPreview documentPreview)
        {
            if (actionMode == null)
            {
                if (documentPreview.Direction == DocumentDirection.Draft)
                {
                    if (!ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines.Any())
                    {
                        Dialogs.ShowConfirmDialog(Activity, Resource.String.no_lines_error_title, Resource.String.no_lines_error_content);
                        return;
                    }

                    StartActivity(ComposeDocumentActivity.CreateIntent(Context,
                                                                       DocumentCreationModeFlag.Edit,
                                                                       CopyToNewOption.None,
                                                                       previousDocumentDirection: documentPreview.Direction,
                                                                       previousDocumentFolderId: Folder.Id,
                                                                       previousDocumentId: documentPreview.Id));
                }
                else
                {
                    StartActivity(SwipeDocumentActivity.CreateIntent(Context, Folder, documentPreview));
                }
            }
            else
            {
                CurrentAdapter.SetSelected(documentPreview, !CurrentAdapter.IsSelected(documentPreview));

                if (CurrentAdapter.SelectedItemCount < 1)
                {
                    actionMode.Finish();
                }
                else
                {
                    actionMode.Title = CurrentAdapter.SelectedItemCount.ToString();
                    actionMode.Invalidate();
                }
            }
        }

        protected void Adapter_ItemLongClicked(object sender, DocumentPreview documentPreview)
        {
            if (actionMode == null)
                actionMode = Activity.StartActionMode(this);

            Adapter_ItemClicked(sender, documentPreview);
        }

        #endregion

        #region Action mode

        public void ShowCategories(DocumentPreview documentPreview)
        {
            StartActivity(CategoriesListActivity.CreateIntent(Context, documentPreview));
        }

        bool ActionMode.ICallback.OnCreateActionMode(ActionMode mode, IMenu menu)
        {
            return true;
        }

        bool ActionMode.ICallback.OnPrepareActionMode(ActionMode mode, IMenu menu)
        {
            swipeHelperCallback.Enabled = false;
            fab?.Hide();

            menu.Clear();

            var selectAllItem = menu.Add(Menu.None, MenuItemActions.SelectDeselectAll, MenuItemActions.SelectDeselectAll, "");
            if (selectEnabled)
            {
                selectAllItem.SetTitle(Resource.String.select_all);
                selectAllItem.SetIcon(Resource.Drawable.action_select_all);
            }
            else
            {
                selectAllItem.SetTitle(Resource.String.deselect_all);
                selectAllItem.SetIcon(Resource.Drawable.action_deselect_all);
            }

            var selectedDocuments = CurrentAdapter.SelectedItems;
            if (ServerConfig.SystemSettings?.SystemInfo?.DelaySendAvailable == true && selectedDocuments.Any(dp => dp.TransmitStatus == TransmitStatus.Delayed))
            {
                menu.Add(MenuItemGroup.Actions, MenuItemActions.SendNow, MenuItemActions.SendNow, Resource.String.send_now);
                menu.Add(MenuItemGroup.Actions, MenuItemActions.CancelSend, MenuItemActions.CancelSend, Resource.String.cancel_send);
            }

            if (selectedDocuments.Any(dp => !dp.IsReadByCurrent) || !selectedDocuments.Any())
                menu.Add(MenuItemGroup.Actions, MenuItemActions.MarkAsRead, MenuItemActions.MarkAsRead, Resource.String.mark_as_read);

            if (selectedDocuments.Any(dp => dp.IsReadByCurrent))
                menu.Add(MenuItemGroup.Actions, MenuItemActions.MarkAsUnread, MenuItemActions.MarkAsUnread, Resource.String.marks_as_unread);

            if (ServerConfig.SystemSettings.DocumentsModuleInfo.WorktrayEnabled ?? true)
                menu.Add(MenuItemGroup.Actions, MenuItemActions.CopyToWorktray, MenuItemActions.CopyToWorktray, Resource.String.copy_to_worktray);

            menu.Add(MenuItemGroup.Actions, MenuItemActions.CopyToFolder, MenuItemActions.CopyToFolder, Resource.String.copy_to_folder);

            if (PlatformConfig.Preferences.EnableMoveToFolder &&
                (Folder.InternalType == FolderInternalType.FilterView
                || Folder.InternalType == FolderInternalType.Static
                || Folder.InternalType == FolderInternalType.Worktray))
            {
                menu.Add(MenuItemGroup.Actions, MenuItemActions.MoveToFolder, MenuItemActions.MoveToFolder, Resource.String.move_to_folder);
            }

            menu.Add(MenuItemGroup.Actions, MenuItemActions.SetPriority, MenuItemActions.SetPriority, Resource.String.set_priority);

            if (CurrentAdapter.SelectedItemCount == 1)
                menu.Add(MenuItemGroup.Actions, MenuItemActions.Categories, MenuItemActions.Categories, Resource.String.categories);

            if (Folder.InternalType == FolderInternalType.FilterView || Folder.InternalType == FolderInternalType.Static || Folder.InternalType == FolderInternalType.Worktray)
                menu.Add(MenuItemGroup.Actions, MenuItemActions.DeleteFromFolder, MenuItemActions.DeleteFromFolder, Resource.String.delete_from_folder);

            AddDeleteDocumentMenuItem(selectedDocuments);

            if (selectedDocuments.Count == 1)
                menu.Add(MenuItemGroup.Actions, MenuItemActions.AddRemoveBookmark, MenuItemActions.AddRemoveBookmark,
                 PlatformConfig.Preferences.HasBookmarkForFolder(Folder.Id, selectedDocuments.First().Id)
                 ? Resource.String.remove_bookmark
                 : Resource.String.add_bookmark);

            if (CurrentAdapter.SelectedItemCount == 1)
                menu.Add(MenuItemGroup.Actions, MenuItemActions.SetPresetCategory, MenuItemActions.SetPresetCategory, Resource.String.set_preset_category);

            menu.SetGroupEnabled(MenuItemGroup.Actions, selectedDocuments.Any());

            return true;
        }

        private void AddDeleteDocumentMenuItem(List<DocumentPreview> documents)
        {
            if ((!ServerConfig.SystemSettings.DocumentsModuleInfo.Permissions.DeleteAllowed
                && documents.Any(dp => dp.Direction != DocumentDirection.Draft))
                || documents.Count == 0)
            {
                return;
            }

            var linesAllowedToDelete =
                ServerConfig.SystemSettings.DocumentsModuleInfo.Permissions.DeleteDocumentsAllowedLines;
            if (!linesAllowedToDelete.Any())
                return;
            
            if (documents.Count == 1)
            {
                var document = documents.FirstOrDefault();
                if (document == null) 
                    return;
            
                var linesGuids = document.Lines.Select(l => l.Guid);
                var intersection = linesAllowedToDelete.Intersect(linesGuids);
                if (!intersection.Any()) 
                    return;
            }
            
            menu.Add(MenuItemGroup.Actions, MenuItemActions.Delete,
                MenuItemActions.Delete, Resource.String.delete);
        }

        bool ActionMode.ICallback.OnActionItemClicked(ActionMode mode, IMenuItem item)
        {
            if (item.ItemId == MenuItemActions.SendNow)
            {
                ForceSend(CurrentAdapter.SelectedItems);
                return true;
            }

            if (item.ItemId == MenuItemActions.CancelSend)
            {
                CancelSend(CurrentAdapter.SelectedItems);
                return true;
            }

            if (item.ItemId == MenuItemActions.MarkAsRead)
            {
                MarkAsRead(CurrentAdapter.SelectedItems);
                return true;
            }

            if (item.ItemId == MenuItemActions.MarkAsUnread)
            {
                MarkAsUnread(CurrentAdapter.SelectedItems);
                return true;
            }

            if (item.ItemId == MenuItemActions.CopyToWorktray)
            {
                CopyToWorktrayAction(CurrentAdapter.SelectedItems);
                return true;
            }

            if (item.ItemId == MenuItemActions.CopyToFolder)
            {
                StartActivity(CopyMoveToFolderListActivity.CreateIntent(Context, CopyMoveToFolderListActivity.ModeType.Copy, ModuleType.Documents,
                                                                        CurrentAdapter.SelectedItems.Select(sp => sp).Cast<IBusinessEntity>().ToList()));
                actionMode?.Finish();
                return true;
            }

            if (item.ItemId == MenuItemActions.MoveToFolder)
            {
                StartActivity(CopyMoveToFolderListActivity.CreateIntent(Context, CopyMoveToFolderListActivity.ModeType.Move, ModuleType.Documents,
                                                                        CurrentAdapter.SelectedItems.Select(sp => sp).Cast<IBusinessEntity>().ToList(), Folder));
                actionMode?.Finish();
                return true;
            }

            if (item.ItemId == MenuItemActions.SetPriority)
            {
                SetPriority(CurrentAdapter.SelectedItems);
                return true;
            }

            if (item.ItemId == MenuItemActions.SelectDeselectAll)
            {
                SelectDeselectAll();
                return true;
            }

            if (item.ItemId == MenuItemActions.Categories)
            {
                ShowCategories(CurrentAdapter.SelectedItems.First());

                actionMode?.Finish();
                return true;
            }

            if (item.ItemId == MenuItemActions.DeleteFromFolder)
            {
                DeleteFromFolderAction(CurrentAdapter.SelectedItems);
                return true;
            }

            if (item.ItemId == MenuItemActions.Delete)
            {
                DeleteAction(CurrentAdapter.SelectedItems);
                return true;
            }

            if(item.ItemId == MenuItemActions.AddRemoveBookmark)
            {
                if (!PlatformConfig.Preferences.HasBookmarkForFolder(Folder.Id, CurrentAdapter.SelectedItems.First().Id))
                    AddBookmark(CurrentAdapter.SelectedItems.First());
                else
                    RemoveBookmark(CurrentAdapter.SelectedItems.First());
                return true;
            }


            if (item.ItemId == MenuItemActions.SetPresetCategory)
            {
                AssignPresetCategory(CurrentAdapter.SelectedItems.First());
                return true;
            }

            return base.OnOptionsItemSelected(item);

        }

        void ActionMode.ICallback.OnDestroyActionMode(ActionMode mode)
        {
            swipeHelperCallback.Enabled = true;
            fab?.Show();

            CurrentAdapter.ClearSelections();
            actionMode = null;
            selectEnabled = true;
        }

        public async Task CopyToOwnWorktray(List<DocumentPreview> documentPreviews)
        {
            CommonConfig.Logger.Info($"Attempting copy to worktray [businessEntities.Count={documentPreviews.Count}]...");

            dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.copying_to_worktray, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.CopyToWorktray(documentPreviews.OfType<IBusinessEntity>().ToList());

                dismissAction();
                actionMode?.Finish();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Copying to worktray failed [businessEntities.Count={CurrentAdapter.SelectedItemCount}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        async void ForceSend(List<DocumentPreview> items)
        {
            var delayedItems = items.Where(i => i.TransmitStatus == TransmitStatus.Delayed).ToList();

            CommonConfig.Logger.Info($"Attempting to force send delayed items [businessEntities.Count={delayedItems.Count}]...");

            dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.send_now, Resource.String.please_wait);

            try
            {
                CommonConfig.UsageAnalytics.LogEvent(new ForceSendEvent());

                await Managers.DocumentsManager.ForceSendDocument(delayedItems);
                adapter.RefreshItems(delayedItems);
                searchAdapter.RefreshItems(delayedItems);

                CommonConfig.Logger.Info($"Documents with IDs:{string.Join(",",items.Select(i=>i.Id).ToList()).TrimEnd(',')} forced sent.");

                foreach(var item in delayedItems)
                    CommonConfig.MessengerHub.Publish(new DocumentUploadStatusChangedMessage(this, DocumentUploadStatusChangedMessage.Status.DocumentSent,
                        Guid.Empty, false));


                dismissAction();
                actionMode?.Finish();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Force send failed [businessEntities.Count={delayedItems.Count}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        async void CancelSend(List<DocumentPreview> items)
        {
            var delayedItems = items.Where(i => i.TransmitStatus == TransmitStatus.Delayed).ToList();

            CommonConfig.Logger.Info($"Attempting to cancel send delayed items [businessEntities.Count={delayedItems.Count}]...");

            dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.cancel_send, Resource.String.please_wait);

            try
            {
                CommonConfig.UsageAnalytics.LogEvent(new CancelSendEvent());

                await Managers.DocumentsManager.CancelSendDocument(delayedItems);
                adapter.RefreshItems(delayedItems);
                searchAdapter.RefreshItems(delayedItems);

                foreach (var item in delayedItems)
                    CommonConfig.MessengerHub.Publish(new DocumentUploadStatusChangedMessage(this, DocumentUploadStatusChangedMessage.Status.DocumentSendCancelled,
                        Guid.Empty, false));

                dismissAction();
                actionMode?.Finish();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Cancel send failed [businessEntities.Count={delayedItems.Count}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        async void MarkAsRead(List<DocumentPreview> items)
        {
            CommonConfig.Logger.Info($"Attempting to mark as read [businessEntities.Count={items.Count}]...");

            dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.marking_as_read, Resource.String.please_wait);

            try
            {
                CommonConfig.UsageAnalytics.LogEvent(new SetReadStatusEvent(items.Count));

                await Managers.DocumentsManager.SetDocumentsReadStatusAsync(items, true);

                if (PlatformConfig.Preferences.SyncUserActivities)
                    await Managers.DocumentsManager.ExecuteUserActivity(UserActivityType.Read, items);

                adapter.RefreshItems(items);
                searchAdapter.RefreshItems(items);

                dismissAction();
                actionMode?.Finish();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Marking as read failed [businessEntities.Count={items.Count}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        async void MarkAsUnread(List<DocumentPreview> items)
        {
            CommonConfig.Logger.Info($"Attempting to mark as unread [businessEntities.Count={items.Count}]...");

            dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.marking_as_unread, Resource.String.please_wait);

            try
            {
                CommonConfig.UsageAnalytics.LogEvent(new SetReadStatusEvent(items.Count));
                await Managers.DocumentsManager.SetDocumentsReadStatusAsync(items, false);
                adapter.RefreshItems(items);
                searchAdapter.RefreshItems(items);

                dismissAction();
                actionMode?.Finish();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Marking as unread failed [businessEntities.Count={items.Count}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        async void CopyToWorktrayAction(List<DocumentPreview> documentPreviews)
        {
            var option = await Dialogs.ShowListDialog(Context, Resource.String.copy_to_worktray, Resource.Array.copy_to_worktray_options, true);

            if (option == 0)
                await CopyToOwnWorktray(documentPreviews);

            if (option == 1)
            {
                StartActivity(CopyToUserWorktrayActivity.CreateIntent(Context, documentPreviews.Cast<IBusinessEntity>().ToList()));
                actionMode?.Finish();
            }
        }

        async void CopyToOwnWorktray(DocumentPreview documentPreview)
        {
            await CopyToOwnWorktray(new List<DocumentPreview>
            {
                documentPreview
            });
        }

        async void SetPriority(List<DocumentPreview> items)
        {
            var possiblePriorities = new List<Priority>
            {
                Priority.Urgent,
                Priority.Normal,
                Priority.Low
            };

            var selectedPriority = items.All(dp => dp.Priority == items[0].Priority) ? items[0].Priority : Priority.None;

            if (!possiblePriorities.Contains(selectedPriority))
                selectedPriority = Priority.Normal;

            var priority = await Dialogs.ShowSingleSelectDialogAsync(Context, Resource.String.set_priority, possiblePriorities, selectedPriority);
            if (priority == default(Priority) || priority == selectedPriority)
                return;

            CommonConfig.Logger.Info($"Attempting to set priority [businessEntities.Count={items.Count}]...");

            dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.setting_priority, Resource.String.please_wait);

            try
            {
                await Managers.DocumentsManager.SetDocumentsPriorityAsync(items, priority);

                dismissAction();
                actionMode?.Finish();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Setting priority failed [businessEntities.Count={items.Count}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }

        }

        async void AssignPresetCategory(DocumentPreview documentPreview)
        {
            if (presetCategory == null)
            {
                var categories = await Managers.DocumentsManager.GetAllCategoriesAsync();
                presetCategory = categories.FirstOrDefault(
                    c => c.Id == PlatformConfig.Preferences.PresetCategoryId);
                if (presetCategory == null)
                    return;
            }
            
            var oldCategories = documentPreview.Categories;
            var newCategories = oldCategories.Union(new List<Category> { presetCategory }).ToList();

            CommonConfig.Logger.Info($"Attempting to assign preset category [documentPreview.Id={documentPreview.Id}]...");
            dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, 
                Resource.String.set_preset_category, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.SetCategoriesAsync(documentPreview, newCategories);
                dismissAction();
                actionMode?.Finish();
            }
            catch (Exception ex)
            {
                dismissAction();
                CommonConfig.Logger.Error($"Assigning preset category for " +
                                          $"[documentPreview.Id={documentPreview.Id}] failed", ex);
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        void SelectDeselectAll()
        {
            if (actionMode == null) 
                return;
            
            CurrentAdapter?.SetSelected(CurrentAdapter.Items, selectEnabled);
            actionMode.Title = CurrentAdapter?.SelectedItemCount.ToString();
            selectEnabled = !selectEnabled;
            actionMode.Invalidate();
        }

        async void DeleteFromFolderAction(List<DocumentPreview> items, bool fromSwipe = false)
        {
            if (PlatformConfig.Preferences.ConfirmationRemoveSwipe && fromSwipe)
            {
                var yesNo = await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.delete_from_folder, Resource.String.delete_from_folder_are_you_sure);
                if (!yesNo)
                    return;
            }

            CommonConfig.Logger.Info($"Attempting to delete from folder [businessEntities.Count={items.Count}]...");

            dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.deleting_from_folder, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.RemoveFromFolder(items.OfType<IBusinessEntity>().ToList(), Folder);

                dismissAction();
                actionMode?.Finish();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Deleting from folder failed [businessEntities.Count={items.Count}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        async void DeleteAction(List<DocumentPreview> items)
        {
            var yesNo = await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.delete, Resource.String.delete_are_you_sure);
            if (!yesNo)
                return;

            CommonConfig.Logger.Info($"Attempting to delete [businessEntities.Count={items.Count}]...");

            dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.deleting, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.Delete(items.OfType<IBusinessEntity>().ToList());

                dismissAction();
                actionMode?.Finish();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Deleting failed [businessEntities.Count={items.Count}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        static class MenuItemActions
        {
            public const int SelectDeselectAll = 05;
            public const int MarkAsRead = 10;
            public const int MarkAsUnread = 11;
            public const int CopyToWorktray = 20;
            public const int CopyToFolder = 30;
            public const int MoveToFolder = 31;
            public const int SetPriority = 40;
            public const int Categories = 50;
            public const int DeleteFromFolder = 60;
            public const int Delete = 61;
            public const int SendNow = 70;
            public const int CancelSend = 71;
            public const int AddRemoveBookmark = 72;
            public const int SetPresetCategory = 73;
        }

        static class MenuItemGroup
        {
            public const int Actions = 1;
        }

        #endregion

        #region Filtering

        bool IMenuItemOnActionExpandListener.OnMenuItemActionExpand(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_filter)
            {
                CommonConfig.UsageAnalytics.LogEvent(new FilterEvent(false, module: ModuleType.Documents));

                menu?.FindItem(10)?.SetVisible(false);

                refreshLayout.Enabled = false;
                adapter.ClearSelections();
                recyclerView.SwapAdapter(searchAdapter, true);
                (this as SearchView.IOnQueryTextListener).OnQueryTextChange(string.Empty);
                return true;
            }

            return false;
        }

        bool IMenuItemOnActionExpandListener.OnMenuItemActionCollapse(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_filter)
            {
                menu?.FindItem(10)?.SetVisible(true);

                searchHandler.RemoveCallbacksAndMessages(null);
                searchAdapter.Clear();
                searchAdapter.ClearSelections();
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
                        searchAdapter.ReplaceItems(adapter.Items);
                    else
                        searchAdapter.ReplaceItems(adapter.Items.Where(dp => MatchesQuery(dp, newText)).ToList());
                },
                500);
            return false;
        }

        bool SearchView.IOnQueryTextListener.OnQueryTextSubmit(string query)
        {
            return false;
        }

        static bool MatchesQuery(DocumentPreview dp, string query)
        {
#if DEBUG
            if (dp.Id.ToString() == query)
                return true;
#endif

            if (dp.ReferenceNumber?.ContainsCaseInsensitive(query) ?? false)
                return true;

            if (dp.Subject?.ContainsCaseInsensitive(query) ?? false)
                return true;

            if (dp.Preview?.ContainsCaseInsensitive(query) ?? false)
                return true;

            if (dp.Addresses.Any(da => da.Name?.ContainsCaseInsensitive(query) ?? false))
                return true;

            if (dp.Addresses.Any(da => da.Address?.ContainsCaseInsensitive(query) ?? false))
                return true;

            if (dp.Categories.Any(da => da.Name?.ContainsCaseInsensitive(query) ?? false))
                return true;

            if (dp.Creator?.ContainsCaseInsensitive(query) ?? false)
                return true;

            return false;
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

        public void UpdatePriority(DocumentPreviewPriorityChangedMessage m)
        {
            var position = adapter.GetPosition(m.DocumentPreviewId);
            if (position >= 0)
            {
                shouldNotifyAdapter = true;
                var dp = adapter.Items[position];
                dp.Priority = m.Priority;
            }

            position = searchAdapter.GetPosition(m.DocumentPreviewId);
            if (position >= 0)
            {
                shouldNotifySearchAdapter = true;
                var dp = searchAdapter.Items[position];
                dp.Priority = m.Priority;
            }
        }

        public void UpdateCategories(EntityCategoriesChangedMessage m)
        {
            var position = adapter.GetPosition(m.EntityId);
            if (position >= 0)
            {
                shouldNotifyAdapter = true;
                var dp = adapter.Items[position];
                dp.Categories.Clear();
                dp.Categories.AddRange(m.Categories);
                adapter.NotifyDataSetChanged();
            }

            position = searchAdapter.GetPosition(m.EntityId);
            if (position >= 0)
            {
                shouldNotifySearchAdapter = true;
                var dp = searchAdapter.Items[position];
                dp.Categories.Clear();
                dp.Categories.AddRange(m.Categories);
                searchAdapter.NotifyDataSetChanged();
            }
        }

        public void UpdateCommentsCount(EntityPreviewCommentCountChangedMessage m)
        {
            var position = adapter.GetPosition(m.EntityId);
            if (position >= 0)
            {
                shouldNotifyAdapter = true;
                var dp = adapter.Items[position];
                dp.CommentsCount = m.CommentsCount;
            }

            position = searchAdapter.GetPosition(m.EntityId);
            if (position >= 0)
            {
                shouldNotifySearchAdapter = true;
                var dp = searchAdapter.Items[position];
                dp.CommentsCount = m.CommentsCount;
            }
        }

        public void UpdateMovedFromFolderEntities(EntityMovedFromFolderMessage m)
        {
            foreach (var entityId in m.EntitiesId)
            {
                var position = adapter.GetPosition(entityId);
                if (position >= 0)
                {
                    shouldNotifyAdapter = true;
                    adapter.RemoveItemAtPosition(position);
                }

                position = searchAdapter.GetPosition(entityId);
                if (position >= 0)
                {
                    shouldNotifySearchAdapter = true;
                    searchAdapter.RemoveItemAtPosition(position);
                }
            }
        }

        public void UpdateRemovedFromFolderEntities(EntityRemovedFromFolderMessage m)
        {
            foreach (var entityId in m.EntitiesId)
            {
                var position = adapter.GetPosition(entityId);
                if (position >= 0)
                {
                    shouldNotifyAdapter = true;
                    adapter.RemoveItemAtPosition(position);
                }

                position = searchAdapter.GetPosition(entityId);
                if (position >= 0)
                {
                    shouldNotifySearchAdapter = true;
                    searchAdapter.RemoveItemAtPosition(position);
                }
            }
        }

        public void UpdateRemovedEntities(EntityRemovedMessage m)
        {
            Activity?.RunOnUiThread(() =>
            {
                foreach (var entityId in m.EntitiesId)
                {
                    var position = adapter.GetPosition(entityId);
                    if (position >= 0)
                    {
                        shouldNotifyAdapter = true;
                        adapter.RemoveItemAtPosition(position);
                    }

                    position = searchAdapter.GetPosition(entityId);
                    if (position >= 0)
                    {
                        shouldNotifySearchAdapter = true;
                        searchAdapter.RemoveItemAtPosition(position); 
                    }
                }
            });
        }
           

        #endregion

        #region RecyclerView Adapter/ViewHolder

        protected class DocumentsListAdapter : RecyclerView.Adapter, ISectionedAdapter
        {
            public override int ItemCount => Items.Count;

            public List<DocumentPreview> Items { get; } = new List<DocumentPreview>(1000);

            public List<DocumentPreview> SelectedItems => selectedDocumentsInView.Values.ToList();

            readonly bool useMessageListAppearance = PlatformConfig.Preferences.UseMessageListAppearance;

            public int SelectedItemCount => selectedDocumentsInView.Count;

            public bool EnableLoadMore { get; set; }
            public int FolderId { get; set; }

            public event EventHandler<DocumentPreview> ItemClicked = delegate { };
            public event EventHandler<DocumentPreview> ItemLongClicked = delegate { };

            readonly Dictionary<int, DocumentPreview> selectedDocumentsInView = new Dictionary<int, DocumentPreview>();
            readonly Context context;
            readonly RecyclerView recyclerView;
            readonly Action<int> loadMoreAction;
            readonly bool unreadIndicatorMe = PlatformConfig.Preferences.UnreadIndicatorMe;
            readonly bool compactList = PlatformConfig.Preferences.CompactDocumentsList;
            readonly bool showCreatorOutgoing = PlatformConfig.Preferences.ShowCreatorOutgoing;

            int? swipedPosition;
            int swipedDirection;

            public DocumentsListAdapter(Context context)
            {
                this.context = context;
            }

            public DocumentsListAdapter(Context context, RecyclerView recyclerView, Action<int> loadMoreAction)
            {
                this.recyclerView = recyclerView;
                this.context = context;
                this.loadMoreAction = loadMoreAction;
            }

            public override int GetItemViewType(int position)
            {
                return Items[position].Direction == DocumentDirection.External ? ViewType.ExternalDocumentView : ViewType.DocumentView;
            }


            string ISectionedAdapter.GetSectionName(int position)
            {
                var vh = recyclerView?.FindViewHolderForAdapterPosition(position);

                if (vh != null)
                {
                    var dpvh = vh as DocumentPreviewViewHolder;
                    if (dpvh != null)
                        return dpvh.BubbleDate;

                    var edpvh = vh as ExternalDocumentPreviewViewHolder;
                    if (edpvh != null)
                        return edpvh.BubbleDate;
                }

                return string.Empty;
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var dp = Items[position];

                if (holder is DocumentPreviewViewHolder)
                {
                    var dpvh = holder as DocumentPreviewViewHolder;

                    dpvh.SwipedDirection = position == swipedPosition ? swipedDirection : 0;

                    dpvh.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, dp)));
                    dpvh.ItemView.SetOnLongClickListener(new ActionOnLongClickListener(() => ItemLongClicked(this, dp)));

                    if (dp.Direction == DocumentDirection.Incoming)
                    {
                        var address = dp.Addresses.FirstOrDefault(da => da.AddressType == DocumentAddressType.From);
                        dpvh.Recipent = address == null ? string.Empty : string.IsNullOrWhiteSpace(address.Name) ? address.Address : address.Name;
                    }
                    else
                    {
                        if (showCreatorOutgoing)
                        {
                            dpvh.Recipent = dp.Creator;
                        }
                        else
                        {
                            var address = dp.Addresses.Where(da => da.AddressType == DocumentAddressType.To || da.AddressType == DocumentAddressType.Cc || da.AddressType == DocumentAddressType.Bcc).OrderBy(da => da.AddressType).FirstOrDefault();
                            dpvh.Recipent = address == null ? string.Empty : string.IsNullOrWhiteSpace(address.Name) ? address.Address : address.Name;
                        }
                    }

                    var isFailed = dp.TransmitStatus == TransmitStatus.Fail || dp.TransmitStatus == TransmitStatus.FailedBounced;
                    var isPartiallySent = dp.TransmitStatus == TransmitStatus.PartialSent;
                    var isDelayed = dp.TransmitStatus == TransmitStatus.Delayed;
                    var isCancelled = dp.TransmitStatus == TransmitStatus.InCancel || dp.TransmitStatus == TransmitStatus.Fail;

                    dpvh.Subject = string.IsNullOrWhiteSpace(dp.Subject) ? context.GetString(Resource.String.no_subject) : dp.Subject;
                    var d = dp.DateReceivedTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime().ConvertDateTimeToTimestampMilliseconds();
                    dpvh.Date = PlatformConfig.Preferences.ShowTimeOlderEmails ? d.FormatUserTimestampAsCompactMediumDateTimeString(context) : d.FormatUserTimestampAsCompactShortDateTimeString(context);
                    dpvh.BubbleDate = d.FormatUserTimestampAsCompactLongDateTimeString(context);
                    dpvh.Preview = string.IsNullOrWhiteSpace(dp.Preview) ? context.GetString(Resource.String.no_content) : Regex.Replace(dp.Preview, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
                    dpvh.Categories = dp.Categories;
                    dpvh.FailIndicator = isFailed;
                    dpvh.PartiallySentIndicator = isPartiallySent;
                    dpvh.IncomingIndicator = dp.Direction == DocumentDirection.Incoming && (!isPartiallySent) && (!isFailed);
                    dpvh.OutgoingIndicator = dp.Direction == DocumentDirection.Outgoing && (!isPartiallySent) && (!isFailed) && (!isDelayed);
                    dpvh.DelayedIndicator = dp.TransmitStatus == TransmitStatus.Delayed && (!isPartiallySent) && (!isFailed);
                    dpvh.CancelledIndicator = isCancelled;
                    dpvh.DraftIndicator = dp.Direction == DocumentDirection.Draft;
                    dpvh.UnreadIndicator = unreadIndicatorMe ? !dp.IsReadByCurrent : !dp.IsReadByAnyone;
                    dpvh.AttachmentIndicator = dp.AttachmentsCount > 0;
                    dpvh.CommentIndicator = dp.CommentsCount > 0;
                    dpvh.PriorityHighIndicator = dp.Priority == Priority.Urgent; 
                    dpvh.PriorityLowIndicator = dp.Priority == Priority.Low;

                    if (compactList)
                    {
                        dpvh.Preview = null;
                        dpvh.AttachmentIndicator = false;
                        dpvh.CommentIndicator = false;
                    }

                    dpvh.Selected = selectedDocumentsInView.ContainsKey(dp.Id);
                    if (useMessageListAppearance)
                        InitializeMessageListAppearance(dp, dpvh);
                    InitializeBookmarkAppearance(dp, FolderId, dpvh);

                }
                else if (holder is ExternalDocumentPreviewViewHolder)
                {
                    var edpvh = holder as ExternalDocumentPreviewViewHolder;

                    edpvh.SwipedDirection = position == swipedPosition ? swipedDirection : 0;
                    edpvh.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, dp)));
                    edpvh.ItemView.SetOnLongClickListener(new ActionOnLongClickListener(() => ItemLongClicked(this, dp)));

                    var d = dp.DateReceivedTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime().ConvertDateTimeToTimestampMilliseconds();
                    edpvh.Date = PlatformConfig.Preferences.ShowTimeOlderEmails ? d.FormatUserTimestampAsCompactMediumDateTimeString(context) : d.FormatUserTimestampAsCompactShortDateTimeString(context);
                    edpvh.BubbleDate = d.FormatUserTimestampAsCompactLongDateTimeString(context);
                    edpvh.Name = string.IsNullOrWhiteSpace(dp.Subject) ? context.GetString(Resource.String.no_subject) : dp.Subject;
                    edpvh.Preview = string.IsNullOrWhiteSpace(dp.Preview) ? context.GetString(Resource.String.no_content) : Regex.Replace(dp.Preview, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
                    edpvh.Categories = dp.Categories;
                    edpvh.CommentIndicator = dp.CommentsCount > 0;
                    edpvh.PriorityHighIndicator = dp.Priority == Priority.Urgent;
                    edpvh.PriorityLowIndicator = dp.Priority == Priority.Low;

                    edpvh.Selected = selectedDocumentsInView.ContainsKey(dp.Id);

                    if (useMessageListAppearance)
                        InitializeMessageListAppearance(dp, edpvh);

                    InitializeBookmarkAppearance(dp, FolderId, edpvh);
                }

                if (recyclerView != null && loadMoreAction != null && position == ItemCount - 1 && EnableLoadMore)
                {
                    CommonConfig.UsageAnalytics.LogEvent(new GetMoreDocumentsEvent());

                    loadMoreAction(dp.Id);
                }
            }

            void InitializeMessageListAppearance(DocumentPreview dp, DocumentPreviewViewHolder cell)
            {
                //apply default appearance
                var defaultAppearance = ServerConfig.SystemSettings.DocumentsModuleInfo.DefaultAppearance;
                var daReadColor = new Color(defaultAppearance.FontColor);
                var daUnreadColor = new Color(defaultAppearance.UnreadFontColor);

                if (defaultAppearance.FontColorEnable)
                    cell.SetTextColor(daReadColor);
       
                if (defaultAppearance.UnreadFontColorEnable)
                    cell.SetTextColor(dp.IsReadByCurrent ? daReadColor : daUnreadColor);

                //if row appearance depends from line use line appearance           
                var lineAppearance = ServerConfig.SystemSettings.DocumentsModuleInfo.LineAppearances.FirstOrDefault(la => dp.Lines.Any(l => l.Guid == la.OriginatorGid));
                if (lineAppearance != null && lineAppearance.Enable)
                {
                    var laReadColor = new Color(lineAppearance.FontColor); 
                    var laUnreadColor = new Color(lineAppearance.UnreadFontColor);
                    var laBgColor = new Color(lineAppearance.BackgroundColor);
                    cell.ItemView.SetBackgroundColor(laBgColor);
                    if (defaultAppearance.FontColorEnable)
                        cell.SetTextColor(laReadColor);
                    if (defaultAppearance.UnreadFontColorEnable)
                        cell.SetTextColor(dp.IsReadByCurrent ? laReadColor : laUnreadColor);

                }

                //if row appearance depends from user use user appearance           
                var userAppearance = ServerConfig.SystemSettings.DocumentsModuleInfo.UserAppearances.FirstOrDefault(la => dp.CreatorGuid == la.OriginatorGid);
                if (userAppearance != null && userAppearance.Enable)
                {
                    var uaReadColor = new Color(userAppearance.FontColor);
                    var uaUnreadColor = new Color(userAppearance.UnreadFontColor);
                    var uaBgColor = new Color(userAppearance.BackgroundColor);
                    cell.ItemView.SetBackgroundColor(uaBgColor);
                    if (defaultAppearance.FontColorEnable)
                        cell.SetTextColor(uaReadColor);
                    if (defaultAppearance.UnreadFontColorEnable)
                        cell.SetTextColor(dp.IsReadByCurrent ? uaReadColor : uaUnreadColor);
                }
            }

            void InitializeMessageListAppearance(DocumentPreview dp, ExternalDocumentPreviewViewHolder cell)
            {
                //apply default appearance
                var defaultAppearance = ServerConfig.SystemSettings.DocumentsModuleInfo.DefaultAppearance;
                var daReadColor = new Color(defaultAppearance.FontColor);
                var daUnreadColor = new Color(defaultAppearance.UnreadFontColor);

                if (defaultAppearance.FontColorEnable)
                    cell.SetTextColor(daReadColor);

                if (defaultAppearance.UnreadFontColorEnable)
                    cell.SetTextColor(dp.IsReadByCurrent ? daReadColor : daUnreadColor);

                //if row appearance depends from line use line appearance           
                var lineAppearance = ServerConfig.SystemSettings.DocumentsModuleInfo.LineAppearances.FirstOrDefault(la => dp.Lines.Any(l => l.Guid == la.OriginatorGid));
                if (lineAppearance != null && lineAppearance.Enable)
                {
                    var laReadColor = new Color(lineAppearance.FontColor);
                    var laUnreadColor = new Color(lineAppearance.UnreadFontColor);
                    var laBgColor = new Color(lineAppearance.BackgroundColor);
                    cell.ItemView.SetBackgroundColor(laBgColor);
                    if (defaultAppearance.FontColorEnable)
                        cell.SetTextColor(laReadColor);
                    if (defaultAppearance.UnreadFontColorEnable)
                        cell.SetTextColor(dp.IsReadByCurrent ? laReadColor : laUnreadColor);

                }

                //if row appearance depends from user use user appearance           
                var userAppearance = ServerConfig.SystemSettings.DocumentsModuleInfo.UserAppearances.FirstOrDefault(la => dp.CreatorGuid == la.OriginatorGid);
                if (userAppearance != null && userAppearance.Enable)
                {
                    var uaReadColor = new Color(userAppearance.FontColor);
                    var uaUnreadColor = new Color(userAppearance.UnreadFontColor);
                    var uaBgColor = new Color(userAppearance.BackgroundColor);
                    cell.ItemView.SetBackgroundColor(uaBgColor);
                    if (defaultAppearance.FontColorEnable)
                        cell.SetTextColor(uaReadColor);
                    if (defaultAppearance.UnreadFontColorEnable)
                        cell.SetTextColor(dp.IsReadByCurrent ? uaReadColor : uaUnreadColor);
                }
            }

            void InitializeBookmarkAppearance(DocumentPreview dp, int folderId, DocumentPreviewViewHolder cell)
            {
                if (PlatformConfig.Preferences.HasBookmarkForFolder(folderId, dp.Id))
                    cell.ItemView.SetBackgroundColor(new Color(ContextCompat.GetColor(context, Resource.Color.brown)));
                else
                    cell.ItemView.SetBackgroundColor(new Color(ContextCompat.GetColor(context, Resource.Color.white)));
            }

            void InitializeBookmarkAppearance(DocumentPreview dp, int folderId, ExternalDocumentPreviewViewHolder cell)
            {
                if (PlatformConfig.Preferences.HasBookmarkForFolder(folderId, dp.Id))
                    cell.ItemView.SetBackgroundColor(new Color(ContextCompat.GetColor(context, Resource.Color.brown)));
                else
                    cell.ItemView.SetBackgroundColor(new Color(ContextCompat.GetColor(context, Resource.Color.white)));
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
 
                if (PlatformConfig.Preferences.SortByDate)
                {
                    Items.Sort();
                    foreach (var item in items)
                        if (!Items.Exists(k => k.Id == item.Id))
                            Items.AddSorted(item);
                }
                else
                    Items.InsertRange(0, items);

                NotifyItemRangeInserted(0, count);
            }

            public void AppendItems(List<DocumentPreview> items)
            {
                var count = Items.Count;
                Items.AddRange(items);
                NotifyItemRangeInserted(count, items.Count);
            }

            public void InsertItems(List<DocumentPreview> items)
            {
                var count = Items.Count;
                Items.Sort();
                foreach (var item in items)
                    if (!Items.Exists(k => k.Id == item.Id))
                        Items.AddSorted(item);
                NotifyItemRangeInserted(count, items.Count);
            }

            public void ReplaceItems(List<DocumentPreview> items)
            {
                Clear();
                AppendItems(items);
            }

            public void RefreshItems(List<DocumentPreview> items)
            {
                foreach (var item in items)
                {
                    var position = GetPosition(item);
                    if (position >= 0)
                        NotifyItemChanged(position);
                }
            }

            public void RefreshItems(List<int> itemsIds)
            {
                foreach (var itemId in itemsIds)
                {
                    var position = GetPosition(itemId);
                    if (position >= 0)
                        NotifyItemChanged(position);
                }
            }

            public void RemoveItems(List<DocumentPreview> items)
            {
                foreach (var item in items)
                {
                    var position = GetPosition(item);
                    if (position >= 0)
                    {
                        Items.RemoveAt(position);
                        NotifyItemRemoved(position);
                    }
                }
            }

            public void RemoveItemAtPosition(int position)
            {
                Items.RemoveAt(position);
                NotifyItemRemoved(position);
            }

            public bool IsSelected(DocumentPreview documentPreview)
            {
                return selectedDocumentsInView.ContainsKey(documentPreview.Id);
            }

            public void SetSelected(List<DocumentPreview> documentPreviews, bool selected)
            {
                foreach (var document in documentPreviews)
                    SetSelected(document, selected);
            }

            public void SetSelected(DocumentPreview documentPreview, bool selected)
            {
                var position = GetPosition(documentPreview);
                if (position < 0)
                    return;

                if (selected)
                    selectedDocumentsInView[documentPreview.Id] = documentPreview;
                else
                    selectedDocumentsInView.Remove(documentPreview.Id);
                NotifyItemChanged(position);
            }

            public void ClearSelections()
            {
                var documents = selectedDocumentsInView.Values.ToArray();
                selectedDocumentsInView.Clear();

                foreach (var document in documents)
                {
                    var position = GetPosition(document);
                    if (position >= 0)
                        NotifyItemChanged(position);
                }
            }

            public void Clear()
            {
                var size = Items.Count;
                Items.Clear();
                selectedDocumentsInView.Clear();
                NotifyItemRangeRemoved(0, size);
            }

            public int GetPosition(int documentPreviewId)
            {
                var position = -1;
                for (var i = 0; i < Items.Count; i++)
                    if (Items[i].Id == documentPreviewId)
                    {
                        position = i;
                        break;
                    }

                return position;
            }

            public int GetPosition(DocumentPreview documentPreview)
            {
                return GetPosition(documentPreview.Id);
            }

            public void SetSwipedState(int position, int direction)
            {
                swipedPosition = position;
                swipedDirection = direction;
            }

            public void ResetSwipedState()
            {
                swipedPosition = null;
            }

            public static class ViewType
            {
                public const int DocumentView = 0;
                public const int ExternalDocumentView = 1;
            }
        }

        class SwipeHelperCallback : ItemTouchHelper.Callback
        {
            public bool Enabled { get; set; } = true;

            readonly Context context;
            readonly DocumentsListAdapter adapter;
            readonly DocumentsListFragment fragment;
            readonly SwipeRefreshLayout refreshLayout;
            readonly Folder folder;

            Drawable leftBackground;
            Drawable rightBackground;

            public SwipeHelperCallback(Context context, DocumentsListFragment fragment, DocumentsListAdapter adapter, SwipeRefreshLayout refreshLayout, Folder folder)
            {
                this.context = context;
                this.fragment = fragment;
                this.adapter = adapter;
                this.refreshLayout = refreshLayout;
                this.folder = folder;
            }

            public override int GetMovementFlags(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder)
            {
                if (!Enabled)
                    return MakeMovementFlags(0, 0);

                return MakeMovementFlags(0, ItemTouchHelper.Left | ItemTouchHelper.Right);
            }

            public override bool OnMove(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, RecyclerView.ViewHolder target)
            {
                return false;
            }

            public override void OnSelectedChanged(RecyclerView.ViewHolder viewHolder, int actionState)
            {
                base.OnSelectedChanged(viewHolder, actionState);

                refreshLayout.Enabled = actionState == ItemTouchHelper.ActionStateIdle;
            }

            public override void OnChildDraw(Canvas c, RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, float dX, float dY, int actionState, bool isCurrentlyActive)
            {

                if (actionState != ItemTouchHelper.ActionStateSwipe || viewHolder.AdapterPosition == -1) //Sometimes it gets called for viewHolders that are already gone
                    return;

                var itemView = viewHolder.ItemView;
                var itemViewHeight = itemView.Bottom - itemView.Top;

                var paint = new TextPaint();
                paint.TextSize = (int)TypedValue.ApplyDimension(ComplexUnitType.Sp, 14, Android.App.Application.Context.Resources.DisplayMetrics);
                paint.Color = Color.White;
                paint.TextAlign = Paint.Align.Left;
                paint.SetTypeface(Typeface.Create(Typeface.Default, TypefaceStyle.Normal));

                var iconMargin = Conversion.ConvertDpToPixels(30);

                var baseline = -paint.Ascent();
                var textHeight = (int)(baseline + paint.Descent() + 0.5f);

                if (dX > 0) //Swiping to right
                {
                    Preferences.EmailSwipeAction action = PlatformConfig.Preferences.EmailLeadingSwipeAction;
                    if (!CheckActionEnabled(action))
                        return;

                    int bgColor = SwipeActionAllowed(action) ? Resource.Color.brown : Resource.Color.lightgray;
                    leftBackground = new ColorDrawable(new Color(ContextCompat.GetColor(context, bgColor)));
                    string text = GetSwipeActionTitle(action, viewHolder.AdapterPosition);
                    leftBackground.SetBounds(itemView.Left, itemView.Top, (int)dX, itemView.Bottom);
                    leftBackground.Draw(c);
                    var textLayout = new StaticLayout(text, paint, c.Width, Layout.Alignment.AlignNormal, 1, 0, false);
                    var textLeft = itemView.Left + iconMargin;
                    var textTop = itemView.Top + (itemViewHeight - textHeight) / 2;

                    c.Save();
                    c.Translate(textLeft, textTop);
                    textLayout.Draw(c);
                    c.Restore();
                }
                else if (dX < 0)
                {
                    Preferences.EmailSwipeAction action = PlatformConfig.Preferences.EmailTrailingSwipeAction;
                    if (!CheckActionEnabled(action))
                        return;

                    int bgColor = SwipeActionAllowed(action) ? Resource.Color.darkblue : Resource.Color.lightgray;
                    rightBackground = new ColorDrawable(new Color(ContextCompat.GetColor(context, bgColor)));
                    string text = GetSwipeActionTitle(action, viewHolder.AdapterPosition);
                    rightBackground.SetBounds(itemView.Right + (int)dX, itemView.Top, itemView.Right, itemView.Bottom);
                    rightBackground.Draw(c);
                    var textLayout = new StaticLayout(text, paint, c.Width, Layout.Alignment.AlignNormal, 1, 0, false);
                    var iconWidth = text.Split(new string[]
                            {
                                "\n"
                            },
                            StringSplitOptions.None)
                        .Select(s => (int)(paint.MeasureText(s) + 0.5f))
                        .Max();

                    var textRight = itemView.Right - iconMargin;
                    var textLeft = textRight - iconWidth;
                    var textTop = itemView.Top + itemViewHeight / 2 - textHeight;

                    c.Save();
                    c.Translate(textLeft, textTop);
                    textLayout.Draw(c);
                    c.Restore();
                }

                base.OnChildDraw(c, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive);
            }

            public override void OnSwiped(RecyclerView.ViewHolder viewHolder, int direction)
            {
                ResetViewHolder(viewHolder, direction);
                switch (direction)
                {
                    case ItemTouchHelper.Left:
                        if (CheckActionEnabled(PlatformConfig.Preferences.EmailTrailingSwipeAction))
                            SwipeActionSelected(PlatformConfig.Preferences.EmailTrailingSwipeAction, viewHolder.AdapterPosition);
                        break;
                    case ItemTouchHelper.Right:
                        if (CheckActionEnabled(PlatformConfig.Preferences.EmailLeadingSwipeAction))
                            SwipeActionSelected(PlatformConfig.Preferences.EmailLeadingSwipeAction, viewHolder.AdapterPosition);
                        break;
                }
            }

            private bool CheckActionEnabled(Preferences.EmailSwipeAction swipeAction)
            {
                if (swipeAction == Preferences.EmailSwipeAction.MoveToFolder)
                    return PlatformConfig.Preferences.EnableMoveToFolder;

                return true;
            }

            void ResetViewHolder(RecyclerView.ViewHolder viewHolder, int direction)
            {
                var position = viewHolder.AdapterPosition;
                var view = viewHolder.ItemView;

                viewHolder.ItemView.TranslationX = 0;
                viewHolder.ItemView.TranslationY = 0;

                adapter.SetSwipedState(position, 0);
                adapter.NotifyItemChanged(position);
            }

            async void SwipeActionSelected(Preferences.EmailSwipeAction action, int adapterPosition)
            {
                CommonConfig.UsageAnalytics.LogEvent(new SwipeActionUsedEvent());

                if (SwipeActionAllowed(action))
                {
                    switch (action)
                    {
                        case Preferences.EmailSwipeAction.Delete:
                            fragment.DeleteAction(new List<DocumentPreview>() { adapter.Items[adapterPosition] });
                            break;
                        case Preferences.EmailSwipeAction.CopyToFolder:
                            fragment.StartActivity(CopyMoveToFolderListActivity.CreateIntent(context, CopyMoveToFolderListActivity.ModeType.Copy, ModuleType.Documents,
                                                                                             new List<DocumentPreview> { adapter.Items[adapterPosition] }.Cast<IBusinessEntity>().ToList(), folder));
                            break;
                        case Preferences.EmailSwipeAction.More:
                            var index = await Dialogs.ShowListDialog(context, Resource.String.pref_email_swipe_dialog_title, Resource.Array.pref_email_swipe_actions_entries_without_more, true);
                            if (index >= 0)
                            {
                                SwipeActionSelected(PlatformConfig.Preferences.GetAllAvailableActions()[index], adapterPosition);
                            }
                            break;
                        case Preferences.EmailSwipeAction.CopyToWorkTray:
                            fragment.CopyToWorktrayAction(new List<DocumentPreview>() { adapter.Items[adapterPosition] });
                            break;
                        case Preferences.EmailSwipeAction.Categories:
                            fragment.ShowCategories(adapter.Items[adapterPosition]);
                            break;
                        case Preferences.EmailSwipeAction.MarkAsReadUnread:
                            if (!adapter.Items[adapterPosition].IsReadByCurrent)
                            {
                                fragment.MarkAsRead(new List<DocumentPreview>() { adapter.Items[adapterPosition] });
                            }
                            if (adapter.Items[adapterPosition].IsReadByCurrent)
                            {
                                fragment.MarkAsUnread(new List<DocumentPreview>() { adapter.Items[adapterPosition] });
                            }
                            break;
                        case Preferences.EmailSwipeAction.RemoveFromFolder:
                            fragment.DeleteFromFolderAction(new List<DocumentPreview>() { adapter.Items[adapterPosition] }, true);
                            break;
                        case Preferences.EmailSwipeAction.Priorities:
                            fragment.SetPriority(new List<DocumentPreview>() { adapter.Items[adapterPosition] });
                            break;
                        case Preferences.EmailSwipeAction.PresetCategory:
                            fragment.AssignPresetCategory(adapter.Items[adapterPosition]);
                            break;
                        case Preferences.EmailSwipeAction.MoveToFolder:
                            fragment.StartActivity(CopyMoveToFolderListActivity.CreateIntent(context, CopyMoveToFolderListActivity.ModeType.Move, ModuleType.Documents,
                                                                                             new List<DocumentPreview> { adapter.Items[adapterPosition] }.Cast<IBusinessEntity>().ToList(), folder));
                            break;
                        case Preferences.EmailSwipeAction.AddBookmark:
                            if (!PlatformConfig.Preferences.HasBookmarkForFolder(folder.Id, adapter.Items[adapterPosition].Id))
                                fragment.AddBookmark(adapter.Items[adapterPosition]);
                            else
                                fragment.RemoveBookmark(adapter.Items[adapterPosition]);
                            break;
                    }
                }

            }

            bool SwipeActionAllowed(Preferences.EmailSwipeAction action)
            {
                switch (action)
                {
                    case Preferences.EmailSwipeAction.CopyToWorkTray:
                        return ServerConfig.SystemSettings.DocumentsModuleInfo.WorktrayEnabled ?? true;
                    case Preferences.EmailSwipeAction.Delete:
                        return ServerConfig.SystemSettings.DocumentsModuleInfo.Permissions.DeleteAllowed || adapter.SelectedItems.All(dp => dp.Direction == DocumentDirection.Draft);
                    case Preferences.EmailSwipeAction.MoveToFolder:
                        return folder.InternalType == FolderInternalType.FilterView || folder.InternalType == FolderInternalType.Static || folder.InternalType == FolderInternalType.Worktray;
                    case Preferences.EmailSwipeAction.RemoveFromFolder:
                        return folder.InternalType == FolderInternalType.FilterView || folder.InternalType == FolderInternalType.Static || folder.InternalType == FolderInternalType.Worktray;
                    default:
                        return true;
                }
            }

            string GetSwipeActionTitle(Preferences.EmailSwipeAction action, int position)
            {
                switch (action)
                {
                    case Preferences.EmailSwipeAction.Delete:
                        return context.Resources.GetString(Resource.String.delete);
                    case Preferences.EmailSwipeAction.More:
                        return context.Resources.GetString(Resource.String.more);
                    case Preferences.EmailSwipeAction.MoveToFolder:
                        return context.Resources.GetString(Resource.String.move_to_folder);
                    case Preferences.EmailSwipeAction.MarkAsReadUnread:
                        if (!adapter.Items[position].IsReadByCurrent)
                        {
                            return context.Resources.GetString(Resource.String.mark_as_read);
                        }
                        if (adapter.Items[position].IsReadByCurrent)
                        {
                            return context.Resources.GetString(Resource.String.marks_as_unread);
                        }
                        return "";
                    case Preferences.EmailSwipeAction.Categories:
                        return context.Resources.GetString(Resource.String.categories);
                    case Preferences.EmailSwipeAction.RemoveFromFolder:
                        return context.Resources.GetString(Resource.String.remove_from_folder);
                    case Preferences.EmailSwipeAction.CopyToWorkTray:
                        return context.Resources.GetString(Resource.String.copy_to_worktray);
                    case Preferences.EmailSwipeAction.CopyToFolder:
                        return context.Resources.GetString(Resource.String.copy_to_folder);
                    case Preferences.EmailSwipeAction.Priorities:
                        return context.Resources.GetString(Resource.String.priority);
                    case Preferences.EmailSwipeAction.PresetCategory:
                        return context.Resources.GetString(Resource.String.set_preset_category);
                    case Preferences.EmailSwipeAction.AddBookmark:
                        if (!PlatformConfig.Preferences.HasBookmarkForFolder(folder.Id, adapter.Items[position].Id))
                            return context.Resources.GetString(Resource.String.add_bookmark);
                        else
                            return context.Resources.GetString(Resource.String.remove_bookmark);
                    default:
                        return "Forgot case ?";
                }
            }
        }

        class DocumentPreviewViewHolder : RecyclerView.ViewHolder
        {
            public int FolderId { get; set; }

            public string Recipent { set => recipentTextView.Text = value; }

            public string Date { set => dateTextView.Text = value; }

            public string BubbleDate { get; set; }

            public string Subject { set => subjectTextView.Text = value; }

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

            public bool IncomingIndicator { set => incomingImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }

            public bool OutgoingIndicator { set => outgoingImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }

            public bool DelayedIndicator { set => delayedImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }

            public bool CancelledIndicator { set => cancelledImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }

            public bool PartiallySentIndicator { set => partiallySentImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }

            public bool FailIndicator { set => failImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }

            public bool DraftIndicator { set => draftImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }

            public bool UnreadIndicator { set => unreadImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }

            public bool AttachmentIndicator { set => attachmentImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }

            public bool CommentIndicator { set => commentImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }
            
            public bool PriorityHighIndicator { set => priorityHighImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }
            
            public bool PriorityLowIndicator { set => priorityLowImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }

            public bool Compact
            {
                set
                {
                    attachmentImageView.Visibility = value ? ViewStates.Gone : attachmentImageView.Visibility;
                    commentImageView.Visibility = value ? ViewStates.Gone : commentImageView.Visibility;
                    previewTextView.Visibility = value ? ViewStates.Gone : previewTextView.Visibility;
                }
            }

            public bool Selected { set => selectedOverlay.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }

            public int SwipedDirection
            {
                set
                {
                    if (value != 0)
                    {
                        swipedBackground.Visibility = ViewStates.Visible;
                        itemContent.Visibility = ViewStates.Invisible; //Otherwisw the view collapses

                        var colorId = value == ItemTouchHelper.Left ? Resource.Color.darkerblue : Resource.Color.brown;

                        swipedBackground.SetBackgroundColor(new Color(ContextCompat.GetColor(ItemView.Context, colorId)));
                    }
                    else
                    {
                        swipedBackground.Visibility = ViewStates.Gone;
                        itemContent.Visibility = ViewStates.Visible;
                    }
                }
            }

            public void SetTextColor(Color color)
            {
                recipentTextView.SetTextColor(color);
                subjectTextView.SetTextColor(color);
                previewTextView.SetTextColor(color);
            }

            readonly AppCompatTextView recipentTextView;
            readonly AppCompatTextView dateTextView;
            readonly AppCompatTextView subjectTextView;
            readonly AppCompatTextView previewTextView;
            readonly LinearLayoutCompat categoriesLayout;
            readonly AppCompatImageView incomingImageView;
            readonly AppCompatImageView outgoingImageView;
            readonly AppCompatImageView delayedImageView;
            readonly AppCompatImageView cancelledImageView;
            readonly AppCompatImageView draftImageView;
            readonly AppCompatImageView unreadImageView;
            readonly AppCompatImageView partiallySentImageView;
            readonly AppCompatImageView failImageView;
            readonly AppCompatImageView attachmentImageView;
            readonly AppCompatImageView commentImageView;
            readonly AppCompatImageView priorityHighImageView;
            readonly AppCompatImageView priorityLowImageView;
            readonly LinearLayoutCompat itemContent;
            readonly View selectedOverlay;
            readonly View swipedBackground;

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
                delayedImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_direction_delayed);
                cancelledImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_failed);
                failImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_failed);
                partiallySentImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_partially_sent);
                draftImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_direction_draft);
                unreadImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_unread);
                attachmentImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_attachment);
                commentImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_comment);
                priorityHighImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_high_priority);
                priorityLowImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_low_priority);
                itemContent = itemView.FindViewById<LinearLayoutCompat>(Resource.Id.list_item_document_internal_layout);
                selectedOverlay = itemView.FindViewById<View>(Resource.Id.selected_overlay);
                swipedBackground = itemView.FindViewById<View>(Resource.Id.swiped_background);
            }
        }

        class ExternalDocumentPreviewViewHolder : RecyclerView.ViewHolder
        {
            public int FolderId { get; set; }

            public string Name { set => nameTextView.Text = value; }

            public string Date { set => dateTextView.Text = value; }

            public string BubbleDate { get; set; }

            public string Preview { set => previewTextView.Text = value; }

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

            public bool CommentIndicator { set => commentImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }
           
            public bool PriorityHighIndicator { set => priorityHighImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }
            
            public bool PriorityLowIndicator { set => priorityLowImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }
            
            public bool Selected { set => selectedOverlay.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }

            public int SwipedDirection
            {
                set
                {
                    if (value != 0)
                    {
                        swipedBackground.Visibility = ViewStates.Visible;
                        itemContent.Visibility = ViewStates.Invisible;

                        var colorId = value == ItemTouchHelper.Left ? Resource.Color.darkerblue : Resource.Color.brown;

                        swipedBackground.SetBackgroundColor(new Color(ContextCompat.GetColor(ItemView.Context, colorId)));
                    }
                    else
                    {
                        swipedBackground.Visibility = ViewStates.Gone;
                        itemContent.Visibility = ViewStates.Visible;
                    }
                }
            }

            public void SetTextColor(Color color)
            {
                nameTextView.SetTextColor(color);
                previewTextView.SetTextColor(color);
            }

            readonly AppCompatTextView nameTextView;
            readonly AppCompatTextView dateTextView;
            readonly AppCompatTextView previewTextView;
            readonly LinearLayoutCompat categoriesLayout;
            readonly AppCompatImageView commentImageView;
            readonly AppCompatImageView priorityHighImageView;
            readonly AppCompatImageView priorityLowImageView;
            readonly LinearLayoutCompat itemContent;
            readonly View selectedOverlay;
            readonly View swipedBackground;

            public ExternalDocumentPreviewViewHolder(View itemView)
                : base(itemView)
            {
                nameTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_document_external_name);
                dateTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_document_external_date);
                previewTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_document_external_preview);
                categoriesLayout = itemView.FindViewById<LinearLayoutCompat>(Resource.Id.list_item_document_external_categories);
                commentImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_external_comment);
                priorityHighImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_external_high_priority);
                priorityLowImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_external_low_priority);
                itemContent = itemView.FindViewById<LinearLayoutCompat>(Resource.Id.list_item_document_external_internal_layout);
                selectedOverlay = itemView.FindViewById<View>(Resource.Id.selected_overlay);
                swipedBackground = itemView.FindViewById<View>(Resource.Id.swiped_background);
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
                            if (cts.IsCancellationRequested)
                                break;

                            var first = firstOrDefaultItem();
                            if (first != null)
                                await work(first.Id);
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