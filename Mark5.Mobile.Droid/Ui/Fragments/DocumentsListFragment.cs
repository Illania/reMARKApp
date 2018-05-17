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
using Android.Support.V4.Content;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Support.V7.Widget.Helper;
using Android.Text;
using Android.Util;
using Android.Views;
using FastScrollRecycler;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.HubMessages;
using Mark5.Mobile.Common.Service;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class DocumentsListFragment : BaseFragment, ActionMode.ICallback, IMenuItemOnActionExpandListener, SearchView.IOnQueryTextListener
    {
        public Folder Folder { get; set; }

        DocumentsListAdapter CurrentAdapter => (DocumentsListAdapter)recyclerView.GetAdapter();

        readonly Handler searchHandler = new Handler();

        const string FolderBundleKey = "Folder_5ab3effc-9a60-4b26-805e-72a0c3527b0d";
        const string SelectedDocumentPreviewsKey = "SelectedDocumentPreviews_9d33e0b7-9791-4ee9-82bd-73af5c0b5716";
        const string FirstRowIdKey = "FirstRowId_ab73aa33-930f-4139-94b1-b7828d5f4de7";
        const string LastRowIdKey = "LastRowId_a92f8e84-7274-48e3-9296-3d52a9b3231c";


        const int AutoRefreshIntervalMs = 5 * 1000; // 5 seconds

        bool refreshing;

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

        bool shouldNotifyAdapter;
        bool shouldNotifySearchAdapter;

        int savedFirstRowId = -1;
        int savedLastRowId = -1;
        List<DocumentPreview> savedSelectedDocumentPreviews;

        AutoRefreshWorker autoRefreshWorker;

        public static (DocumentsListFragment fragment, string tag) NewInstance(Folder folder)
        {
            var args = new Bundle();

            if (folder != null)
                args.PutString(FolderBundleKey, Serializer.Serialize(folder));

            var fragment = new DocumentsListFragment();
            fragment.Arguments = args;

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

            swipeHelperCallback = new SwipeHelperCallback(Context, this, adapter, refreshLayout);
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

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.documents);
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = Folder?.Name;

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

            var searchItem = menu.Add(Menu.None, 10, Menu.None, Resource.String.search);
            searchItem.SetIcon(Resource.Drawable.action_search_server);
            searchItem.SetShowAsAction(ShowAsAction.Always);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == 10)
            {
                StartActivity(SearchActivity.CreateIntent(Context, ModuleType.Documents));
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

                if (documents.Count > 0)
                {
                    CommonConfig.Logger.Info($"Received {documents?.Count} new documents");

                    var snackbar = Snackbar.Make(coordinatorLayout, Resources.GetQuantityString(Resource.Plurals.new_documents_received, documents.Count, documents.Count), Snackbar.LengthShort);
                    snackbar.View.SetBackgroundColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.darkerblue)));
                    snackbar.Show();

                    Activity?.RunOnUiThread(() => { adapter?.PrependItems(documents); });

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
                adapter.EnableLoadMore = documentPreviews.Count >= PlatformConfig.Preferences.DocumentsToDownload;
                CommonConfig.Logger.Info($"Enable load more documents set to {adapter.EnableLoadMore}");

                if (forceClear)
                    adapter.Clear();

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

        void Adapter_ItemClicked(object sender, DocumentPreview documentPreview)
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

        void Adapter_ItemLongClicked(object sender, DocumentPreview documentPreview)
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
            Activity.Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
            Activity.Window.SetStatusBarColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));

            swipeHelperCallback.Enabled = false;
            fab?.Hide();

            menu.Clear();

            if (CurrentAdapter.SelectedItems.Any(dp => !dp.IsReadByCurrent))
                menu.Add(Menu.None, MenuItemActions.MarkAsRead, MenuItemActions.MarkAsRead, Resource.String.mark_as_read);

            if (CurrentAdapter.SelectedItems.Any(dp => dp.IsReadByCurrent))
                menu.Add(Menu.None, MenuItemActions.MarkAsUnread, MenuItemActions.MarkAsUnread, Resource.String.marks_as_unread);

            menu.Add(Menu.None, MenuItemActions.CopyToWorktray, MenuItemActions.CopyToWorktray, Resource.String.copy_to_worktray);
            menu.Add(Menu.None, MenuItemActions.CopyToFolder, MenuItemActions.CopyToFolder, Resource.String.copy_to_folder);

            if (Folder.InternalType == FolderInternalType.FilterView || Folder.InternalType == FolderInternalType.Static || Folder.InternalType == FolderInternalType.Worktray)
                menu.Add(Menu.None, MenuItemActions.MoveToFolder, MenuItemActions.MoveToFolder, Resource.String.move_to_folder);

            menu.Add(Menu.None, MenuItemActions.SetPriority, MenuItemActions.SetPriority, Resource.String.set_priority);

            if (CurrentAdapter.SelectedItemCount == CurrentAdapter.ItemCount)
                menu.Add(Menu.None, MenuItemActions.UnselectAll, MenuItemActions.UnselectAll, Resource.String.unselect_all);
            else
                menu.Add(Menu.None, MenuItemActions.SelectAll, MenuItemActions.SelectAll, Resource.String.select_all);

            if (CurrentAdapter.SelectedItemCount == 1)
                menu.Add(Menu.None, MenuItemActions.Categories, MenuItemActions.Categories, Resource.String.categories);
            
            if (Folder.InternalType == FolderInternalType.FilterView || Folder.InternalType == FolderInternalType.Static || Folder.InternalType == FolderInternalType.Worktray)
                menu.Add(Menu.None, MenuItemActions.DeleteFromFolder, MenuItemActions.DeleteFromFolder, Resource.String.delete_from_folder);

            if (ServerConfig.SystemSettings.DocumentsModuleInfo.Permissions.DeleteAllowed || CurrentAdapter.SelectedItems.All(dp => dp.Direction == DocumentDirection.Draft))
                menu.Add(Menu.None, MenuItemActions.Delete, MenuItemActions.Delete, Resource.String.delete);

            return true;
        }

        bool ActionMode.ICallback.OnActionItemClicked(ActionMode mode, IMenuItem item)
        {
            if (item.ItemId == MenuItemActions.MarkAsRead)
            {
                MarkAsRead();
                return true;
            }

            if (item.ItemId == MenuItemActions.MarkAsUnread)
            {
                MarkAsUnread();
                return true;
            }

            if (item.ItemId == MenuItemActions.CopyToWorktray)
            {
                CopyToWorktrayAction();
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
                SetPriority();
                return true;
            }

            if (item.ItemId == MenuItemActions.SelectAll)
            {
                SelectAll();
                return true;
            }

            if (item.ItemId == MenuItemActions.UnselectAll)
            {
                UnSelectAll();
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
                DeleteFromFolderAction();
                return true;
            }

            if (item.ItemId == MenuItemActions.Delete)
            {
                DeleteAction();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        void ActionMode.ICallback.OnDestroyActionMode(ActionMode mode)
        {
            Activity.Window.AddFlags(WindowManagerFlags.TranslucentStatus);
            Activity.Window.SetStatusBarColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));

            swipeHelperCallback.Enabled = true;
            fab?.Show();

            CurrentAdapter.ClearSelections();
            actionMode = null;
        }

        public async Task CopyToOwnWorktray(List<DocumentPreview> documentPreviews)
        {
            CommonConfig.Logger.Info($"Attempting copy to worktray [businessEntities.Count={documentPreviews.Count}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.copying_to_worktray, Resource.String.please_wait);

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

        async void MarkAsRead()
        {
            CommonConfig.Logger.Info($"Attempting to mark as read [businessEntities.Count={CurrentAdapter.SelectedItemCount}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.marking_as_read, Resource.String.please_wait);

            try
            {
                CommonConfig.UsageAnalytics.LogEvent(new SetReadStatusEvent(CurrentAdapter.SelectedItems.Count));

                await Managers.DocumentsManager.SetDocumentsReadStatusAsync(CurrentAdapter.SelectedItems, true);
                adapter.RefreshItems(CurrentAdapter.SelectedItems);
                searchAdapter.RefreshItems(CurrentAdapter.SelectedItems);

                dismissAction();
                actionMode?.Finish();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Marking as read failed [businessEntities.Count={CurrentAdapter.SelectedItemCount}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        async void MarkAsUnread()
        {
            CommonConfig.Logger.Info($"Attempting to mark as unread [businessEntities.Count={CurrentAdapter.SelectedItemCount}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.marking_as_unread, Resource.String.please_wait);

            try
            {
                CommonConfig.UsageAnalytics.LogEvent(new SetReadStatusEvent(CurrentAdapter.SelectedItems.Count));

                await Managers.DocumentsManager.SetDocumentsReadStatusAsync(CurrentAdapter.SelectedItems, false);
                adapter.RefreshItems(CurrentAdapter.SelectedItems);
                searchAdapter.RefreshItems(CurrentAdapter.SelectedItems);

                dismissAction();
                actionMode?.Finish();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Marking as unread failed [businessEntities.Count={CurrentAdapter.SelectedItemCount}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        async void CopyToWorktrayAction()
        {
            var option = await Dialogs.ShowListDialog(Context, Resource.String.copy_to_worktray, Resource.Array.copy_to_worktray_options, true);

            if (option == 0)
                await CopyToOwnWorktray(CurrentAdapter.SelectedItems);

            if (option == 1)
            {
                StartActivity(CopyToUserWorktrayActivity.CreateIntent(Context, CurrentAdapter.SelectedItems.Cast<IBusinessEntity>().ToList()));
            }
        }

        async void CopyToOwnWorktray(DocumentPreview documentPreview)
        {
            await CopyToOwnWorktray(new List<DocumentPreview>
            {
                documentPreview
            });
        }

        async void SetPriority()
        {
            var possiblePriorities = new List<Priority>
            {
                Priority.Urgent,
                Priority.Normal,
                Priority.Low
            };
            var selectedPriority = CurrentAdapter.SelectedItems.All(dp => dp.Priority == CurrentAdapter.SelectedItems[0].Priority) ? CurrentAdapter.SelectedItems[0].Priority : Priority.None;

            if (!possiblePriorities.Contains(selectedPriority))
                selectedPriority = Priority.Normal;

            var priority = await Dialogs.ShowSingleSelectDialogAsync(Context, Resource.String.set_priority, possiblePriorities, selectedPriority);
            if (priority == default(Priority) || priority == selectedPriority)
                return;

            CommonConfig.Logger.Info($"Attempting to set priority [businessEntities.Count={CurrentAdapter.SelectedItemCount}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.setting_priority, Resource.String.please_wait);

            try
            {
                await Managers.DocumentsManager.SetDocumentsPriorityAsync(CurrentAdapter.SelectedItems, priority);

                dismissAction();
                actionMode?.Finish();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Setting priority failed [businessEntities.Count={CurrentAdapter.SelectedItemCount}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        void SelectAll()
        {
            CurrentAdapter.SetSelected(CurrentAdapter.Items, true);
            actionMode.Title = CurrentAdapter.SelectedItemCount.ToString();
            actionMode.Invalidate();
        }

        void UnSelectAll()
        {
            CurrentAdapter.SetSelected(CurrentAdapter.Items, false);
            actionMode.Title = CurrentAdapter.SelectedItemCount.ToString();
            actionMode.Invalidate();
        }

        async void DeleteFromFolderAction()
        {
            var yesNo = await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.delete_from_folder, Resource.String.delete_from_folder_are_you_sure);
            if (!yesNo)
                return;

            CommonConfig.Logger.Info($"Attempting to delete from folder [businessEntities.Count={CurrentAdapter.SelectedItemCount}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.deleting_from_folder, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.RemoveFromFolder(CurrentAdapter.SelectedItems.OfType<IBusinessEntity>().ToList(), Folder);

                dismissAction();
                actionMode?.Finish();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Deleting from folder failed [businessEntities.Count={CurrentAdapter.SelectedItemCount}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        async void DeleteAction()
        {
            var yesNo = await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.delete, Resource.String.delete_are_you_sure);
            if (!yesNo)
                return;

            CommonConfig.Logger.Info($"Attempting to delete [businessEntities.Count={CurrentAdapter.SelectedItemCount}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.deleting, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.Delete(CurrentAdapter.SelectedItems.OfType<IBusinessEntity>().ToList());

                dismissAction();
                actionMode?.Finish();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Deleting failed [businessEntities.Count={CurrentAdapter.SelectedItemCount}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        static class MenuItemActions
        {
            public const int MarkAsRead = 10;
            public const int MarkAsUnread = 11;
            public const int CopyToWorktray = 20;
            public const int CopyToFolder = 30;
            public const int MoveToFolder = 31;
            public const int SetPriority = 40;
            public const int Categories = 50;
            public const int DeleteFromFolder = 60;
            public const int Delete = 61;
            public const int SelectAll = 70;
            public const int UnselectAll = 71;
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
            }

            position = searchAdapter.GetPosition(m.EntityId);
            if (position >= 0)
            {
                shouldNotifySearchAdapter = true;
                var dp = searchAdapter.Items[position];
                dp.Categories.Clear();
                dp.Categories.AddRange(m.Categories);
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

        #endregion

        #region RecyclerView Adapter/ViewHolder

        protected class DocumentsListAdapter : RecyclerView.Adapter, ISectionedAdapter
        {
            public override int ItemCount => Items.Count;

            public List<DocumentPreview> Items { get; } = new List<DocumentPreview>(1000);

            public List<DocumentPreview> SelectedItems => selectedDocumentsInView.Values.ToList();

            public int SelectedItemCount => selectedDocumentsInView.Count;

            public bool EnableLoadMore { get; set; }

            public event EventHandler<DocumentPreview> ItemClicked = delegate { };
            public event EventHandler<DocumentPreview> ItemLongClicked = delegate { };

            readonly Dictionary<int, DocumentPreview> selectedDocumentsInView = new Dictionary<int, DocumentPreview>();
            readonly Context context;
            readonly RecyclerView recyclerView;
            readonly Action<int> loadMoreAction;

            bool unreadIndicatorMe = PlatformConfig.Preferences.UnreadIndicatorMe;
            bool compactList = PlatformConfig.Preferences.CompactDocumentsList;
            bool showCreatorOutgoing = PlatformConfig.Preferences.ShowCreatorOutgoing;

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

                    dpvh.Subject = string.IsNullOrWhiteSpace(dp.Subject) ? context.GetString(Resource.String.no_subject) : dp.Subject;
                    var d = dp.DateReceivedTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime().ConvertDateTimeToTimestampMilliseconds();
                    dpvh.Date = d.FormatUserTimestampAsCompactShortDateTimeString(context);
                    dpvh.BubbleDate = d.FormatUserTimestampAsCompactLongDateTimeString(context);
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
                }
                else if (holder is ExternalDocumentPreviewViewHolder)
                {
                    var edpvh = holder as ExternalDocumentPreviewViewHolder;

                    edpvh.SwipedDirection = position == swipedPosition ? swipedDirection : 0;

                    edpvh.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, dp)));
                    edpvh.ItemView.SetOnLongClickListener(new ActionOnLongClickListener(() => ItemLongClicked(this, dp)));

                    var d = dp.DateReceivedTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime().ConvertDateTimeToTimestampMilliseconds();
                    edpvh.Date = d.FormatUserTimestampAsCompactShortDateTimeString(context);
                    edpvh.BubbleDate = d.FormatUserTimestampAsCompactLongDateTimeString(context);
                    edpvh.Name = string.IsNullOrWhiteSpace(dp.Subject) ? context.GetString(Resource.String.no_subject) : dp.Subject;
                    edpvh.Preview = string.IsNullOrWhiteSpace(dp.Preview) ? context.GetString(Resource.String.no_content) : Regex.Replace(dp.Preview, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
                    edpvh.Categories = dp.Categories;
                    edpvh.CommentIndicator = dp.CommentsCount > 0;

                    edpvh.Selected = selectedDocumentsInView.ContainsKey(dp.Id);
                }

                if (recyclerView != null && loadMoreAction != null && position == ItemCount - 1 && EnableLoadMore)
                {
                    CommonConfig.UsageAnalytics.LogEvent(new GetMoreDocumentsEvent());

                    loadMoreAction(dp.Id);
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
                Items.InsertRange(0, items);
                NotifyItemRangeInserted(0, count);
            }

            public void AppendItems(List<DocumentPreview> items)
            {
                var count = Items.Count;
                Items.AddRange(items);
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

            readonly Drawable leftBackground;
            readonly Drawable rightBackground;

            public SwipeHelperCallback(Context context, DocumentsListFragment fragment, DocumentsListAdapter adapter, SwipeRefreshLayout refreshLayout)
            {
                this.context = context;
                this.fragment = fragment;
                this.adapter = adapter;
                this.refreshLayout = refreshLayout;

                leftBackground = new ColorDrawable(new Color(ContextCompat.GetColor(context, Resource.Color.brown)));
                rightBackground = new ColorDrawable(new Color(ContextCompat.GetColor(context, Resource.Color.darkblue)));
            }

            public override int GetMovementFlags(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder)
            {
                if (!Enabled)
                    return MakeMovementFlags(0, 0);

                if (fragment.Folder?.InternalType == FolderInternalType.Worktray)
                    return MakeMovementFlags(0, ItemTouchHelper.Right);

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
                    var text = context.Resources.GetString(Resource.String.categories);

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
                    var text = context.Resources.GetString(Resource.String.copy_to_worktray_multiline);

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
                if (direction == ItemTouchHelper.Left)
                    fragment.CopyToOwnWorktray(adapter.Items[viewHolder.AdapterPosition]);
                else if (direction == ItemTouchHelper.Right)
                    fragment.ShowCategories(adapter.Items[viewHolder.AdapterPosition]);

                ResetViewHolder(viewHolder, direction);
            }

            void ResetViewHolder(RecyclerView.ViewHolder viewHolder, int direction)
            {
                var position = viewHolder.AdapterPosition;
                var view = viewHolder.ItemView;

                viewHolder.ItemView.TranslationX = 0;
                viewHolder.ItemView.TranslationY = 0;

                adapter.SetSwipedState(position, direction);
                adapter.NotifyItemChanged(position);

                viewHolder.ItemView.PostDelayed(() =>
                    {
                        adapter.ResetSwipedState();
                        adapter.NotifyItemChanged(position);
                    },
                    400);
            }
        }

        class DocumentPreviewViewHolder : RecyclerView.ViewHolder
        {
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

            public bool DraftIndicator { set => draftImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }

            public bool UnreadIndicator { set => unreadImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }

            public bool AttachmentIndicator { set => attachmentImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }

            public bool CommentIndicator { set => commentImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }

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
                draftImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_direction_draft);
                unreadImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_unread);
                attachmentImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_attachment);
                commentImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_comment);
                itemContent = itemView.FindViewById<LinearLayoutCompat>(Resource.Id.list_item_document_internal_layout);
                selectedOverlay = itemView.FindViewById<View>(Resource.Id.selected_overlay);
                swipedBackground = itemView.FindViewById<View>(Resource.Id.swiped_background);
            }
        }

        class ExternalDocumentPreviewViewHolder : RecyclerView.ViewHolder
        {
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

            readonly AppCompatTextView nameTextView;
            readonly AppCompatTextView dateTextView;
            readonly AppCompatTextView previewTextView;
            readonly LinearLayoutCompat categoriesLayout;
            readonly AppCompatImageView commentImageView;
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