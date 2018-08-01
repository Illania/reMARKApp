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
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class FoldersListFragment : BaseFragment, ActionMode.ICallback, IMenuItemOnActionExpandListener, SearchView.IOnQueryTextListener
    {
        protected const string RemoteFolderBundleKey = "RemoteFolder_551ec209-d787-4a8e-b4ba-99313741ddd1";
        protected const string HideSearchBundleKey = "HideSearch_694b0906-42a6-4c04-9892-238c920f7c74";
        protected const string HideFabBundleKey = "HideFab_35efe47d-6c24-4374-afe4-74713393d00a";
        protected const string LoadRemoteFromCacheBundleKey = "LoadRemote_ae16f485-9e09-4f74-9f47-ad4d357eee12";
        protected const string RecoveredPositionsKey = "RecoveredItemPositions_e71c23ca-686c-4c63-a4ee-022c3855fdeb";
        protected const string SubFoldersDownloadedKey = "SubfoldersDownloaded_35f51cb3-b96c-4f26-be2d-af6760109bbc";

        protected Folder RemoteFolder;
        protected bool HideSearch;
        protected bool HideFab;
        protected bool LoadRemoteFromCache;

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

        protected FolderListAdapter CurrentAdapter => SearchEnabled ? SearchAdapter : Adapter;

        public static FoldersListFragment NewInstance()
        {
            return (new FoldersListFragment());
        }

        public static (FoldersListFragment fragment, string tag) NewInstance(Folder remoteFolder, bool? hideSearch = null)
        {
            var args = new Bundle();

            if (remoteFolder != null)
                args.PutString(RemoteFolderBundleKey, Serializer.Serialize(remoteFolder));

            if (hideSearch != null)
                args.PutBoolean(HideSearchBundleKey, hideSearch.Value);

            var fragment = new FoldersListFragment();
            fragment.Arguments = args;

            var tag = $"{nameof(FoldersListFragment)} [FolderId={remoteFolder.Id}, ModuleType={remoteFolder.Module}]";

            return (fragment, tag);
        }

        #region Overrides

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Arguments.ContainsKey(RemoteFolderBundleKey))
                RemoteFolder = Serializer.Deserialize<Folder>(Arguments.GetString(RemoteFolderBundleKey));

            if (Arguments.ContainsKey(HideSearchBundleKey))
                HideSearch = Arguments.GetBoolean(HideSearchBundleKey);

            if (Arguments.ContainsKey(HideFabBundleKey))
                HideFab = Arguments.GetBoolean(HideFabBundleKey);

            if (Arguments.ContainsKey(LoadRemoteFromCacheBundleKey))
                LoadRemoteFromCache = Arguments.GetBoolean(LoadRemoteFromCacheBundleKey);

            if (savedInstanceState?.ContainsKey(RecoveredPositionsKey) == true)
                recoveredSelectedItemsPosition = Serializer.Deserialize<List<int>>(savedInstanceState.GetString(RecoveredPositionsKey));

            if (savedInstanceState?.ContainsKey(SubFoldersDownloadedKey) == true)
                LoadRemoteFromCache = LoadRemoteFromCache || savedInstanceState.GetBoolean(SubFoldersDownloadedKey);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
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
            RecyclerView.SetBackgroundColor(Color.Transparent);
            var bottomPadding = Conversion.ConvertDpToPixels(56) + (Resources.GetDimension(Resource.Dimension.fab_margin) + 2) * 2;
            RecyclerView.SetPadding(0, 0, 0, (int)bottomPadding);

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

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            if (RemoteFolder.Root)
                RemoteFolder = Folder.RootForModule(RemoteFolder.Module);

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

                ((AppCompatActivity)Activity).SupportActionBar.Title = title;
                ((AppCompatActivity)Activity).SupportActionBar.Subtitle = RemoteFolder.Root ? null : RemoteFolder.Name;
            }

            CommonConfig.Logger.Info($"Created {nameof(FoldersListFragment)} [folder.id={RemoteFolder?.Id}, folder.name={RemoteFolder?.Name}]");
        }

        public override void OnResume()
        {
            base.OnResume();

            CommonConfig.Logger.Info($"Resuming {nameof(FoldersListFragment)} [folder.id={RemoteFolder?.Id}, folder.name={RemoteFolder?.Name}]...");

            fab = ((BaseAppCompatActivity)Activity).Fab;
            if (HideFab)
            {
                fab.Visibility = ViewStates.Gone;
                fab = null;
            }
            else
            {
                if (RemoteFolder?.Module == ModuleType.Documents)
                {
                    fab.SetImageResource(Resource.Drawable.action_new);
                    fab.SetOnClickListener(new ActionOnClickListener(ComposeDocument));
                    fab.Visibility = ViewStates.Visible;
                }
                if (RemoteFolder?.Module == ModuleType.Contacts
                    && ServerConfig.SystemSettings.ContactsModuleInfo.Permissions.CreateAllowed)
                {
                    fab.SetImageResource(Resource.Drawable.action_add);
                    fab.SetOnClickListener(new ActionOnClickListener(CreateContact));
                    fab.Visibility = ViewStates.Visible;
                }
                if (RemoteFolder?.Module == ModuleType.Shortcodes
                    && ServerConfig.SystemSettings.ShortcodesModuleInfo.Permissions.CreateAllowed)
                {
                    fab.SetImageResource(Resource.Drawable.action_add);
                    fab.SetOnClickListener(new ActionOnClickListener(CreateShortcode));
                    fab.Visibility = ViewStates.Visible;
                }
            }

            SetSections();
            RefreshData();
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            if (Adapter?.SelectedItemPositions != null)
                outState.PutString(RecoveredPositionsKey, Serializer.Serialize(Adapter.SelectedItemPositions));

            if (RemoteFolder.SubFolders != null && RemoteFolder.SubFolders.Any())
                outState.PutBoolean(SubFoldersDownloadedKey, true);
        }

        public override void OnUserVisibilityHintChanged()
        {
            if (!UserVisibleHint && menu != null && RecyclerView.GetAdapter() == SearchAdapter)
            {
                menu?.FindItem(10)?.SetVisible(true);

                SearchHandler.RemoveCallbacksAndMessages(null);
                SearchAdapter.Clear();
                RecyclerView.SwapAdapter(Adapter, true);
                RefreshLayout.Enabled = true;
                SearchEnabled = false;
            }
        }

        public override void OnPause()
        {
            base.OnPause();

            CommonConfig.Logger.Info($"Pausing {nameof(FoldersListFragment)} [folder.id={RemoteFolder?.Id}, folder.name={RemoteFolder?.Name}]...");
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            this.menu = menu;

            inflater.Inflate(Resource.Menu.menu_main, menu);

            var filterItem = menu.FindItem(Resource.Id.action_filter);
            filterItem.SetOnActionExpandListener(this);
            SearchView = (SearchView)filterItem.ActionView;
            SearchView.QueryHint = GetString(Resource.String.filter);
            SearchView.SetOnQueryTextListener(this);

            if (!HideSearch)
            {
                var searchItem = menu.Add(Menu.None, 10, Menu.None, Resource.String.search);
                searchItem.SetIcon(Resource.Drawable.action_search_server);
                searchItem.SetShowAsAction(ShowAsAction.Always);
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == 10)
            {
                StartActivity(SearchActivity.CreateIntent(Context, RemoteFolder.Module));
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        protected virtual View InflateView(LayoutInflater inflater, ViewGroup container)
        {
            return inflater.Inflate(Resource.Layout.list, container, false);
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

        async void CreateContact()
        {
            var values = new List<ContactType> { ContactType.Company, ContactType.Department, ContactType.Person };
            var index = await Dialogs.ShowListDialog(Context, Resource.String.edit_contact_dialog_title, values.Select(v => GetString(UI.ContactTypeResourceId(v))).ToArray(), true);
            if (index >= 0)
                StartActivity(AddEditContactActivity.CreateIntent(Context, contactCreationModeFlag: (int)ContactCreationModeFlag.New, contactType: (int)values[index]));
        }

        void CreateShortcode()
        {
            StartActivity(AddEditShortcodeActivity.CreateIntent(Context, ShortcodeCreationModeFlag.New));
        }

        #endregion

        #region Utility methods

        protected virtual void RestoreSelection()
        {
            CommonConfig.Logger.Info("Restoring selected items");

            if (recoveredSelectedItemsPosition?.Any() == true)
            {
                actionMode = Activity.StartActionMode(this);
                Adapter.SetSelection(recoveredSelectedItemsPosition);
                actionMode.Title = Adapter.SelectedItemsCount.ToString();
                actionMode.Invalidate();
                recoveredSelectedItemsPosition = null;
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
                    AvailableSections.Add(Section.Local);
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

        protected virtual (BaseFragment fragment, string tag) GetFolderFragment(Folder folder)
        {
            return NewInstance(folder, HideSearch);
        }

        void RefreshLocal()
        {
            CommonConfig.Logger.Info($"Refreshing local folders...");

            var localRootFolder = Folder.LocalRootForModule(RemoteFolder.Module);
            Adapter.Refresh(localRootFolder.SubFolders, Section.Local);
        }

        void NavigateToFolder(Folder folder)
        {
            CommonConfig.UsageAnalytics.LogEvent(new ExpandFolderEvent(folder.Module));

            var fragmentManager = ((AppCompatActivity)Activity).SupportFragmentManager;
            var (foldersListFragment, tag) = GetFolderFragment(folder);

            fragmentManager.BeginTransaction().SetCustomAnimations(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left, Resource.Animation.enter_from_left, Resource.Animation.exit_to_right).Replace(Resource.Id.fragment_container, foldersListFragment, tag).AddToBackStack(tag).Commit();
        }

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

            RestoreSelection();
        }

        async Task RefreshRemote(bool forceRefresh = false)
        {
            CommonConfig.Logger.Info($"Refreshing remote folders...");

            if (forceRefresh || !RemoteFolder.SubFolders.Any())
            {
                List<Folder> remoteFolders = null;

                if (LoadRemoteFromCache)
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

                if (remoteFolders == null)
                    try
                    {
                        remoteFolders = await Managers.FoldersManager.GetFoldersAsync(RemoteFolder);
                    }
                    catch (Exception ex)
                    {
                        CommonConfig.Logger.Error($"Downloading folders failed [folder.name={RemoteFolder.Name}, folder.id={RemoteFolder.Id}]", ex);
                        await Dialogs.ShowErrorDialogAsync(Activity, ex);
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

        #endregion

        #region List item event handlers

        protected virtual void Adapter_ItemClicked(object sender, int position)
        {
            if (actionMode == null)
            {
                var (folder, section) = CurrentAdapter.GetItemAtPosition(position);

                if (folder.IsOutgoing)
                    CommonConfig.UsageAnalytics.LogEvent(new OpenOutgoingFolderEvent());
                else
                    CommonConfig.UsageAnalytics.LogEvent(new OpenFolderEvent(folder.Module, section == Section.Favourites));

                if (folder.Module == ModuleType.Documents)
                    StartActivity(DocumentsListActivity.CreateIntent(Context, folder.ShallowCopy()));
                if (folder.Module == ModuleType.Contacts)
                    StartActivity(ContactsListActivity.CreateIntent(Context, folder.ShallowCopy()));
                if (folder.Module == ModuleType.Shortcodes)
                    StartActivity(ShortcodesListActivity.CreateIntent(Context, folder.ShallowCopy()));
            }
            else
            {
                if (!IsSelectionValid(position))
                    return;

                ToggleSelection(position);
            }
        }

        protected virtual void Adapter_ItemLongClicked(object sender, int position)
        {
            if (!IsSelectionValid(position))
                return;

            if (actionMode == null)
                actionMode = Activity.StartActionMode(this);

            ToggleSelection(position);
        }

        void Adapter_ExpandClicked(object sender, int position)
        {
            NavigateToFolder(CurrentAdapter.GetItemAtPosition(position).Folder);
        }

        bool IsSelectionValid(int position)
        {
            var selectedIsLocal = CurrentAdapter.GetItemAtPosition(position).Folder.Local;
            var sectionForPrelectedItems = CurrentAdapter.GetSectionForSelectedItems();
            var selectedItemNotInPreselectedSection = sectionForPrelectedItems != CurrentAdapter.GetSectionForPosition(position);
            if (selectedIsLocal || sectionForPrelectedItems != null && selectedItemNotInPreselectedSection)
                return false;

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

        public bool OnCreateActionMode(ActionMode mode, IMenu menu)
        {
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

        public virtual bool OnPrepareActionMode(ActionMode mode, IMenu menu)
        {
            Activity.Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
            Activity.Window.SetStatusBarColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));

            fab?.Hide();

            (Activity as MainActivity)?.LockDrawer();

            var selectedFolders = CurrentAdapter.GetSelectedItems().ToList();
            if (!selectedFolders.Any())
                return false;

            var section = CurrentAdapter.GetSectionForSelectedItems();

            menu.Clear();

            var foldersFavoriteState = selectedFolders.Select(f => AsyncHelpers.RunSync(() => Managers.FoldersManager.IsFolderFavouriteAsync(f.Module, f)));

            if (foldersFavoriteState.Any(v => v))
                menu.Add(Menu.None, MenuItemActions.RemoveFromFavourites, MenuItemActions.RemoveFromFavourites, Resource.String.remove_favorites).SetShowAsAction(ShowAsAction.Never);
            if (foldersFavoriteState.Any(v => !v))
                menu.Add(Menu.None, MenuItemActions.AddToFavourites, MenuItemActions.AddToFavourites, Resource.String.add_favorites).SetShowAsAction(ShowAsAction.Never);

            if (section != Section.Favourites)
            {
                var foldersAvailableOfflineState = selectedFolders.Select(f => AsyncHelpers.RunSync(() => Managers.FoldersManager.IsSavedFolderOfflineInfo(f)));

                if (RemoteFolder.Module == ModuleType.Documents)
                {
                    if (foldersAvailableOfflineState.Any(v => v))
                        menu.Add(Menu.None, MenuItemActions.DisableOffline, MenuItemActions.DisableOffline, Resource.String.remove_offline).SetShowAsAction(ShowAsAction.Never);
                    if (foldersAvailableOfflineState.Any(v => !v))
                        menu.Add(Menu.None, MenuItemActions.EnableOffline, MenuItemActions.EnableOffline, Resource.String.add_offline).SetShowAsAction(ShowAsAction.Never);
                }

                if ((RemoteFolder.Module == ModuleType.Contacts || RemoteFolder.Module == ModuleType.Shortcodes) && selectedFolders.Count == 1 && AsyncHelpers.RunSync(() => Managers.FoldersManager.IsSavedFolderOfflineInfo(selectedFolders[0])))
                    menu.Add(Menu.None, MenuItemActions.MakeOnline, MenuItemActions.MakeOnline, Resource.String.make_online).SetShowAsAction(ShowAsAction.Never);

                if ((RemoteFolder.Module == ModuleType.Contacts || RemoteFolder.Module == ModuleType.Shortcodes) && selectedFolders.Count == 1)
                    menu.Add(Menu.None, MenuItemActions.SaveOffline, MenuItemActions.SaveOffline, Resource.String.save_offline).SetShowAsAction(ShowAsAction.Never);

                if (RemoteFolder.Module == ModuleType.Documents && !string.IsNullOrEmpty(PlatformConfig.Preferences.PushNotificationToken))
                {
                    var foldersSubscribedState = selectedFolders.Select(f => f.Subscribed);

                    if (foldersSubscribedState.Any(v => v))
                        menu.Add(Menu.None, MenuItemActions.Unsubscribe, MenuItemActions.Unsubscribe, Resource.String.disable_notifications_folder).SetShowAsAction(ShowAsAction.Never);
                    if (foldersSubscribedState.Any(v => !v))
                        menu.Add(Menu.None, MenuItemActions.Subscribe, MenuItemActions.Subscribe, Resource.String.enable_notifications_folder).SetShowAsAction(ShowAsAction.Never);
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
                case MenuItemActions.MakeOnline:
                    MakeFolderOnline();
                    break;
                case MenuItemActions.SaveOffline:
                    SaveFolderOffline();
                    break;
            }

            return true;
        }

        static class MenuItemActions
        {
            public const int AddToFavourites = 10;
            public const int RemoveFromFavourites = 20;
            public const int Subscribe = 30;
            public const int Unsubscribe = 40;
            public const int EnableOffline = 50;
            public const int DisableOffline = 60;
            public const int MakeOnline = 70;
            public const int SaveOffline = 80;
        }

        #endregion

        #region Folder actions

        async void SetFoldersSubscriptionToSelection(bool enabled)
        {
            var selectedFolders = CurrentAdapter.GetSelectedItems().ToList();
            if (!selectedFolders.Any())
                return;

            CommonConfig.UsageAnalytics.LogEvent(new SetFolderNotifyEvent(selectedFolders.First().Module, selectedFolders.Count()));

            CommonConfig.Logger.Info($"Setting subscription status of {selectedFolders.Count} folders to {enabled}");

            var token = PlatformConfig.Preferences.PushNotificationToken;
            if (string.IsNullOrEmpty(token))
            {
                Dialogs.ShowConfirmDialog(Activity, Resource.String.notification_token_missing_title, Resource.String.notification_token_missing_content);
                return;
            }

            var module = selectedFolders.First().Module;
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, enabled ? Resource.String.enabling_notifications : Resource.String.disabling_notifications_folders, Resource.String.please_wait);

            var t = Managers.NotificationsManager.SetFoldersNotificationsAsync(DeviceType.Android, PlatformConfig.Preferences.PushNotificationToken, module, selectedFolders, enabled);
            await t;

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
        }

        async void SetFolderOfflineStatusForSelection(bool offline)
        {
            var selectedFolders = CurrentAdapter.GetSelectedItems().ToList();
            if (!selectedFolders.Any())
                return;

            CommonConfig.UsageAnalytics.LogEvent(new SetFolderSyncEvent(selectedFolders.First().Module, selectedFolders.Count()));

            CommonConfig.Logger.Info($"Setting offline status of {selectedFolders.Count} folders to {offline}");

            Task t = null;

            foreach (var folder in selectedFolders)
            {
                if (offline)
                    t = Managers.FoldersManager.AddSavedFolderInfo(folder);
                else
                    t = Managers.FoldersManager.RemoveSavedFolderInfo(folder);
                await t;
            }

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
        }

        async void SetFolderFavouriteStatusForSelection(bool favourite)
        {
            var selectedFolders = CurrentAdapter.GetSelectedItems().ToList();
            if (!selectedFolders.Any())
                return;

            CommonConfig.UsageAnalytics.LogEvent(new SetFolderFavoriteEvent(selectedFolders.First().Module, selectedFolders.Count()));

            CommonConfig.Logger.Info($"Setting favourite status of {selectedFolders.Count} folders to {favourite}");

            Task t = null;
            foreach (var folder in selectedFolders)
            {
                if (favourite)
                    t = Managers.FoldersManager.AddFavoriteFolderAsync(folder.Module, folder);
                else
                    t = Managers.FoldersManager.RemoveFavoriteFolderAsync(folder.Module, folder);
                await t;
            }

            if (t.IsFaulted)
            {
                CommonConfig.Logger.Error($"Error while changing favourite status for folders", t.Exception.InnerException);
                Dialogs.ShowErrorDialog(Activity, t.Exception.InnerException);
            }
            else
            {
                actionMode.Finish();

                if (AvailableSections.Contains(Section.Favourites))
                    await RefreshFavorites();
            }
        }

        void MakeFolderOnline()
        {
            var selectedFolder = CurrentAdapter.GetSelectedItems().FirstOrDefault();
            if (selectedFolder == null)
                return;

            Task.Run(async () =>
            {
                await Managers.FoldersManager.RemoveSavedFolderInfo(selectedFolder);
            }).ContinueWith((t) =>
            {
                if (t.IsFaulted)
                {
                    CommonConfig.Logger.Error($"Error while making folder online.", t.Exception.InnerException);
                    Dialogs.ShowErrorDialog(Activity, t.Exception.InnerException);
                }
                else
                {
                    actionMode.Finish();
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        void SaveFolderOffline()
        {
            var selectedFolder = CurrentAdapter.GetSelectedItems().FirstOrDefault();
            if (selectedFolder == null)
                return;

            StartActivity(DownloadActivity.CreateIntent(Context, selectedFolder.ShallowCopy()));
        }

        #endregion

        #region SwipeRefresLayout event handlers

        void RefreshLayout_Refresh(object sender, EventArgs e)
        {
            CommonConfig.UsageAnalytics.LogEvent(new PullToRefreshEvent(true, RemoteFolder.Module));
            RefreshData(true);
        }

        #endregion

        #region Filtering 

        public bool OnQueryTextSubmit(string query)
        {
            return false;
        }

        public virtual bool OnMenuItemActionExpand(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_filter)
            {
                CommonConfig.UsageAnalytics.LogEvent(new FilterEvent(true, RemoteFolder.Module));

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

        public virtual bool OnQueryTextChange(string newText)
        {
            SearchHandler.RemoveCallbacksAndMessages(null);
            SearchHandler.PostDelayed(async () =>
                {
                    if (string.IsNullOrWhiteSpace(newText))
                    {
                        var folder = Folder.RootForModule(RemoteFolder.Module);
                        var matchingFolders = folder.SubFolders.Flatten(f => f.SubFolders).OrderBy(f => f.Name).ToList();
                    SearchAdapter.RefreshSearch(matchingFolders,newText);
                    }
                    else
                    {
                        var root = Folder.RootForModule(RemoteFolder.Module);

                        var localFolders = new List<Folder>();
                        SearchRecursively(root, newText, localFolders);

                        try
                        {
                            #pragma warning disable CS4014 // We dont need to await this call
                            Task.Run(async () => {
                                var remoteFolders = await Managers.FoldersManager.SearchFolders(newText);
                                SearchAdapter.RefreshSearch(remoteFolders, newText);
                            });
                            #pragma warning restore CS4014

                        }
                        catch (Exception ex)
                        {
                            CommonConfig.Logger.Error(ex);
                        }
                        
                        SearchAdapter.RefreshSearch(localFolders, newText);
                    }
                },
                500);
            return false;
        }

        bool IMenuItemOnActionExpandListener.OnMenuItemActionCollapse(IMenuItem item)
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

        #endregion

        #region RecyclerView Adapter

        protected class FolderListAdapter : RecyclerView.Adapter
        {
            public override int ItemCount { get { return foldersInSection.Sum(f => f.Value.Count) + (sectionsInView.Count == 1 ? 0 : sectionsInView.Count); } }

            public int SelectedItemsCount => selectedItemPositions.Count;

            public List<int> SelectedItemPositions => selectedItemPositions.ToList();

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
                sectionHeight = Conversion.ConvertDpToPixels(56);
                this.context = context;
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
                    var folder = GetItemAtPosition(position).Folder;

                    fh.FolderNameTitle.Text = folder.Name;

                    var sectionForPosition = GetSectionForPosition(position);
                    if (sectionForPosition == Section.Favourites || sectionForPosition == Section.None)
                    {
                        fh.FolderNameSubTitle.Text = folder.Path;
                    }
                    else
                    {
                        var subtitleString = string.Empty;
                        if (folder.Subscribed)
                            subtitleString += context.GetString(Resource.String.notifications);
                        if (AsyncHelpers.RunSync(() => Managers.FoldersManager.IsSavedFolderOfflineInfo(folder)))
                        {
                            if (subtitleString.Length > 0)
                                subtitleString += ", ";
                            subtitleString += context.GetString(Resource.String.offline);
                        }

                        fh.FolderNameSubTitle.Text = subtitleString;
                    }

                    fh.FolderNameSubTitle.Visibility = !string.IsNullOrEmpty(fh.FolderNameSubTitle.Text) ? ViewStates.Visible : ViewStates.Gone;

                    fh.ExpandButton.Visibility = folder.HasSubFolders && sectionForPosition != Section.None ? ViewStates.Visible : ViewStates.Invisible;

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
                        var title = string.Empty;

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
                    NotifyItemChanged(sectionPosition);
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
                            foldersInSection[section][index].Subscribed = subscriptionEnabled.Value;
                        var sectionPosition = sectionsPositionToSection.FirstOrDefault(c => c.Value == section).Key;
                        NotifyItemChanged(sectionPosition + index + offset);
                    }
                }
            }

            public void RefreshFolders(List<Folder> folders, bool? subscriptionEnabled = null)
            {
                foreach (var folder in folders)
                    RefreshFolder(folder, subscriptionEnabled);
            }

            public void ClearSelections()
            {
                var selectedItemPositionsCopy = new List<int>(selectedItemPositions);
                selectedItemPositions.Clear();
                foreach (var position in selectedItemPositionsCopy)
                    NotifyItemChanged(position);
            }

            public (Folder Folder, Section Section) GetItemAtPosition(int position)
            {
                if (sectionsInView.Count == 1)
                    return (foldersInSection[sectionsInView.First()][position], sectionsInView.First());

                var sectionPosition = 0;
                var sectionPositionToSection = SectionsPositionToSection();
                var sectionPositions = sectionPositionToSection.Keys.ToList();
                for (var i = sectionPositions.Count - 1; i > 0; i--)
                    if (position > sectionPositions[i])
                    {
                        sectionPosition = sectionPositions[i];
                        break;
                    }

                var section = sectionPositionToSection[sectionPosition];
                return (foldersInSection[section][position - sectionPosition - 1], section);
            }

            public IEnumerable<Folder> GetSelectedItems()
            {
                return selectedItemPositions.Select(i => GetItemAtPosition(i).Folder);
            }

            public bool ToggleSelection(int position)
            {
                var isItemSelected = IsItemSelected(position);
                if (isItemSelected)
                    selectedItemPositions.Remove(position);
                else
                    selectedItemPositions.Add(position);

                NotifyItemChanged(position);
                return !isItemSelected;
            }

            public Section? GetSectionForSelectedItems()
            {
                if (selectedItemPositions.Any())
                    return GetSectionForPosition(selectedItemPositions.First());

                return null;
            }

            public Section GetSectionForPosition(int position)
            {
                if (sectionsInView.Count == 1)
                    return sectionsInView[0];

                var sectionPositions = SectionsPositionToSection();

                var currentSection = Section.Favourites;
                foreach (var sectionPosition in sectionPositions.Keys)
                    if (position > sectionPosition)
                        currentSection = sectionPositions[sectionPosition];
                    else
                        break;

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

            #endregion

            #region Utilities

            bool IsItemSelected(int position)
            {
                return selectedItemPositions.Contains(position);
            }

            Dictionary<int, Section> SectionsPositionToSection()
            {
                if (sectionsInView.Count <= 1)
                    return new Dictionary<int, Section>();

                var positions = new Dictionary<int, Section>
                {
                    { 0, sectionsInView[0] }
                };
                var previousSectionPosition = 0;
                var previousSectionItemsCount = foldersInSection[sectionsInView[0]].Count;
                for (var i = 1; i < sectionsInView.Count; i++)
                {
                    var sectionPosition = previousSectionPosition + previousSectionItemsCount + 1;
                    positions.Add(sectionPosition, sectionsInView[i]);

                    previousSectionPosition = sectionPosition;
                    previousSectionItemsCount = foldersInSection[sectionsInView[i]].Count;
                }

                return positions;
            }

            #endregion

            public static class ViewType
            {
                public const int FolderView = 0;
                public const int SectionView = 1;
            }
        }

        protected class SearchFolderListAdapter : FolderListAdapter
        {
            string searchQuery = String.Empty;

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

            public void RefreshSearch(List<Folder> folders, string searchText)
            {
                if(searchQuery.Equals(searchText)) {
                    var mergedFolders = foldersInSection[Section.None].Union(folders, new FolderComparer()).ToList();
                    Refresh(mergedFolders, Section.None);
                } else {
                    Refresh(folders, Section.None);
                }

                searchQuery = searchText;
            }
        }

        #endregion

        #region RecyclerView ViewHolders

        class FolderViewHolder : RecyclerView.ViewHolder
        {
            public AppCompatImageButton ExpandButton { get; }
            public AppCompatTextView FolderNameTitle { get; }
            public AppCompatTextView FolderNameSubTitle { get; }
            public AppCompatImageView FolderIcon { get; }
            public View SelectedOverlay { get; }

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
            public AppCompatTextView SectionTitle { get; }

            public SectionViewHolder(View itemView)
                : base(itemView)
            {
                // Locate and cache view references
                SectionTitle = itemView as AppCompatTextView;
                SectionTitle.TextSize = 18;
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