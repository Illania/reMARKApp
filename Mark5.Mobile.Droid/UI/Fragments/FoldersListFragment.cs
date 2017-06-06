//
// Project: Mark5.Mobile.Droid
// File: FoldersListFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.DataAccess.Exceptions;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class FoldersListFragment : RetainableStateFragment, ActionMode.ICallback, MenuItemCompat.IOnActionExpandListener, SearchView.IOnQueryTextListener
    {
        public Folder RemoteFolder { get; set; }

        protected View Container;
        protected FolderListAdapter Adapter;
        protected SearchFolderListAdapter SearchAdapter;
        protected SearchView SearchView;
        protected RecyclerView RecyclerView;
        protected SwipeRefreshLayout RefreshLayout;
        protected List<Section> AvailableSections;
        protected bool SearchEnabled;
        protected readonly Handler SearchHandler = new Handler();

        IMenu menu;
        ActionMode actionMode;
        FloatingActionButton fab;
        List<int> recoveredSelectedItemsPosition;

        protected virtual bool LoadRemoteFromCache { get; }

        protected FolderListAdapter CurrentAdapter
        {
            get { return SearchEnabled ? SearchAdapter : Adapter; }
        }

        #region Overrides

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            Container = container;

            CommonConfig.Logger.Info($"Creating {nameof(FoldersListFragment)} [folder.id={RemoteFolder?.Id}, folder.name={RemoteFolder?.Name}]...");

            var rootView = InflateView(inflater, container);

            var emptyView = rootView.FindViewById<AppCompatTextView>(Resource.Id.empty_view);
            emptyView.SetText(Resource.String.empty_folder);

            RefreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            RefreshLayout.SetColorSchemeResources(Resource.Color.blue, Resource.Color.darkerblue);
            RefreshLayout.Refresh += RefreshLayout_Refresh;

            RecyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            RecyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            RecyclerView.SetItemAnimator(new DefaultItemAnimator());
            RecyclerView.HasFixedSize = true;

            Adapter = new FolderListAdapter(Context, RecyclerView);
            Adapter.RegisterAdapterDataObserver(new LambdaEmptyAdapterObserver(() =>
            {
                if (RecyclerView.GetAdapter() != Adapter)
                    return;

                emptyView.Visibility = Adapter.ItemCount < 1 ? ViewStates.Visible : ViewStates.Gone;
                RecyclerView.Visibility = Adapter.ItemCount > 0 ? ViewStates.Visible : ViewStates.Gone;
                menu?.FindItem(Resource.Id.action_filter)?.SetEnabled(Adapter.ItemCount > 0);
            }));
            Adapter.ExpandIconClicked += Adapter_ExpandClicked;
            Adapter.ItemClicked += Adapter_ItemClicked;
            Adapter.ItemLongClicked += Adapter_ItemLongClicked;

            SearchAdapter = new SearchFolderListAdapter(Context, RecyclerView);
            SearchAdapter.ItemClicked += Adapter_ItemClicked;
            SearchAdapter.ItemLongClicked += Adapter_ItemLongClicked;

            RecyclerView.SetAdapter(Adapter);

            HasOptionsMenu = true;

            return rootView;
        }

        protected virtual View InflateView(LayoutInflater inflater, ViewGroup container)
        {
            return inflater.Inflate(Resource.Layout.list, container, false);
            ;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            if (!(view.Parent is ViewPager))
            {
                var title = string.Empty;

                switch (RemoteFolder.Module)
                {
                    case ModuleType.Documents:
                        title = GetString(Resource.String.documents);
                        break;
                    case ModuleType.Contacts:
                        title = GetString(Resource.String.contacts);
                        break;
                    case ModuleType.Shortcodes:
                        title = GetString(Resource.String.shortcodes);
                        break;
                }

                ((AppCompatActivity) Activity).SupportActionBar.Title = title;
                ((AppCompatActivity) Activity).SupportActionBar.Subtitle = RemoteFolder.Root ? null : RemoteFolder.Name;
            }

            CommonConfig.Logger.Info($"Created {nameof(FoldersListFragment)} [folder.id={RemoteFolder?.Id}, folder.name={RemoteFolder?.Name}]");
        }

        public override void OnResume()
        {
            base.OnResume();

            CommonConfig.Logger.Info($"Resuming {nameof(FoldersListFragment)} [folder.id={RemoteFolder?.Id}, folder.name={RemoteFolder?.Name}]...");

            fab = ((View) Container.Parent.Parent.Parent.Parent).FindViewById<FloatingActionButton>(Resource.Id.fab);
            if (RemoteFolder?.Module == ModuleType.Documents)
            {
                fab.SetImageResource(Resource.Drawable.action_new);
                fab.SetOnClickListener(new ActionOnClickListener(ComposeDocument));
                fab.Visibility = ViewStates.Visible;
            }
            else
            {
                fab.Visibility = ViewStates.Gone;
                fab = null;
            }

            SetSections();
            RefreshData();
            RestoreSelection();
        }

        public override void OnPause()
        {
            base.OnPause();

            CommonConfig.Logger.Info($"Pausing {nameof(FoldersListFragment)} [folder.id={RemoteFolder?.Id}, folder.name={RemoteFolder?.Name}]...");

            actionMode?.Finish();
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            this.menu = menu;

            inflater.Inflate(Resource.Menu.menu_main, menu);

            var filterItem = menu.FindItem(Resource.Id.action_filter);
            MenuItemCompat.SetOnActionExpandListener(filterItem, this);
            SearchView = (SearchView) MenuItemCompat.GetActionView(filterItem);
            SearchView.QueryHint = GetString(Resource.String.filter);
            SearchView.SetOnQueryTextListener(this);

            var searchItem = menu.Add(Menu.None, 10, Menu.None, Resource.String.search);
            searchItem.SetIcon(Resource.Drawable.action_search_server);
            searchItem.SetShowAsAction(ShowAsAction.Always);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == 10)
            {
                var i = new Intent(Activity, typeof(SearchActivity));
                i.PutExtra(SearchActivity.ModuleIntentKey, SerializationUtils.Serialize(RemoteFolder.Module));

                StartActivity(i);

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

            StartActivity(ComposeDocumentActivity.CreateIntent(Context, DocumentCreationModeFlag.New, DocumentDirection.None));
        }

        #endregion

        #region Utility methods

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void RefreshData(bool forceRefresh = false)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            CommonConfig.Logger.Info($"Refreshing...");

            if (!RemoteFolder.HasSubFolders)
                return;

            RefreshLayout.Post(() => RefreshLayout.Refreshing = true);

            await Task.Delay(300); // Let the animation finish

            if (AvailableSections.Contains(Section.Remote))
                await RefreshRemote(forceRefresh);

            if (AvailableSections.Contains(Section.Favourites))
                await RefreshFavorites();

            if (AvailableSections.Contains(Section.Local))
                RefreshLocal();

            RefreshLayout.Post(() => RefreshLayout.Refreshing = false);
        }

        async Task RefreshRemote(bool forceRefresh = false)
        {
            CommonConfig.Logger.Info($"Refreshing remote folders...");

            if (forceRefresh || !RemoteFolder.SubFolders.Any())
            {
                List<Folder> remoteFolders = null;

                if (LoadRemoteFromCache)
                {
                    try
                    {
                        remoteFolders = await Managers.FoldersManager.GetFoldersAsync(RemoteFolder, sourceType: SourceType.Local);
                    }
                    catch (DataNotFoundException)
                    {
                        // Do nothing
                    }
                    catch (Exception ex)
                    {
                        CommonConfig.Logger.Error($"Retrieving folders from cache only failed [folder.name={RemoteFolder.Name}, folder.id={RemoteFolder.Id}]", ex);
                    }
                }

                if (remoteFolders == null)
                {
                    try
                    {
                        remoteFolders = await Managers.FoldersManager.GetFoldersAsync(RemoteFolder);
                    }
                    catch (Exception ex)
                    {
                        CommonConfig.Logger.Error($"Downloading folders failed [folder.name={RemoteFolder.Name}, folder.id={RemoteFolder.Id}]", ex);
                        await Dialogs.ShowErrorDialogAsync(Activity, ex);
                    }
                }

                if (remoteFolders != null)
                    Adapter.Refresh(remoteFolders, Section.Remote);
            }
            else
            {
                CommonConfig.Logger.Info($"Folders already downloaded, refreshing views...");

                Adapter.Refresh(RemoteFolder.SubFolders, Section.Remote);
            }
        }

        async Task RefreshFavorites()
        {
            CommonConfig.Logger.Info($"Refreshing favourite folders...");

            var favouriteFolders = await Managers.FoldersManager.GetFavoriteFoldersAsync(RemoteFolder.Module);
            Adapter.Refresh(favouriteFolders, Section.Favourites);
        }

        void RefreshLocal()
        {
            CommonConfig.Logger.Info($"Refreshing local folders...");

            var localRootFolder = Folder.LocalRootForModule(RemoteFolder.Module);
            Adapter.Refresh(localRootFolder.SubFolders, Section.Local);
        }

        protected virtual void RestoreSelection()
        {
            CommonConfig.Logger.Info("Restoring selected items");

            if (recoveredSelectedItemsPosition != null && recoveredSelectedItemsPosition.Any())
            {
                actionMode = Activity.StartActionMode(this);
                Adapter.SetSelection(recoveredSelectedItemsPosition);
                actionMode.Title = Adapter.SelectedItemsCount.ToString();
                actionMode.Invalidate();
            }
        }

        protected virtual void SetSections()
        {
            CommonConfig.Logger.Info("Setting sections according to the folder");

            if (RemoteFolder.Root)
            {
                AvailableSections = new List<Section>
                {
                    Section.Favourites,
                    Section.Remote
                };
                if (RemoteFolder.Module == ModuleType.Documents)
                {
                    AvailableSections.Add(Section.Local);
                }
            }
            else
            {
                AvailableSections = new List<Section>
                {
                    Section.Remote
                };
            }

            Adapter.SetSections(AvailableSections);
        }

        void NavigateToFolder(Folder folder)
        {
            var fragmentManager = ((AppCompatActivity) Activity).SupportFragmentManager;
            var foldersListFragment = GetFolderFragment(folder);
            var tag = foldersListFragment.GenerateTag();

            fragmentManager.BeginTransaction().SetCustomAnimations(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left, Resource.Animation.enter_from_left, Resource.Animation.exit_to_right).Replace(Resource.Id.fragment_container, foldersListFragment, tag).AddToBackStack(tag).Commit();
        }

        protected virtual RetainableStateFragment GetFolderFragment(Folder folder)
        {
            return new FoldersListFragment
            {
                RemoteFolder = folder,
            };
        }

        #endregion

        #region List item event handlers

        void Adapter_ExpandClicked(object sender, int position)
        {
            NavigateToFolder(CurrentAdapter.GetItemAtPosition(position));
        }

        protected virtual void Adapter_ItemClicked(object sender, int position)
        {
            if (actionMode == null)
            {
                var folder = CurrentAdapter.GetItemAtPosition(position);

                if (folder.Module == ModuleType.Documents)
                {
                    var i = new Intent(Activity, typeof(DocumentsListActivity));
                    i.PutExtra(DocumentsListActivity.FolderIntentKey, SerializationUtils.Serialize(folder.ShallowCopy()));
                    StartActivity(i);
                }
                if (folder.Module == ModuleType.Contacts)
                {
                    var i = new Intent(Activity, typeof(ContactsListActivity));
                    i.PutExtra(ContactsListActivity.FolderIntentKey, SerializationUtils.Serialize(folder.ShallowCopy()));
                    StartActivity(i);
                }
                if (folder.Module == ModuleType.Shortcodes)
                {
                    var i = new Intent(Activity, typeof(ShortcodesListActivity));
                    i.PutExtra(ShortcodesListActivity.FolderIntentKey, SerializationUtils.Serialize(folder.ShallowCopy()));
                    StartActivity(i);
                }
            }
            else
            {
                if (!IsSelectionValid(position))
                {
                    return;
                }

                ToggleSelection(position);
            }
        }

        protected virtual void Adapter_ItemLongClicked(object sender, int position)
        {
            if (!IsSelectionValid(position))
            {
                return;
            }

            if (actionMode == null)
            {
                actionMode = Activity.StartActionMode(this);
            }

            ToggleSelection(position);
        }

        bool IsSelectionValid(int position)
        {
            var selectedIsLocal = CurrentAdapter.GetItemAtPosition(position).Local;
            var sectionForPrelectedItems = CurrentAdapter.GetSectionForSelectedItems();
            var selectedItemNotInPreselectedSection = sectionForPrelectedItems != CurrentAdapter.GetSectionForPosition(position);
            if (selectedIsLocal || (sectionForPrelectedItems != null && selectedItemNotInPreselectedSection))
            {
                return false;
            }

            return true;
        }

        void ToggleSelection(int position)
        {
            CurrentAdapter.ToggleSelection(position);

            var selectedItemsCount = CurrentAdapter.SelectedItemsCount;
            if (selectedItemsCount == 0)
            {
                actionMode.Finish();
            }
            else
            {
                actionMode.Title = selectedItemsCount.ToString();
                actionMode.Invalidate();
            }
        }

        #endregion

        #region ActionMode callbacks

        static class MenuItemActions
        {
            public const int AddToFavourites = 10;
            public const int RemoveFromFavourites = 20;
            public const int Subscribe = 30;
            public const int Unsubscribe = 40;
            public const int EnableOffline = 50;
            public const int DisableOffline = 60;
        }

        public bool OnCreateActionMode(ActionMode mode, IMenu menu)
        {
            return true;
        }

        public virtual bool OnPrepareActionMode(ActionMode mode, IMenu menu)
        {
            Activity.Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
            Activity.Window.SetStatusBarColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));

            fab?.Hide();

            (Activity as MainActivity)?.LockDrawer();

            var selectedFolders = CurrentAdapter.GetSelectedItems().ToList();
            if (!selectedFolders.Any())
            {
                return false;
            }

            var section = CurrentAdapter.GetSectionForSelectedItems();

            menu.Clear();

            var foldersFavoriteState = selectedFolders.Select(f => AsyncHelpers.RunSync(() => Managers.FoldersManager.IsFolderFavouriteAsync(f.Module, f)));

            if (foldersFavoriteState.Any(v => v))
            {
                menu.Add(Menu.None, MenuItemActions.RemoveFromFavourites, MenuItemActions.RemoveFromFavourites, Resource.String.remove_favorites).SetShowAsAction(ShowAsAction.Never);
            }
            if (foldersFavoriteState.Any(v => !v))
            {
                menu.Add(Menu.None, MenuItemActions.AddToFavourites, MenuItemActions.AddToFavourites, Resource.String.add_favorites).SetShowAsAction(ShowAsAction.Never);
            }

            if (section != Section.Favourites)
            {
                var foldersAvailableOfflineState = selectedFolders.Select(f => AsyncHelpers.RunSync(() => Managers.FoldersManager.IsFolderOfflineAsync(f.Module, f)));

                if (foldersAvailableOfflineState.Any(v => v))
                {
                    menu.Add(Menu.None, MenuItemActions.DisableOffline, MenuItemActions.DisableOffline, Resource.String.remove_offline).SetShowAsAction(ShowAsAction.Never);
                }
                if (foldersAvailableOfflineState.Any(v => !v))
                {
                    menu.Add(Menu.None, MenuItemActions.EnableOffline, MenuItemActions.EnableOffline, Resource.String.add_offline).SetShowAsAction(ShowAsAction.Never);
                }

                if (RemoteFolder.Module == ModuleType.Documents && !string.IsNullOrEmpty(PlatformConfig.Preferences.PushNotificationToken))
                {
                    var foldersSubscribedState = selectedFolders.Select(f => f.Subscribed);

                    if (foldersSubscribedState.Any(v => v))
                    {
                        menu.Add(Menu.None, MenuItemActions.Unsubscribe, MenuItemActions.Unsubscribe, Resource.String.disable_notifications_folder).SetShowAsAction(ShowAsAction.Never);
                    }
                    if (foldersSubscribedState.Any(v => !v))
                    {
                        menu.Add(Menu.None, MenuItemActions.Subscribe, MenuItemActions.Subscribe, Resource.String.enable_notifications_folder).SetShowAsAction(ShowAsAction.Never);
                    }
                }
            }
            return true;
        }

        public virtual bool OnActionItemClicked(ActionMode mode, IMenuItem item)
        {
            switch (item.ItemId)
            {
                case MenuItemActions.AddToFavourites:
                    SetFolderFavouriteStatusForSelection(true);
                    break;
                case MenuItemActions.RemoveFromFavourites:
                    SetFolderFavouriteStatusForSelection(false);
                    break;
                case MenuItemActions.EnableOffline:
                    SetFolderOfflineStatusForSelection(true);
                    break;
                case MenuItemActions.DisableOffline:
                    SetFolderOfflineStatusForSelection(false);
                    break;
                case MenuItemActions.Subscribe:
                    SetFoldersSubscriptionToSelection(true);
                    break;
                case MenuItemActions.Unsubscribe:
                    SetFoldersSubscriptionToSelection(false);
                    break;
            }

            return true;
        }

        public void OnDestroyActionMode(ActionMode mode)
        {
            Activity.Window.AddFlags(WindowManagerFlags.TranslucentStatus);
            Activity.Window.SetStatusBarColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));

            fab?.Show();

            (Activity as MainActivity)?.UnlockDrawer();

            CurrentAdapter.ClearSelections();
            actionMode = null;
        }

        #endregion

        #region Folder actions

        void SetFoldersSubscriptionToSelection(bool enabled)
        {
            var selectedFolders = CurrentAdapter.GetSelectedItems().ToList();
            if (!selectedFolders.Any())
            {
                return;
            }

            CommonConfig.Logger.Info($"Setting subscription status of {selectedFolders.Count} folders to {enabled}");

            var token = PlatformConfig.Preferences.PushNotificationToken;
            if (string.IsNullOrEmpty(token))
            {
                Dialogs.ShowConfirmDialog(Activity, Resource.String.notification_token_missing_title, Resource.String.notification_token_missing_content);
                return;
            }

            var module = selectedFolders.First().Module;
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, enabled ? Resource.String.enabling_notifications : Resource.String.disabling_notifications_folders, Resource.String.please_wait);

            Task.Run(async () => { await Managers.NotificationsManager.SetFoldersNotificationsAsync(DeviceType.Android, PlatformConfig.Preferences.PushNotificationToken, module, selectedFolders, enabled); })
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        dismissAction();

                        CommonConfig.Logger.Error($"{(enabled ? "Subscription" : "Unsubscription")}  failed", t.Exception.InnerException);
                        Dialogs.ShowErrorDialog(Activity, t.Exception.InnerException);
                    }
                    else
                    {
                        dismissAction();
                        Adapter.RefreshFolders(selectedFolders, enabled);
                        SearchAdapter.RefreshFolders(selectedFolders, enabled);
                    }

                    actionMode.Finish();
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        void SetFolderOfflineStatusForSelection(bool offline)
        {
            var selectedFolders = CurrentAdapter.GetSelectedItems().ToList();
            if (!selectedFolders.Any())
            {
                return;
            }

            CommonConfig.Logger.Info($"Setting offline status of {selectedFolders.Count} folders to {offline}");

            Task.Run(async () =>
                {
                    foreach (var folder in selectedFolders)
                    {
                        if (offline)
                        {
                            await Managers.FoldersManager.AddOfflineFolderAsync(folder.Module, folder);
                        }
                        else
                        {
                            await Managers.FoldersManager.RemoveOfflineFolderAsync(folder.Module, folder);
                        }
                    }
                })
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        CommonConfig.Logger.Error($"Error while changing offline status for folders", t.Exception.InnerException);
                        Dialogs.ShowErrorDialog(Activity, t.Exception.InnerException);
                    }
                    else
                    {
                        Adapter.RefreshFolders(selectedFolders);
                        SearchAdapter.RefreshFolders(selectedFolders);
                    }

                    actionMode.Finish();
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        void SetFolderFavouriteStatusForSelection(bool favourite)
        {
            var selectedFolders = CurrentAdapter.GetSelectedItems().ToList();
            if (!selectedFolders.Any())
            {
                return;
            }

            CommonConfig.Logger.Info($"Setting favourite status of {selectedFolders.Count} folders to {favourite}");

            Task.Run(async () =>
                {
                    foreach (var folder in selectedFolders)
                    {
                        if (favourite)
                        {
                            await Managers.FoldersManager.AddFavoriteFolderAsync(folder.Module, folder);
                        }
                        else
                        {
                            await Managers.FoldersManager.RemoveFavoriteFolderAsync(folder.Module, folder);
                        }
                    }
                })
                .ContinueWith(async (t) =>
                {
                    if (t.IsFaulted)
                    {
                        CommonConfig.Logger.Error($"Error while changing favourite status for folders", t.Exception.InnerException);
                        Dialogs.ShowErrorDialog(Activity, t.Exception.InnerException);
                    }
                    else
                    {
                        actionMode.Finish();

                        if (AvailableSections.Contains(Section.Favourites))
                        {
                            await RefreshFavorites();
                        }
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        #endregion

        #region SwipeRefresLayout event handlers

        void RefreshLayout_Refresh(object sender, EventArgs e) => RefreshData(true);

        #endregion

        #region Filtering 

        virtual public bool OnMenuItemActionExpand(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_filter)
            {
                menu?.FindItem(10)?.SetVisible(false);

                SearchEnabled = true;
                RefreshLayout.Enabled = false;
                Adapter.ClearSelections();
                RecyclerView.SwapAdapter(SearchAdapter, true);
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

                SearchHandler.RemoveCallbacksAndMessages(null);
                SearchAdapter.Clear();
                RecyclerView.SwapAdapter(Adapter, true);
                RefreshLayout.Enabled = true;
                SearchEnabled = false;
                return true;
            }

            return false;
        }

        public virtual bool OnQueryTextChange(string newText)
        {
            SearchHandler.RemoveCallbacksAndMessages(null);
            SearchHandler.PostDelayed(() =>
            {
                if (string.IsNullOrWhiteSpace(newText))
                {
                    var folder = Folder.RootForModule(RemoteFolder.Module);
                    var matchingFolders = folder.SubFolders.Flatten(f => f.SubFolders).OrderBy(f => f.Name).ToList();
                    SearchAdapter.RefreshSearch(matchingFolders);
                }
                else
                {
                    var root = Folder.RootForModule(RemoteFolder.Module);

                    var resultList = new List<Folder>();
                    SearchRecursively(root, newText, resultList);

                    SearchAdapter.RefreshSearch(resultList);
                }
            }, 500);
            return false;
        }

        void SearchRecursively(Folder folder, string searchText, List<Folder> resultList)
        {
            if (folder.SubFolders == null || folder.SubFolders.Count < 1)
                return;

            foreach (var subFolder in folder.SubFolders)
            {
                if (subFolder.Name.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    resultList.Add(subFolder);

                SearchRecursively(subFolder, searchText, resultList);
            }
        }

        public bool OnQueryTextSubmit(string query)
        {
            return false;
        }

        #endregion

        #region Retained Fragment methods

        public override string GenerateTag()
        {
            return $"{nameof(FoldersListFragment)} [FolderId={RemoteFolder.Id}, ModuleType={RemoteFolder.Module}]";
        }

        public override IRetainableState OnRetainInstanceState()
        {
            CommonConfig.Logger.Info($"Retaining state: [folderName={RemoteFolder?.Name}, folderId={RemoteFolder?.Id}, selectedItemsCount={Adapter.SelectedItemPositions.Count}]");

            return new FolderListFragmentState
            {
                Folder = RemoteFolder,
                SelectedItemPositions = new List<int>(Adapter.SelectedItemPositions)
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var flfs = restoredState as FolderListFragmentState;
            if (flfs != null)
            {
                RemoteFolder = flfs.Folder;
                recoveredSelectedItemsPosition = flfs.SelectedItemPositions;

                CommonConfig.Logger.Info($"Restored state state: [folderName={RemoteFolder.Name}, folderId={RemoteFolder.Id}, selectedItemsCount={recoveredSelectedItemsPosition.Count}]");
            }
        }

        protected class FolderListFragmentState : IRetainableState
        {
            public Folder Folder { get; set; }
            public Folder FavouriteRootFolder { get; set; }
            public List<int> SelectedItemPositions { get; set; }
        }

        #endregion

        #region RecyclerView Adapter

        protected class FolderListAdapter : RecyclerView.Adapter
        {
            public static class ViewType
            {
                public const int FolderView = 0;
                public const int SectionView = 1;
            }

            protected List<Section> sectionsInView = new List<Section>();
            protected Dictionary<Section, List<Folder>> foldersInSection = new Dictionary<Section, List<Folder>>();

            readonly RecyclerView parentView;
            readonly HashSet<int> selectedItemPositions = new HashSet<int>();
            readonly int sectionHeight;
            readonly Context context;

            public event EventHandler<int> ExpandIconClicked = delegate { };
            public event EventHandler<int> ItemClicked = delegate { };
            public event EventHandler<int> ItemLongClicked = delegate { };

            public FolderListAdapter(Context context, RecyclerView parentRecyclerView)
            {
                parentView = parentRecyclerView;
                sectionHeight = ConversionUtils.ConvertDpToPixels(56);
                this.context = context;
            }

            public override int ItemCount
            {
                get { return foldersInSection.Sum(f => f.Value.Count) + (sectionsInView.Count == 1 ? 0 : sectionsInView.Count); }
            }

            public int SelectedItemsCount
            {
                get { return selectedItemPositions.Count; }
            }

            public List<int> SelectedItemPositions
            {
                get { return selectedItemPositions.ToList(); }
            }

            #region Overrides

            public override int GetItemViewType(int position)
            {
                return SectionsPositionToSection().ContainsKey(position) ? ViewType.SectionView : ViewType.FolderView;
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                //Binding of actual parameters, the view is already created
                if (holder is FolderViewHolder)
                {
                    var fh = holder as FolderViewHolder;
                    var folder = GetItemAtPosition(position);

                    fh.FolderNameTitle.Text = folder.Name;

                    var sectionForPosition = GetSectionForPosition(position);
                    if (sectionForPosition == Section.Favourites || sectionForPosition == Section.None)
                    {
                        fh.FolderNameSubTitle.Text = folder.Path;
                    }
                    else
                    {
                        var isFolderAvailableOffline = AsyncHelpers.RunSync(() => Managers.FoldersManager.IsFolderOfflineAsync(folder.Module, folder));

                        var subtitleStrings = new List<string>();
                        if (folder.Subscribed)
                        {
                            subtitleStrings.Add("Notifications On");
                        }
                        if (isFolderAvailableOffline)
                        {
                            subtitleStrings.Add("Available Offline");
                        }

                        fh.FolderNameSubTitle.Text = string.Join(", ", subtitleStrings);
                    }

                    fh.FolderNameSubTitle.Visibility = !string.IsNullOrEmpty(fh.FolderNameSubTitle.Text) ? ViewStates.Visible : ViewStates.Gone;

                    fh.ExpandButton.Visibility = (folder.HasSubFolders && sectionForPosition != Section.None) ? ViewStates.Visible : ViewStates.Gone;

                    if (folder.InternalType == FolderInternalType.Worktray)
                        fh.FolderIcon.SetImageResource(Resource.Drawable.folder_worktray);
                    else if (folder.Type == FolderType.Draft)
                        fh.FolderIcon.SetImageResource(Resource.Drawable.folder_draft);
                    else
                        fh.FolderIcon.SetImageResource(Resource.Drawable.folder);

                    fh.SelectedOverlay.Visibility = IsItemSelected(position) ? ViewStates.Visible : ViewStates.Gone;
                }
                else if (holder is SectionViewHolder)
                {
                    var sh = holder as SectionViewHolder;
                    var section = SectionsPositionToSection()[position];

                    if (foldersInSection[section].Any())
                    {
                        string title = string.Empty;

                        switch (section)
                        {
                            case Section.Favourites:
                                title = context.GetString(Resource.String.folder_favorites);
                                break;
                            case Section.Remote:
                                title = context.GetString(Resource.String.folder_remote);
                                break;
                            case Section.Local:
                                title = context.GetString(Resource.String.folder_local);
                                break;
                        }

                        sh.SectionTitle.Text = title;

                        sh.ItemView.Visibility = ViewStates.Visible;
                        sh.ItemView.LayoutParameters.Height = sectionHeight;
                    }
                    else
                    {
                        sh.ItemView.Visibility = ViewStates.Gone;
                        sh.ItemView.LayoutParameters.Height = 1;
                    }
                }
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                if (viewType == ViewType.FolderView)
                {
                    var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_folders, parent, false);

                    var folderViewHolder = new FolderViewHolder(itemView);
                    folderViewHolder.ExpandClicked += (sender, e) =>
                    {
                        var position = parentView.GetChildLayoutPosition(e);
                        ExpandIconClicked(e, position);
                    };
                    folderViewHolder.ItemClicked += (sender, e) =>
                    {
                        var position = parentView.GetChildLayoutPosition(e);
                        ItemClicked(e, position);
                    };
                    folderViewHolder.ItemLongClicked += (sender, e) =>
                    {
                        var position = parentView.GetChildLayoutPosition(e);
                        ItemLongClicked(e, position);
                    };
                    return folderViewHolder;
                }
                else
                {
                    var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_section, parent, false);
                    return new SectionViewHolder(itemView);
                }
            }

            #endregion

            #region Public methods

            public void Refresh(List<Folder> folders, Section section)
            {
                var sectionPosition = SectionsPositionToSection().FirstOrDefault(c => c.Value == section).Key;
                var offset = sectionsInView.Count == 1 ? 0 : 1;

                var oldItemCount = foldersInSection[section].Count;
                if (oldItemCount > 0)
                {
                    foldersInSection[section].Clear();
                    NotifyItemRangeRemoved(sectionPosition + offset, oldItemCount);
                }

                var newItemCount = folders.Count;
                foldersInSection[section].AddRange(folders);
                NotifyItemRangeInserted(sectionPosition + offset, newItemCount);
                if (sectionsInView.Count > 1)
                {
                    NotifyItemChanged(sectionPosition);
                }
            }

            public void RefreshFolder(Folder folder, bool? subscriptionEnabled = null)
            {
                var offset = sectionsInView.Count == 1 ? 0 : 1;
                var sectionsPositionToSection = SectionsPositionToSection();
                foreach (var section in sectionsInView)
                {
                    var index = foldersInSection[section].FindIndex(f => f.Id == folder.Id);
                    if (index >= 0)
                    {
                        if (subscriptionEnabled.HasValue)
                        {
                            foldersInSection[section][index].Subscribed = subscriptionEnabled.Value;
                        }
                        var sectionPosition = sectionsPositionToSection.FirstOrDefault(c => c.Value == section).Key;
                        NotifyItemChanged(sectionPosition + index + offset);
                    }
                }
            }

            public void RefreshFolders(List<Folder> folders, bool? subscriptionEnabled = null)
            {
                foreach (var folder in folders)
                {
                    RefreshFolder(folder, subscriptionEnabled);
                }
            }

            public void ClearSelections()
            {
                var selectedItemPositionsCopy = new List<int>(selectedItemPositions);
                selectedItemPositions.Clear();
                foreach (var position in selectedItemPositionsCopy)
                {
                    NotifyItemChanged(position);
                }
            }

            public Folder GetItemAtPosition(int position)
            {
                if (sectionsInView.Count == 1)
                {
                    return foldersInSection[sectionsInView.First()][position];
                }

                int sectionPosition = 0;
                var sectionPositionToSection = SectionsPositionToSection();
                var sectionPositions = sectionPositionToSection.Keys.ToList();
                for (int i = sectionPositions.Count - 1; i > 0; i--)
                {
                    if (position > sectionPositions[i])
                    {
                        sectionPosition = sectionPositions[i];
                        break;
                    }
                }

                var section = sectionPositionToSection[sectionPosition];
                return foldersInSection[section][position - sectionPosition - 1];
            }

            public IEnumerable<Folder> GetSelectedItems()
            {
                return selectedItemPositions.Select(i => GetItemAtPosition(i));
            }

            public bool ToggleSelection(int position)
            {
                var isItemSelected = IsItemSelected(position);
                if (isItemSelected)
                {
                    selectedItemPositions.Remove(position);
                }
                else
                {
                    selectedItemPositions.Add(position);
                }

                NotifyItemChanged(position);
                return !isItemSelected;
            }

            public Section? GetSectionForSelectedItems()
            {
                if (selectedItemPositions.Any())
                {
                    return GetSectionForPosition(selectedItemPositions.First());
                }

                return null;
            }

            public Section GetSectionForPosition(int position)
            {
                if (sectionsInView.Count == 1)
                {
                    return sectionsInView[0];
                }

                var sectionPositions = SectionsPositionToSection();

                Section currentSection = Section.Favourites;
                foreach (var sectionPosition in sectionPositions.Keys)
                {
                    if (position > sectionPosition)
                    {
                        currentSection = sectionPositions[sectionPosition];
                    }
                    else
                    {
                        break;
                    }
                }

                return currentSection;
            }

            public void SetSelection(List<int> positionList)
            {
                ClearSelections();
                selectedItemPositions.Clear();
                foreach (var position in positionList)
                {
                    selectedItemPositions.Add(position);
                    NotifyItemChanged(position);
                }
            }

            public void SetSections(List<Section> availableSections)
            {
                sectionsInView = availableSections;
                sectionsInView.ForEach(s => foldersInSection[s] = new List<Folder>());
                NotifyDataSetChanged();
            }

            public void SetSelectionForFolders(IEnumerable<Folder> folders)
            {
                var sectionsPositionToSection = SectionsPositionToSection();
                var offset = sectionsInView.Count == 1 ? 0 : 1;
                foreach (var folder in folders)
                {
                    foreach (var section in sectionsInView)
                    {
                        var sectionPosition = sectionsPositionToSection.FirstOrDefault(c => c.Value == section).Key;

                        var index = foldersInSection[section].FindIndex(f => f.Id == folder.Id);
                        if (index >= 0)
                        {
                            var position = sectionPosition + offset + index;
                            selectedItemPositions.Add(position);
                            NotifyItemChanged(position);
                        }
                    }
                }
            }

            #endregion

            #region Utilities

            bool IsItemSelected(int position)
            {
                return selectedItemPositions.Contains(position);
            }

            Dictionary<int, Section> SectionsPositionToSection()
            {
                if (sectionsInView.Count <= 1)
                {
                    return new Dictionary<int, Section>();
                }

                var positions = new Dictionary<int, Section>();
                positions.Add(0, sectionsInView[0]);

                int previousSectionPosition = 0;
                int previousSectionItemsCount = foldersInSection[sectionsInView[0]].Count;
                for (int i = 1; i < sectionsInView.Count; i++)
                {
                    var sectionPosition = previousSectionPosition + previousSectionItemsCount + 1;
                    positions.Add(sectionPosition, sectionsInView[i]);

                    previousSectionPosition = sectionPosition;
                    previousSectionItemsCount = foldersInSection[sectionsInView[i]].Count;
                }

                return positions;
            }

            #endregion
        }

        protected class SearchFolderListAdapter : FolderListAdapter
        {
            public SearchFolderListAdapter(Context context, RecyclerView parentRecyclerView)
                : base(context, parentRecyclerView)
            {
                sectionsInView = new List<Section>
                {
                    Section.None
                };
                foldersInSection[Section.None] = new List<Folder>();
            }

            public void Clear()
            {
                var itemCount = foldersInSection[Section.None].Count;
                foldersInSection[Section.None].Clear();
                NotifyItemRangeRemoved(0, itemCount);
            }

            public void RefreshSearch(List<Folder> folders)
            {
                Refresh(folders, Section.None);
            }
        }

        #endregion

        #region RecyclerView ViewHolders

        class FolderViewHolder : RecyclerView.ViewHolder
        {
            public AppCompatImageButton ExpandButton { get; private set; }
            public AppCompatTextView FolderNameTitle { get; private set; }
            public AppCompatTextView FolderNameSubTitle { get; private set; }
            public AppCompatImageView FolderIcon { get; private set; }
            public View SelectedOverlay { get; private set; }

            public event EventHandler<View> ExpandClicked = delegate { };
            public event EventHandler<View> ItemClicked = delegate { };
            public event EventHandler<View> ItemLongClicked = delegate { };

            public FolderViewHolder(View itemView)
                : base(itemView)
            {
                // Locate and cache view references
                ExpandButton = itemView.FindViewById<AppCompatImageButton>(Resource.Id.list_item_folder_expand);
                ExpandButton.Click += (sender, e) => { ExpandClicked(this, itemView); };

                FolderNameTitle = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_folder_name);
                FolderNameSubTitle = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_folder_subtitle);

                FolderIcon = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_folder_icon);

                var internalContainerLayout = itemView.FindViewById<LinearLayoutCompat>(Resource.Id.list_item_folder_internal_Layout);
                internalContainerLayout.Click += (sender, e) => ItemClicked(this, itemView);
                internalContainerLayout.LongClick += (sender, e) => ItemLongClicked(this, itemView);

                SelectedOverlay = itemView.FindViewById<View>(Resource.Id.selected_overlay);
            }
        }

        class SectionViewHolder : RecyclerView.ViewHolder
        {
            public AppCompatTextView SectionTitle { get; private set; }

            public SectionViewHolder(View itemView)
                : base(itemView)
            {
                // Locate and cache view references
                SectionTitle = itemView as AppCompatTextView;
            }
        }

        #endregion
    }

    public enum Section
    {
        None,
        Favourites,
        Remote,
        Local,
    }
}