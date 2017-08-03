using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using FastScrollRecycler;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Model.HubMessages;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public abstract class AbstractContactsListFragment : RetainableStateFragment, ActionMode.ICallback, MenuItemCompat.IOnActionExpandListener, SearchView.IOnQueryTextListener
    {
        static class RequestCodes
        {
            public const int SaveOfflineRequest = 1;
        }

        public Folder Folder { get; set; }
        public Action CloseRequest { get; set; }

        bool refreshing;

        IMenu menu;
        SwipeRefreshLayout refreshLayout;
        RecyclerView recyclerView;
        ContactsListAdapter adapter;
        ContactsListAdapter searchAdapter;
        protected ActionMode ActionMode;
        SearchView searchView;

        bool shouldNotifyAdapter;
        bool shouldNotifySearchAdapter;

        protected ContactsListAdapter CurrentAdapter => (ContactsListAdapter)recyclerView.GetAdapter();

        CancellationTokenSource cts;

        readonly Handler searchHandler = new Handler();

        #region Fragment overrides

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(ContactsListFragment)} [folder.id={Folder?.Id}, folder.name={Folder?.Name}]...");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            var emptyView = rootView.FindViewById<AppCompatTextView>(Resource.Id.empty_view);
            emptyView.SetText(Resource.String.empty_folder);

            refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.SetColorSchemeResources(Resource.Color.blue, Resource.Color.darkerblue);
            refreshLayout.Refresh += (sender, e) =>
            {
                ActionMode?.Finish();
                ActionMode = null;

                RefreshData(force: true);
            };

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));
            recyclerView.HasFixedSize = true;

            adapter = new ContactsListAdapter();
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

            searchAdapter = new ContactsListAdapter();
            searchAdapter.ItemClicked += Adapter_ItemClicked;
            searchAdapter.ItemLongClicked += Adapter_ItemLongClicked;

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.contacts);
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = Folder?.Name;

            CommonConfig.Logger.Info($"Created {nameof(ContactsListFragment)} [folder.id={Folder?.Id}, folder.name={Folder?.Name}]");
        }

        public override void OnResume()
        {
            base.OnResume();

            CommonConfig.Logger.Info($"Resuming {nameof(ContactsListFragment)} [folder.id={Folder?.Id}, folder.name={Folder?.Name}]...");

            if (adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"No elements - will refresh...");

                RefreshData();
            }

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

            CommonConfig.Logger.Info($"Pausing {nameof(ContactsListFragment)} [folder.id={Folder?.Id}, folder.name={Folder?.Name}]...");

            cts?.Cancel();
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            this.menu = menu;

            inflater.Inflate(Resource.Menu.menu_main, menu);

            var filterItem = menu.FindItem(Resource.Id.action_filter);
            MenuItemCompat.SetOnActionExpandListener(filterItem, this);
            searchView = (SearchView)MenuItemCompat.GetActionView(filterItem);
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
                var i = new Intent(Activity, typeof(SearchActivity));
                i.PutExtra(SearchActivity.ModuleIntentKey, Serializer.Serialize(ModuleType.Contacts));
                StartActivity(i);

                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region RetainableStateFragment overrides

        public override IRetainableState OnRetainInstanceState()
        {
            CommonConfig.Logger.Info($"Retaining state [folder.id={Folder?.Id}, folder.name={Folder?.Name}, contactPreviews.Count={adapter?.ItemCount}/{adapter?.SelectedItemCount}, refreshing={refreshing}]...");

            return new ContactsListFragmentState
            {
                Folder = Folder,
                ContactPreviews = adapter.Items,
                SelectedContactPreviews = adapter.SelectedItems,
                RefreshInProgress = refreshing
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var dlfs = restoredState as ContactsListFragmentState;
            if (dlfs != null)
            {
                CommonConfig.Logger.Info($"Restoring state [dlfs.folder.id={dlfs.Folder?.Id}, dlfs.items.count={dlfs.ContactPreviews?.Count}, dlfs.selectedItems.count={dlfs.SelectedContactPreviews?.Count}]...");

                Folder = dlfs.Folder;
                adapter.AppendItems(dlfs.ContactPreviews);

                if (dlfs.RefreshInProgress)
                {
                    CommonConfig.Logger.Info("Refresh was in progress before - will continue...");

                    RefreshData(dlfs.ContactPreviews[dlfs.ContactPreviews.Count - 1].RowId);
                }

                if (dlfs.SelectedContactPreviews.Count > 0)
                {
                    ActionMode?.Finish();
                    ActionMode = Activity.StartActionMode(this);

                    adapter.SetSelected(dlfs.SelectedContactPreviews, true);
                    ActionMode.Title = adapter.SelectedItemCount.ToString();
                    ActionMode.Invalidate();
                }
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(ContactsListFragment)} [folder.id={Folder.Id}, folder.name={Folder.Name}]";
        }

        #endregion

        #region Refreshing

        async void RefreshData(int startRowId = -1, bool force = false, bool skipOfflineCheck = false)
        {
            CommonConfig.Logger.Info($"Attempting refresh [startRowId={startRowId}, force={force}]...");

            if (refreshing)
                return;

            refreshing = true;
            refreshLayout.Refreshing = true;

            if (force && !skipOfflineCheck && await Managers.FoldersManager.IsSavedFolderOfflineInfo(Folder))
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
                    var i = new Intent(Activity, typeof(DownloadActivity));
                    i.PutExtra(DownloadActivity.FolderIntentKey, Serializer.Serialize(Folder));
                    StartActivityForResult(i, RequestCodes.SaveOfflineRequest);

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

            cts?.Cancel();
            cts = new CancellationTokenSource();

            if (force)
                adapter.Clear();

            var sourceType = await Managers.FoldersManager.IsSavedFolderOfflineInfo(Folder) ? SourceType.Local : SourceType.Auto;

            Managers.ContactsManager.GetAllContactPreviews(Folder,
                cps =>
                {
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
                    CommonConfig.Logger.Error($"Downloading contacts failed [folder.name={Folder?.Name}, folder.id={Folder?.Id}, startRowId={startRowId}, force={force}]", ex);

                    Dialogs.ShowErrorDialog(Activity, ex);

                    if (CloseRequest != null && adapter.ItemCount < 1)
                        CloseRequest();
                },
                startRowId,
                cts.Token,
                sourceType);
        }

        #endregion

        #region Adapter callbacks

        protected virtual void Adapter_ItemClicked(object sender, ContactPreview contactPreview)
        {
        }

        protected virtual void Adapter_ItemLongClicked(object sender, ContactPreview contactPreview)
        {
        }

        #endregion

        #region Action mode

        static class MenuItemActions
        {
            public const int CopyToWorktray = 30;
            public const int CopyToFolder = 40;
            public const int MoveToFolder = 41;
            public const int Categories = 50;
            public const int DeleteFromFolder = 70;
            public const int Delete = 71;
        }

        public bool OnCreateActionMode(ActionMode mode, IMenu menu)
        {
            return true;
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

            if (CurrentAdapter.SelectedItemCount == 1)
                menu.Add(Menu.None, MenuItemActions.Categories, MenuItemActions.Categories, Resource.String.categories);

            if (Folder.InternalType == FolderInternalType.FilterView || Folder.InternalType == FolderInternalType.Static || Folder.InternalType == FolderInternalType.Worktray)
                menu.Add(Menu.None, MenuItemActions.DeleteFromFolder, MenuItemActions.DeleteFromFolder, Resource.String.delete_from_folder);

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator || ServerConfig.SystemSettings.ContactsModuleInfo.Permissions.DeleteAllowed)
                menu.Add(Menu.None, MenuItemActions.Delete, MenuItemActions.Delete, Resource.String.delete);

            return true;
        }

        bool ActionMode.ICallback.OnActionItemClicked(ActionMode mode, IMenuItem item)
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
                i.PutExtra(CopyMoveToFolderListActivity.ModuleIntentKey, Serializer.Serialize(ModuleType.Contacts));
                i.PutExtra(CopyMoveToFolderListActivity.BusinessEntitiesIntentKey, Serializer.Serialize(CurrentAdapter.SelectedItems.Select(sp => sp).Cast<IBusinessEntity>().ToList()));
                StartActivity(i);

                ActionMode?.Finish();
                return true;
            }

            if (item.ItemId == MenuItemActions.MoveToFolder)
            {
                var i = new Intent(Activity, typeof(CopyMoveToFolderListActivity));
                i.PutExtra(CopyMoveToFolderListActivity.ModeIntentKey, (int)CopyMoveToFolderListActivity.ModeType.Move);
                i.PutExtra(CopyMoveToFolderListActivity.ModuleIntentKey, Serializer.Serialize(ModuleType.Contacts));
                i.PutExtra(CopyMoveToFolderListActivity.BusinessEntitiesIntentKey, Serializer.Serialize(CurrentAdapter.SelectedItems.Select(sp => sp).Cast<IBusinessEntity>().ToList()));
                i.PutExtra(CopyMoveToFolderListActivity.FromFolderIntentKey, Serializer.Serialize(Folder));
                StartActivity(i);

                ActionMode?.Finish();
                return true;
            }

            if (item.ItemId == MenuItemActions.Categories)
            {
                StartActivity(CategoriesListActivity.CreateIntent(Context,CurrentAdapter.SelectedItems.First()));
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

        void ActionMode.ICallback.OnDestroyActionMode(ActionMode mode)
        {
            Activity.Window.AddFlags(WindowManagerFlags.TranslucentStatus);
            Activity.Window.SetStatusBarColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));

            CurrentAdapter.ClearSelections();
            ActionMode = null;
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
                var i = new Intent(Activity, typeof(CopyToUserWorktrayActivity));
                i.PutExtra(CopyToUserWorktrayActivity.BusinessEntitiesIntentKey, Serializer.Serialize(CurrentAdapter.SelectedItems.Cast<IBusinessEntity>().ToList()));
                StartActivity(i);
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
                adapter.RemoveItems(CurrentAdapter.SelectedItems);
                searchAdapter.RemoveItems(CurrentAdapter.SelectedItems);

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
                adapter.RemoveItems(CurrentAdapter.SelectedItems);
                searchAdapter.RemoveItems(CurrentAdapter.SelectedItems);

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

        #endregion

        #region Filtering

        bool MenuItemCompat.IOnActionExpandListener.OnMenuItemActionExpand(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_filter)
            {
                menu?.FindItem(10)?.SetVisible(false);

                refreshLayout.Enabled = false;
                adapter.ClearSelections();
                recyclerView.SwapAdapter(searchAdapter, true);
                (this as SearchView.IOnQueryTextListener).OnQueryTextChange(string.Empty);
                return true;
            }

            return false;
        }

        bool MenuItemCompat.IOnActionExpandListener.OnMenuItemActionCollapse(IMenuItem item)
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

        static bool MatchesQuery(ContactPreview cp, string query)
        {
            if (cp.Name?.ContainsCaseInsensitive(query) ?? false)
                return true;

            if (cp.CompanyName?.ContainsCaseInsensitive(query) ?? false)
                return true;

            if (cp.ShortId?.ContainsCaseInsensitive(query) ?? false)
                return true;

            if (cp.Description?.ContainsCaseInsensitive(query) ?? false)
                return true;

            if (cp.PrimaryAddress?.Address?.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0)
                return true;

            if (cp.Categories.Any(da => da.Name?.ContainsCaseInsensitive(query) ?? false))
                return true;

            return false;
        }

        #endregion

        #region State

        class ContactsListFragmentState : IRetainableState
        {
            public Folder Folder { get; set; }

            public List<ContactPreview> ContactPreviews { get; set; }

            public List<ContactPreview> SelectedContactPreviews { get; set; }

            public bool RefreshInProgress { get; set; }
        }

        #endregion

        #region Messenger hub related

        public void UpdateCategories(ContactPreviewCategoriesChangedMessage m)
        {
            var position = adapter.GetPosition(m.ContactPreviewId);
            if (position >= 0)
            {
                shouldNotifyAdapter = true;
                var cp = adapter.Items[position];
                cp.Categories.Clear();
                cp.Categories.AddRange(m.Categories);
            }

            position = searchAdapter.GetPosition(m.ContactPreviewId);
            if (position >= 0)
            {
                shouldNotifySearchAdapter = true;
                var cp = searchAdapter.Items[position];
                cp.Categories.Clear();
                cp.Categories.AddRange(m.Categories);
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

        protected class ContactsListAdapter : RecyclerView.Adapter, ISectionedAdapter
        {
            public List<ContactPreview> Items { get; } = new List<ContactPreview>(1000);

            public List<ContactPreview> SelectedItems => selectedContactsInView.Values.ToList();

            public override int ItemCount => Items.Count;

            public int SelectedItemCount => selectedContactsInView.Count;

            readonly Dictionary<int, ContactPreview> selectedContactsInView = new Dictionary<int, ContactPreview>();

            public event EventHandler<ContactPreview> ItemClicked = delegate { };
            public event EventHandler<ContactPreview> ItemLongClicked = delegate { };

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var cpvh = holder as ContactPreviewViewHolder;
                if (cpvh == null)
                    return;

                var cp = Items[position];

                cpvh.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, cp)));
                cpvh.ItemView.SetOnLongClickListener(new ActionOnLongClickListener(() => ItemLongClicked(this, cp)));

                cpvh.Type = cp.Type;
                cpvh.Name = cp.Name;
                cpvh.Categories = cp.Categories;

                cpvh.Selected = selectedContactsInView.ContainsKey(cp.Id);
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_contacts, parent, false);
                return new ContactPreviewViewHolder(itemView);
            }

            public void PrependItems(List<ContactPreview> items)
            {
                var count = items.Count;
                Items.InsertRange(0, items);
                NotifyItemRangeInserted(0, count);
            }

            public void AppendItems(List<ContactPreview> items)
            {
                var count = Items.Count;
                Items.AddRange(items);
                NotifyItemRangeInserted(count, items.Count);
            }

            public void ReplaceItems(List<ContactPreview> items)
            {
                Clear();
                AppendItems(items);
            }

            public void RemoveItems(List<ContactPreview> items)
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

            public bool IsSelected(ContactPreview contactPreview)
            {
                return selectedContactsInView.ContainsKey(contactPreview.Id);
            }

            public void SetSelected(List<ContactPreview> contactPreviews, bool selected)
            {
                foreach (var contact in contactPreviews)
                    SetSelected(contact, selected);
            }

            public void SetSelected(ContactPreview contactPreview, bool selected)
            {
                var position = GetPosition(contactPreview);
                if (position < 0)
                    return;

                if (selected)
                    selectedContactsInView[contactPreview.Id] = contactPreview;
                else
                    selectedContactsInView.Remove(contactPreview.Id);
                NotifyItemChanged(position);
            }

            public void ClearSelections()
            {
                var contacts = selectedContactsInView.Values.ToArray();
                selectedContactsInView.Clear();

                foreach (var contact in contacts)
                {
                    var position = GetPosition(contact);
                    if (position >= 0)
                        NotifyItemChanged(position);
                }
            }

            public void Clear()
            {
                var size = Items.Count;
                Items.Clear();
                selectedContactsInView.Clear();
                NotifyItemRangeRemoved(0, size);
            }

            public int GetPosition(int contactPreviewId)
            {
                var position = -1;
                for (var i = 0; i < Items.Count; i++)
                    if (Items[i].Id == contactPreviewId)
                    {
                        position = i;
                        break;
                    }

                return position;
            }

            public int GetPosition(ContactPreview contactPreview)
            {
                var position = -1;
                for (var i = 0; i < Items.Count; i++)
                    if (Items[i].Id == contactPreview.Id)
                    {
                        position = i;
                        break;
                    }

                return position;
            }

            string ISectionedAdapter.GetSectionName(int position)
            {
                return Items[position].Name?.SafeSubstring(0, 1)?.ToUpper() ?? "";
            }
        }

        class ContactPreviewViewHolder : RecyclerView.ViewHolder
        {
            public ContactType Type
            {
                set
                {
                    switch (value)
                    {
                        case ContactType.Person:
                            iconImageView.SetImageResource(Resource.Drawable.large_person);
                            break;
                        case ContactType.Department:
                            iconImageView.SetImageResource(Resource.Drawable.large_department);
                            break;
                        case ContactType.Company:
                            iconImageView.SetImageResource(Resource.Drawable.large_company);
                            break;
                        default:
                            iconImageView.SetImageDrawable(null);
                            break;
                    }
                }
            }

            public string Name { set => nameTextView.Text = value; }

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

            public bool Selected { set => selectedOverlay.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }

            readonly AppCompatImageView iconImageView;
            readonly AppCompatTextView nameTextView;
            readonly LinearLayoutCompat categoriesLayout;
            readonly View selectedOverlay;

            public ContactPreviewViewHolder(View itemView)
                : base(itemView)
            {
                iconImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_contact_icon);
                nameTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_contact_name);
                categoriesLayout = itemView.FindViewById<LinearLayoutCompat>(Resource.Id.list_item_contact_categories);
                selectedOverlay = itemView.FindViewById<View>(Resource.Id.selected_overlay);
            }
        }

        #endregion
    }
}