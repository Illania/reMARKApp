//
// Project: Mark5.Mobile.Droid
// File: ShortcodesListFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Android.OS;
using Android.Support.V4.Content;
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
using Mark5.Mobile.Droid.Ui.Common.HubMessages;

namespace Mark5.Mobile.Droid.Ui.Fragments
{

    public class ShortcodesListFragment : RetainableStateFragment, ActionMode.ICallback, MenuItemCompat.IOnActionExpandListener, SearchView.IOnQueryTextListener
    {

        public Folder Folder { get; set; }
        public Action CloseRequest { get; set; }

        bool refreshing;

        IMenu menu;
        SwipeRefreshLayout refreshLayout;
        RecyclerView recyclerView;
        ShortcodesListAdapter adapter;
        ShortcodesListAdapter searchAdapter;
        ActionMode actionMode;
        SearchView searchView;

        bool shouldNotifyAdapter;
        bool shouldNotifySearchAdapter;

        ShortcodesListAdapter CurrentAdapter
        {
            get { return (ShortcodesListAdapter)recyclerView.GetAdapter(); }
        }

        CancellationTokenSource cts;

        readonly Handler searchHandler = new Handler();

        #region Fragment overrides

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(ShortcodesListFragment)} [folder.id={Folder?.Id}, folder.name={Folder?.Name}]...");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            var emptyView = rootView.FindViewById<AppCompatTextView>(Resource.Id.empty_view);
            emptyView.SetText(Resource.String.empty_folder);

            refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.SetColorSchemeResources(Resource.Color.blue, Resource.Color.darkerblue);
            refreshLayout.Refresh += (sender, e) =>
            {
                actionMode?.Finish();
                actionMode = null;

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
                if (recyclerView.GetAdapter() != adapter) return;
                if (refreshing) return;

                emptyView.Visibility = adapter.ItemCount < 1 ? ViewStates.Visible : ViewStates.Gone;
                recyclerView.Visibility = adapter.ItemCount > 0 ? ViewStates.Visible : ViewStates.Gone;
                menu?.FindItem(Resource.Id.action_search)?.SetEnabled(adapter.ItemCount > 0);
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

            CommonConfig.Logger.Info($"Created {nameof(ShortcodesListFragment)} [folder.id={Folder?.Id}, folder.name={Folder?.Name}]");
        }

        public override void OnResume()
        {
            base.OnResume();

            CommonConfig.Logger.Info($"Resuming {nameof(ShortcodesListFragment)} [folder.id={Folder?.Id}, folder.name={Folder?.Name}]...");

            if (adapter.ItemCount < 1)
            {
                RefreshData();
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
        }

        public override void OnPause()
        {
            base.OnPause();

            CommonConfig.Logger.Info($"Pausing {nameof(ShortcodesListFragment)} [folder.id={Folder?.Id}, folder.name={Folder?.Name}]...");

            cts?.Cancel();
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            this.menu = menu;

            inflater.Inflate(Resource.Menu.menu_main, menu);

            var searchItem = menu.FindItem(Resource.Id.action_search);
            searchItem.SetIcon(Resource.Drawable.action_search);
            MenuItemCompat.SetOnActionExpandListener(searchItem, this);
            searchView = (SearchView)MenuItemCompat.GetActionView(searchItem);
            searchView.QueryHint = GetString(Resource.String.filter);
            searchView.SetOnQueryTextListener(this);
        }

        #endregion

        #region RetainableStateFragment overrides

        public override IRetainableState OnRetainInstanceState()
        {
            CommonConfig.Logger.Info($"Retaining state [folder.id={Folder?.Id}, folder.name={Folder?.Name}, shortcodePreviews.Count={adapter?.ItemCount}/{adapter?.SelectedItemCount}, refreshing={refreshing}]...");

            return new ShortcodesListFragmentState
            {
                Folder = Folder,
                ShortcodePreviews = adapter.Items,
                SelectedShortcodePreviews = adapter.SelectedItems,
                RefreshInProgress = refreshing
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var dlfs = restoredState as ShortcodesListFragmentState;
            if (dlfs != null)
            {
                CommonConfig.Logger.Info($"Restoring state [dlfs.folder.id={dlfs.Folder.Id}, dlfs.items.count={dlfs.ShortcodePreviews.Count}, dlfs.selectedItems.count={dlfs.SelectedShortcodePreviews.Count}]...");

                Folder = dlfs.Folder;
                adapter.AppendItems(dlfs.ShortcodePreviews);

                if (dlfs.RefreshInProgress)
                {
                    CommonConfig.Logger.Info($"Refresh was in progress before - will continue...");

                    RefreshData(dlfs.ShortcodePreviews[dlfs.ShortcodePreviews.Count - 1].RowId);
                }

                if (dlfs.SelectedShortcodePreviews.Count > 0)
                {
                    actionMode?.Finish();
                    actionMode = Activity.StartActionMode(this);

                    adapter.SetSelected(dlfs.SelectedShortcodePreviews, true);
                    actionMode.Title = adapter.SelectedItemCount.ToString();
                    actionMode.Invalidate();
                }
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(ShortcodesListFragment)} [folder.id={Folder.Id}, folder.name={Folder.Name}]";
        }

        #endregion

        #region Refreshing

        void RefreshData(int startRowId = -1, bool force = false)
        {
            CommonConfig.Logger.Info($"Attempting refresh [startRowId={startRowId}, force={force}]...");

            if (refreshing) return;

            refreshing = true;
            refreshLayout.Refreshing = true;

            CommonConfig.Logger.Info($"Refresh running...");

            cts?.Cancel();
            cts = new CancellationTokenSource();

            if (force)
            {
                adapter.Clear();
            }

            Managers.ShortcodesManager.GetAllShortcodePreviews(Folder, cps =>
            {
                CommonConfig.Logger.Debug($"Retrieved {cps?.Count} contacts");

                Managers.DownloadManager.Notify(ObjectType.Shortcode, Folder.Id);
                Activity.RunOnUiThread(() => adapter.AppendItems(cps));
            }, () =>
            {
                refreshLayout.Refreshing = false;
                refreshing = false;

                CommonConfig.Logger.Info($"Refresh finished");
            }, ex =>
            {
                CommonConfig.Logger.Error($"Downloading shortcodes failed [folder.name={Folder?.Name}, folder.id={Folder?.Id}, startRowId={startRowId}, force={force}]", ex);

                Dialogs.ShowErrorDialog(Activity, ex);

                if (CloseRequest != null && adapter.ItemCount < 1) CloseRequest();
            }, startRowId, cts.Token);
        }

        #endregion

        #region Adapter callbacks

        void Adapter_ItemClicked(object sender, ShortcodePreview shortcodePreview)
        {
            if (actionMode == null)
            {
                var i = new Intent(Activity, typeof(ShortcodeActivity));
                i.PutExtra(ShortcodeActivity.FolderIntentKey, SerializationUtils.Serialize(Folder));
                i.PutExtra(ShortcodeActivity.ShortcodePreviewIntentKey, SerializationUtils.Serialize(shortcodePreview));
                StartActivity(i);
            }
            else
            {
                CurrentAdapter.SetSelected(shortcodePreview, !CurrentAdapter.IsSelected(shortcodePreview));

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

        void Adapter_ItemLongClicked(object sender, ShortcodePreview shortcodePreview)
        {
            if (actionMode == null)
            {
                actionMode = Activity.StartActionMode(this);
            }

            Adapter_ItemClicked(sender, shortcodePreview);
        }

        #endregion

        #region Action mode

        static class MenuItemActions
        {
            public const int CopyToWorktray = 10;
            public const int CopyToFolder = 20;
            public const int MoveToFolder = 21;
            public const int DeleteFromFolder = 30;
            public const int Delete = 31;
        }

        public bool OnCreateActionMode(ActionMode mode, IMenu menu)
        {
            return true;
        }

        bool ActionMode.ICallback.OnPrepareActionMode(ActionMode mode, IMenu menu)
        {
            Activity.Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
            Activity.Window.SetStatusBarColor(new Color(ContextCompat.GetColor(Context, Resource.Color.lightblue)));

            menu.Clear();

            menu.Add(Menu.None, MenuItemActions.CopyToWorktray, MenuItemActions.CopyToWorktray, Resource.String.copy_to_worktray);
            menu.Add(Menu.None, MenuItemActions.CopyToFolder, MenuItemActions.CopyToFolder, Resource.String.copy_to_folder);

            if (Folder.InternalType == FolderInternalType.FilterView
                || Folder.InternalType == FolderInternalType.Static
                || Folder.InternalType == FolderInternalType.Worktray)
            {
                menu.Add(Menu.None, MenuItemActions.MoveToFolder, MenuItemActions.MoveToFolder, Resource.String.move_to_folder);
            }

            if (Folder.InternalType == FolderInternalType.FilterView
                || Folder.InternalType == FolderInternalType.Static
                || Folder.InternalType == FolderInternalType.Worktray)
            {
                menu.Add(Menu.None, MenuItemActions.DeleteFromFolder, MenuItemActions.DeleteFromFolder, Resource.String.delete_from_folder);
            }

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator
                || ServerConfig.SystemSettings.ShortcodesModuleInfo.Permissions.DeleteAllowed)
            {
                menu.Add(Menu.None, MenuItemActions.Delete, MenuItemActions.Delete, Resource.String.delete);
            }

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
                var i = new Intent(Activity, typeof(CopyMoveToFolderListActivity));
                i.PutExtra(CopyMoveToFolderListActivity.ModeIntentKey, (int)CopyMoveToFolderListActivity.ModeType.Copy);
                i.PutExtra(CopyMoveToFolderListActivity.ModuleIntentKey, SerializationUtils.Serialize(ModuleType.Shortcodes));
                i.PutExtra(CopyMoveToFolderListActivity.BusinessEntitiesIntentKey, SerializationUtils.Serialize(CurrentAdapter.SelectedItems.Select(sp => sp).Cast<IBusinessEntity>().ToList()));
                StartActivity(i);

                actionMode?.Finish();
                return true;
            }

            if (item.ItemId == MenuItemActions.MoveToFolder)
            {
                var i = new Intent(Activity, typeof(CopyMoveToFolderListActivity));
                i.PutExtra(CopyMoveToFolderListActivity.ModeIntentKey, (int)CopyMoveToFolderListActivity.ModeType.Move);
                i.PutExtra(CopyMoveToFolderListActivity.ModuleIntentKey, SerializationUtils.Serialize(ModuleType.Shortcodes));
                i.PutExtra(CopyMoveToFolderListActivity.BusinessEntitiesIntentKey, SerializationUtils.Serialize(CurrentAdapter.SelectedItems.Select(sp => sp).Cast<IBusinessEntity>().ToList()));
                i.PutExtra(CopyMoveToFolderListActivity.FromFolderIntentKey, SerializationUtils.Serialize(Folder));
                StartActivity(i);

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

        public void OnDestroyActionMode(ActionMode mode)
        {
            Activity.Window.AddFlags(WindowManagerFlags.TranslucentStatus);
            Activity.Window.SetStatusBarColor(new Color(ContextCompat.GetColor(Context, Resource.Color.lightblue)));

            CurrentAdapter.ClearSelections();
            actionMode = null;
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
                    actionMode?.Finish();
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
                StartActivity(CopyToUserWorktrayActivity.CreateIntent(Activity, CurrentAdapter.SelectedItems.Cast<IBusinessEntity>().ToList()));
            }
        }

        async void DeleteFromFolderAction()
        {
            var yesNo = await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.delete_from_folder, Resource.String.delete_from_folder_are_you_sure);
            if (!yesNo)
            {
                return;
            }

            CommonConfig.Logger.Info($"Attempting to delete from folder [businessEntities.Count={CurrentAdapter.SelectedItemCount}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.deleting_from_folder, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.RemoveFromFolder(CurrentAdapter.SelectedItems.OfType<IBusinessEntity>().ToList(), Folder);
                adapter.RemoveItems(CurrentAdapter.SelectedItems);
                searchAdapter.RemoveItems(CurrentAdapter.SelectedItems);

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
            {
                return;
            }

            CommonConfig.Logger.Info($"Attempting to delete [businessEntities.Count={CurrentAdapter.SelectedItemCount}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.deleting, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.Delete(CurrentAdapter.SelectedItems.OfType<IBusinessEntity>().ToList());
                adapter.RemoveItems(CurrentAdapter.SelectedItems);
                searchAdapter.RemoveItems(CurrentAdapter.SelectedItems);

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

        static bool MatchesQuery(ShortcodePreview cp, string query)
        {
            if (cp.Name.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                return true;
            }
            if (cp.Description.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region State

        class ShortcodesListFragmentState : IRetainableState
        {

            public Folder Folder { get; set; }

            public List<ShortcodePreview> ShortcodePreviews { get; set; }

            public List<ShortcodePreview> SelectedShortcodePreviews { get; set; }

            public bool RefreshInProgress { get; set; }
        }

        #endregion

        #region Messenger hub related

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
                    adapter.Items.RemoveAt(position);
                }

                position = searchAdapter.GetPosition(entityId);
                if (position >= 0)
                {
                    shouldNotifySearchAdapter = true;
                    adapter.Items.RemoveAt(position);
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
                    adapter.Items.RemoveAt(position);
                }

                position = searchAdapter.GetPosition(entityId);
                if (position >= 0)
                {
                    shouldNotifySearchAdapter = true;
                    adapter.Items.RemoveAt(position);
                }
            }
        }

        #endregion

        #region RecyclerView Adapter/ViewHolder

        class ShortcodesListAdapter : RecyclerView.Adapter
        {

            public List<ShortcodePreview> Items
            {
                get
                {
                    return shortcodePreviewsInView;
                }
            }

            public List<ShortcodePreview> SelectedItems
            {
                get
                {
                    return selectedShortcodesInView.Values.ToList();
                }
            }

            public override int ItemCount
            {
                get
                {
                    return shortcodePreviewsInView.Count;
                }
            }

            public int SelectedItemCount
            {
                get
                {
                    return selectedShortcodesInView.Count;
                }
            }

            readonly List<ShortcodePreview> shortcodePreviewsInView = new List<ShortcodePreview>(1000);
            readonly Dictionary<int, ShortcodePreview> selectedShortcodesInView = new Dictionary<int, ShortcodePreview>();

            public event EventHandler<ShortcodePreview> ItemClicked = delegate { };
            public event EventHandler<ShortcodePreview> ItemLongClicked = delegate { };

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var cpvh = holder as ShortcodePreviewViewHolder;
                if (cpvh == null) return;

                var cp = shortcodePreviewsInView[position];

                cpvh.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, cp)));
                cpvh.ItemView.SetOnLongClickListener(new ActionOnLongClickListener(() => ItemLongClicked(this, cp)));

                cpvh.Name = cp.Name;
                cpvh.Description = cp.Description;

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
                shortcodePreviewsInView.InsertRange(0, items);
                NotifyItemRangeInserted(0, count);
            }

            public void AppendItems(List<ShortcodePreview> items)
            {
                var count = shortcodePreviewsInView.Count;
                shortcodePreviewsInView.AddRange(items);
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
                        shortcodePreviewsInView.RemoveAt(position);
                        NotifyItemRemoved(position);
                    }
                }
            }

            public bool IsSelected(ShortcodePreview shortcodePreview)
            {
                return selectedShortcodesInView.ContainsKey(shortcodePreview.Id);
            }

            public void SetSelected(List<ShortcodePreview> shortcodePreviews, bool selected)
            {
                foreach (var shortcode in shortcodePreviews)
                {
                    SetSelected(shortcode, selected);
                }
            }

            public void SetSelected(ShortcodePreview shortcodePreview, bool selected)
            {
                var position = GetPosition(shortcodePreview);
                if (position < 0) return;

                if (selected)
                {
                    selectedShortcodesInView[shortcodePreview.Id] = shortcodePreview;
                }
                else
                {
                    selectedShortcodesInView.Remove(shortcodePreview.Id);
                }
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
                    {
                        NotifyItemChanged(position);
                    }
                }
            }

            public void Clear()
            {
                var size = shortcodePreviewsInView.Count;
                shortcodePreviewsInView.Clear();
                selectedShortcodesInView.Clear();
                NotifyItemRangeRemoved(0, size);
            }

            int GetPosition(ShortcodePreview shortcodePreview)
            {
                return GetPosition(shortcodePreview.Id);
            }

            public int GetPosition(int shortcodePreviewsId)
            {
                var position = -1;
                for (var i = 0; i < shortcodePreviewsInView.Count; i++)
                {
                    if (shortcodePreviewsInView[i].Id == shortcodePreviewsId)
                    {
                        position = i;
                        break;
                    }
                }
                return position;
            }
        }

        class ShortcodePreviewViewHolder : RecyclerView.ViewHolder
        {

            static readonly int[] colors = { Resource.Color.darkerblue, Resource.Color.darkblue, Resource.Color.blue };

            public string Name
            {
                set
                {
                    nameTextView.Text = value;
                    letterTextView.Text = value.SafeSubstring(0, 1).ToUpper();

                    var sd = new ShapeDrawable(new OvalShape());
                    sd.Paint.Color = new Color(ContextCompat.GetColor(ItemView.Context, colors[Math.Abs(value.GetHashCode() % colors.Length)]));
                    letterTextView.Background = sd;
                }
            }

            public string Description
            {
                set
                {
                    descTextView.Text = value;
                    descTextView.Visibility = string.IsNullOrWhiteSpace(value) ? ViewStates.Gone : ViewStates.Visible;
                }
            }

            public bool Selected
            {
                set
                {
                    selectedOverlay.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
                }
            }

            readonly AppCompatTextView letterTextView;
            readonly AppCompatTextView nameTextView;
            readonly AppCompatTextView descTextView;
            readonly View selectedOverlay;

            public ShortcodePreviewViewHolder(View itemView)
                    : base(itemView)
            {
                letterTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_shortcode_letter);
                nameTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_shortcode_name);
                descTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_shortcode_desc);
                selectedOverlay = itemView.FindViewById<View>(Resource.Id.selected_overlay);
            }
        }

        #endregion
    }
}

