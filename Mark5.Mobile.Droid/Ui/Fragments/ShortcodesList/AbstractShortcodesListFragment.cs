using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using FastScrollRecycler;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.DataAccess.Exceptions;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.HubMessages;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public abstract class AbstractShortcodesListFragment : BaseFragment, ActionMode.ICallback, IMenuItemOnActionExpandListener, SearchView.IOnQueryTextListener
    {
        static class RequestCodes
        {
            public const int SaveOfflineRequest = 1;
        }

        public Folder Folder { get; set; }

        protected ShortcodesListAdapter CurrentAdapter => (ShortcodesListAdapter)recyclerView.GetAdapter();

        readonly Handler searchHandler = new Handler();

        protected const string FolderBundleKey = "BundleKey_002d532f-2acf-4ee2-9bb1-e9601e5bf83e";

        protected const string ShortcodePreviewsKey = "ShortcodePreviews_3eca9281-9489-4c2e-82a7-918367c06e8f";
        protected const string SelectedShortcodePreviewsKey = "SelectedPreviews_7b36f3cd-ded2-4e09-a138-76019d2bb358";
        protected const string RefreshInProgressKey = "RefreshInProgress_ba32b92c-1d9e-44d1-b25a-7695864ee1de";

        protected ActionMode ActionMode;

        bool refreshing;

        List<ShortcodePreview> savedSelectedShortcodes;
        bool savedRefreshInProgress;

        IMenu menu;
        SwipeRefreshLayout refreshLayout;
        RecyclerView recyclerView;
        ShortcodesListAdapter adapter;
        ShortcodesListAdapter searchAdapter;
        SearchView searchView;

        bool shouldNotifyAdapter;
        bool shouldNotifySearchAdapter;

        CancellationTokenSource cts;

        #region Fragment overrides

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Arguments.ContainsKey(FolderBundleKey))
                Folder = Serializer.Deserialize<Folder>(Arguments.GetString(FolderBundleKey));

            savedRefreshInProgress = savedInstanceState?.ContainsKey(RefreshInProgressKey) == true && savedInstanceState.GetBoolean(RefreshInProgressKey);

            if (savedInstanceState?.ContainsKey(SelectedShortcodePreviewsKey) == true)
                savedSelectedShortcodes = savedInstanceState.ContainsKey(SelectedShortcodePreviewsKey) ?
                                                              Serializer.Deserialize<List<ShortcodePreview>>(savedInstanceState.GetString(SelectedShortcodePreviewsKey)) : null;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(AbstractShortcodesListFragment)} [folder.id={Folder?.Id}, folder.name={Folder?.Name}]...");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            var emptyView = rootView.FindViewById<AppCompatTextView>(Resource.Id.empty_view);
            emptyView.SetText(Resource.String.empty_folder);

            refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.SetColorSchemeResources(Resource.Color.blue, Resource.Color.darkerblue);
            refreshLayout.Refresh += (sender, e) =>
            {
                CommonConfig.UsageAnalytics.LogEvent(new PullToRefreshEvent(false, module: ModuleType.Shortcodes));

                ActionMode?.Finish();
                ActionMode = null;

                RefreshData(force: true);
            };

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));
            recyclerView.HasFixedSize = true;

            adapter = new ShortcodesListAdapter();
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

            searchAdapter = new ShortcodesListAdapter();
            searchAdapter.ItemClicked += Adapter_ItemClicked;
            searchAdapter.ItemLongClicked += Adapter_ItemLongClicked;

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.shortcodes);
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = Folder?.Name;

            CommonConfig.Logger.Info($"Created {nameof(AbstractShortcodesListFragment)} [folder.id={Folder?.Id}, folder.name={Folder?.Name}]");
        }

        public override void OnResume()
        {
            base.OnResume();

            CommonConfig.Logger.Info($"Resuming {nameof(AbstractShortcodesListFragment)} [folder.id={Folder?.Id}, folder.name={Folder?.Name}]...");

            if (adapter.ItemCount < 1)
                RefreshData();

            if (shouldNotifyAdapter)
            {
                shouldNotifyAdapter = false;
                ActionMode?.Finish();
                ActionMode = null;
                adapter.NotifyDataSetChanged();
            }
            if (shouldNotifySearchAdapter)
            {
                shouldNotifySearchAdapter = false;
                ActionMode?.Finish();
                ActionMode = null;
                searchAdapter.NotifyDataSetChanged();
            }
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if (requestCode == RequestCodes.SaveOfflineRequest && resultCode == (int)Result.Ok)
                RefreshData(force: true, skipOfflineCheck: true);
        }

        public override void OnPause()
        {
            base.OnPause();

            CommonConfig.Logger.Info($"Pausing {nameof(AbstractShortcodesListFragment)} [folder.id={Folder?.Id}, folder.name={Folder?.Name}]...");

            cts?.Cancel();
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            if (adapter?.Items != null)
                outState.PutString(ShortcodePreviewsKey, Serializer.Serialize(adapter.Items));

            if (adapter?.SelectedItems != null)
                outState.PutString(SelectedShortcodePreviewsKey, Serializer.Serialize(adapter.SelectedItems));

            outState.PutBoolean(RefreshInProgressKey, refreshing);
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
                StartActivity(SearchActivity.CreateIntent(Context, ModuleType.Shortcodes));
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Refreshing

        async void RefreshData(int startRowId = -1, bool force = false, bool skipOfflineCheck = false)
        {
            CommonConfig.Logger.Info($"Attempting refresh [startRowId={startRowId}, force={force}]...");

            if (refreshing)
                return;

            var isSavedOffline = await Managers.FoldersManager.IsSavedFolderOfflineInfo(Folder);

            cts?.Cancel();
            cts = new CancellationTokenSource();

            refreshing = true;
            refreshLayout.Refreshing = true;

            if (force && !skipOfflineCheck && isSavedOffline)
            {
                var result = await Dialogs.ShowYesNoCancelDialogAsync(Activity,
                                                                      Resource.String.folder_offline_title,
                                                                      Resource.String.folder_offline_message,
                                                                      Resource.String.folder_offline_go_online,
                                                                      Resource.String.folder_offline_redownload,
                                                                      Resource.String.cancel);

                if (result == 1)
                    await Managers.FoldersManager.RemoveSavedFolderInfo(Folder);
                if (result == 0)
                {
                    StartActivityForResult(DownloadActivity.CreateIntent(Context, Folder), RequestCodes.SaveOfflineRequest);
                    refreshLayout.Refreshing = false;
                    refreshing = false;

                    return;
                }
                if (result == -1)
                {
                    refreshLayout.Refreshing = false;
                    refreshing = false;

                    return;
                }
            }

            CommonConfig.Logger.Info($"Refresh running...");

            if (force)
                adapter.Clear();

            var sourceType = isSavedOffline || Restored ? SourceType.Local : SourceType.Auto;

            Managers.ShortcodesManager.GetAllShortcodePreviews(Folder,
                cps =>
                {
                    CommonConfig.Logger.Debug($"Retrieved {cps?.Count} contacts");

                    Activity.RunOnUiThread(() => adapter.AppendItems(cps));
                },
                () =>
                {
                    refreshLayout.Refreshing = false;
                    refreshing = false;

                    CommonConfig.Logger.Info($"Refresh finished");
                },
                ex =>
                {
                    CommonConfig.Logger.Error($"Downloading shortcodes failed [folder.name={Folder?.Name}, folder.id={Folder?.Id}, startRowId={startRowId}, force={force}]", ex);

                    if (!(ex is DataNotFoundException && Restored))
                        Dialogs.ShowErrorDialog(Activity, ex);

                    if (adapter.ItemCount < 1)
                        Activity?.OnBackPressed();
                },
                startRowId, cts.Token, sourceType);
        }

        #endregion

        #region Action mode

        public bool OnCreateActionMode(ActionMode mode, IMenu menu)
        {
            return true;
        }

        public bool OnActionItemClicked(ActionMode mode, IMenuItem item)
        {
            if (item.ItemId == MenuItemActions.CopyToWorktray)
            {
                CopyToWorktrayAction();
                return true;
            }

            if (item.ItemId == MenuItemActions.CopyToFolder)
            {
                StartActivity(CopyMoveToFolderListActivity.CreateIntent(Context, CopyMoveToFolderListActivity.ModeType.Copy, ModuleType.Shortcodes, CurrentAdapter.SelectedItems.Select(sp => sp).Cast<IBusinessEntity>().ToList()));
                ActionMode?.Finish();
                return true;
            }

            if (item.ItemId == MenuItemActions.MoveToFolder)
            {
                StartActivity(CopyMoveToFolderListActivity.CreateIntent(Context, CopyMoveToFolderListActivity.ModeType.Move, ModuleType.Shortcodes, CurrentAdapter.SelectedItems.Select(sp => sp).Cast<IBusinessEntity>().ToList(), Folder));
                ActionMode?.Finish();
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

        public void OnDestroyActionMode(ActionMode mode)
        {
            Activity.Window.AddFlags(WindowManagerFlags.TranslucentStatus);
            Activity.Window.SetStatusBarColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));

            CurrentAdapter.ClearSelections();
            ActionMode = null;
        }

        bool ActionMode.ICallback.OnPrepareActionMode(ActionMode mode, IMenu menu)
        {
            Activity.Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
            Activity.Window.SetStatusBarColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));

            menu.Clear();

            menu.Add(Menu.None, MenuItemActions.CopyToWorktray, MenuItemActions.CopyToWorktray, Resource.String.copy_to_worktray);
            menu.Add(Menu.None, MenuItemActions.CopyToFolder, MenuItemActions.CopyToFolder, Resource.String.copy_to_folder);

            if (Folder.InternalType == FolderInternalType.FilterView || Folder.InternalType == FolderInternalType.Static || Folder.InternalType == FolderInternalType.Worktray)
                menu.Add(Menu.None, MenuItemActions.MoveToFolder, MenuItemActions.MoveToFolder, Resource.String.move_to_folder);

            if (Folder.InternalType == FolderInternalType.FilterView || Folder.InternalType == FolderInternalType.Static || Folder.InternalType == FolderInternalType.Worktray)
                menu.Add(Menu.None, MenuItemActions.DeleteFromFolder, MenuItemActions.DeleteFromFolder, Resource.String.delete_from_folder);

            if (ServerConfig.SystemSettings.ShortcodesModuleInfo.Permissions.DeleteAllowed)
                menu.Add(Menu.None, MenuItemActions.Delete, MenuItemActions.Delete, Resource.String.delete);

            return true;
        }

        async void CopyToWorktrayAction()
        {
            var option = await Dialogs.ShowListDialog(Context, Resource.String.copy_to_worktray, Resource.Array.copy_to_worktray_options, true);

            if (option == 0)
            {
                CommonConfig.Logger.Info($"Attempting copy to worktray [businessEntities.Count={CurrentAdapter.SelectedItemCount}]...");

                var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.copying_to_worktray, Resource.String.please_wait);

                try
                {
                    await Managers.CommonActionsManager.CopyToWorktray(CurrentAdapter.SelectedItems.OfType<IBusinessEntity>().ToList());

                    dismissAction();
                    ActionMode?.Finish();
                }
                catch (Exception ex)
                {
                    dismissAction();

                    CommonConfig.Logger.Error($"Copying to worktray failed [businessEntities.Count={CurrentAdapter.SelectedItemCount}]", ex);

                    await Dialogs.ShowErrorDialogAsync(Activity, ex);
                }
            }

            if (option == 1)
            {
                StartActivity(CopyToUserWorktrayActivity.CreateIntent(Context, CurrentAdapter.SelectedItems.Cast<IBusinessEntity>().ToList()));
            }
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
                ActionMode?.Finish();
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
                ActionMode?.Finish();
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
            public const int CopyToWorktray = 10;
            public const int CopyToFolder = 20;
            public const int MoveToFolder = 21;
            public const int DeleteFromFolder = 30;
            public const int Delete = 31;
        }

        #endregion

        #region Filtering

        bool IMenuItemOnActionExpandListener.OnMenuItemActionExpand(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_filter)
            {
                CommonConfig.UsageAnalytics.LogEvent(new FilterEvent(false, module: ModuleType.Shortcodes));

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

        static bool MatchesQuery(ShortcodePreview sp, string query)
        {
            if (sp.Name?.ContainsCaseInsensitive(query) ?? false)
                return true;

            if (sp.Description?.ContainsCaseInsensitive(query) ?? false)
                return true;

            return false;
        }

        #endregion

        #region Messenger hub related

        public void UpdateShortcodePreview(EntityPreviewChangedMessage m)
        {
            var position = adapter.GetPosition(m.EntityPreview.Id);
            if (position >= 0)
            {
                shouldNotifyAdapter = true;
                adapter.Items[position] = (ShortcodePreview)m.EntityPreview;
            }

            position = searchAdapter.GetPosition(m.EntityPreview.Id);
            if (position >= 0)
            {
                shouldNotifySearchAdapter = true;
                adapter.Items[position] = (ShortcodePreview)m.EntityPreview;
            }
        }

        public void UpdateMovedEntities(EntityMovedFromFolderMessage m)
        {
            foreach (var entityId in m.EntitiesId)
            {
                var position = adapter.GetPosition(entityId);
                if (position >= 0)
                {
                    shouldNotifyAdapter = true;
                    adapter.Items.RemoveAt(position);
                }

                position = searchAdapter.GetPosition(entityId);
                if (position >= 0)
                {
                    shouldNotifySearchAdapter = true;
                    searchAdapter.Items.RemoveAt(position);
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

        #region Adapter callbacks

        protected virtual void Adapter_ItemClicked(object sender, ShortcodePreview shortcodePreview)
        {
        }

        protected virtual void Adapter_ItemLongClicked(object sender, ShortcodePreview shortcodePreview)
        {
        }

        #endregion

        #region RecyclerView Adapter/ViewHolder

        protected class ShortcodesListAdapter : RecyclerView.Adapter, ISectionedAdapter
        {
            public List<ShortcodePreview> Items { get; } = new List<ShortcodePreview>(1000);

            public List<ShortcodePreview> SelectedItems => selectedShortcodesInView.Values.ToList();

            public override int ItemCount => Items.Count;

            public int SelectedItemCount => selectedShortcodesInView.Count;

            readonly Dictionary<int, ShortcodePreview> selectedShortcodesInView = new Dictionary<int, ShortcodePreview>();

            public event EventHandler<ShortcodePreview> ItemClicked = delegate { };
            public event EventHandler<ShortcodePreview> ItemLongClicked = delegate { };

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var cpvh = holder as ShortcodePreviewViewHolder;
                if (cpvh == null)
                    return;

                var cp = Items[position];

                cpvh.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, cp)));
                cpvh.ItemView.SetOnLongClickListener(new ActionOnLongClickListener(() => ItemLongClicked(this, cp)));

                cpvh.Name = cp.Name;

                cpvh.Selected = selectedShortcodesInView.ContainsKey(cp.Id);
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_shortcodes, parent, false);
                return new ShortcodePreviewViewHolder(itemView);
            }

            public void PrependItems(List<ShortcodePreview> items)
            {
                var count = items.Count;
                Items.InsertRange(0, items);
                NotifyItemRangeInserted(0, count);
            }

            public void AppendItems(List<ShortcodePreview> items)
            {
                var count = Items.Count;
                Items.AddRange(items);
                NotifyItemRangeInserted(count, items.Count);
            }

            public void ReplaceItems(List<ShortcodePreview> items)
            {
                Clear();
                AppendItems(items);
            }

            public void RemoveItems(List<ShortcodePreview> items)
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

            public bool IsSelected(ShortcodePreview shortcodePreview)
            {
                return selectedShortcodesInView.ContainsKey(shortcodePreview.Id);
            }

            public void SetSelected(List<ShortcodePreview> shortcodePreviews, bool selected)
            {
                foreach (var shortcode in shortcodePreviews)
                    SetSelected(shortcode, selected);
            }

            public void SetSelected(ShortcodePreview shortcodePreview, bool selected)
            {
                var position = GetPosition(shortcodePreview);
                if (position < 0)
                    return;

                if (selected)
                    selectedShortcodesInView[shortcodePreview.Id] = shortcodePreview;
                else
                    selectedShortcodesInView.Remove(shortcodePreview.Id);
                NotifyItemChanged(position);
            }

            public void ClearSelections()
            {
                var shortcodes = selectedShortcodesInView.Values.ToArray();
                selectedShortcodesInView.Clear();

                foreach (var shortcode in shortcodes)
                {
                    var position = GetPosition(shortcode);
                    if (position >= 0)
                        NotifyItemChanged(position);
                }
            }

            public void Clear()
            {
                var size = Items.Count;
                Items.Clear();
                selectedShortcodesInView.Clear();
                NotifyItemRangeRemoved(0, size);
            }

            public int GetPosition(int shortcodePreviewsId)
            {
                var position = -1;
                for (var i = 0; i < Items.Count; i++)
                    if (Items[i].Id == shortcodePreviewsId)
                    {
                        position = i;
                        break;
                    }

                return position;
            }

            int GetPosition(ShortcodePreview shortcodePreview)
            {
                return GetPosition(shortcodePreview.Id);
            }

            string ISectionedAdapter.GetSectionName(int position)
            {
                return Items[position].Name?.SafeSubstring(0, 1)?.ToUpper() ?? "";
            }
        }

        class ShortcodePreviewViewHolder : RecyclerView.ViewHolder
        {
            public string Name { set => nameTextView.Text = value; }

            public bool Selected { set => selectedOverlay.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }

            readonly AppCompatTextView nameTextView;
            readonly View selectedOverlay;

            public ShortcodePreviewViewHolder(View itemView)
                : base(itemView)
            {
                nameTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_shortcode_name);
                selectedOverlay = itemView.FindViewById<View>(Resource.Id.selected_overlay);
            }
        }

        #endregion
    }
}