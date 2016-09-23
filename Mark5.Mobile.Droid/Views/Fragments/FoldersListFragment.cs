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
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Views.Activities;
using Mark5.Mobile.Droid.Views.Common;

namespace Mark5.Mobile.Droid.Views.Fragments
{
    public class FoldersListFragment : RetainableStateFragment, ActionMode.ICallback, View.IOnClickListener, SearchView.IOnQueryTextListener, SearchView.IOnCloseListener
    {
        public Folder Folder { get; set; }
        Folder FavouriteRootFolder;

        FolderListAdapter adapter;
        SearchFolderListAdapter searchAdapter;
        SearchView searchView;
        RecyclerView recyclerView;
        SwipeRefreshLayout refreshLayout;
        ActionMode actionMode;
        List<int> recoveredSelectedItemsPosition;
        List<Section> availableSections;

        bool searchEnabled;

        readonly Handler searchHandler = new Handler();

        #region Overrides

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
            refreshLayout.SetColorSchemeResources(Resource.Color.lightbrown, Resource.Color.brown);
            refreshLayout.Refresh += RefreshLayout_Refresh;

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recyclerView);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));
            recyclerView.SetItemAnimator(new DefaultItemAnimator());
            recyclerView.HasFixedSize = true;

            adapter = new FolderListAdapter(recyclerView);
            adapter.ExpandIconClicked += Adapter_ExpandClicked;
            adapter.ItemClicked += Adapter_ItemClicked;
            adapter.ItemLongClicked += Adapter_ItemLongClicked;

            searchAdapter = new SearchFolderListAdapter(recyclerView);
            searchAdapter.ItemClicked += Adapter_ItemClicked;
            searchAdapter.ItemLongClicked += Adapter_ItemLongClicked;

            recyclerView.SetAdapter(adapter);

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = Folder.Module.ToString();
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = Folder.Root ? string.Empty : Folder.Name;
        }

        public async override void OnResume()
        {
            base.OnResume();

            SetSections();
            await RefreshData();
            RestoreSelection();
        }

        public override void OnPause()
        {
            base.OnPause();
            if (actionMode != null)
            {
                actionMode.Finish();
            }
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.menu_main, menu);

            var searchItem = menu.FindItem(Resource.Id.action_search);
            searchView = (SearchView)MenuItemCompat.GetActionView(searchItem);
            searchView.QueryHint = GetString(Resource.String.filter);
            searchView.SetOnSearchClickListener(this);
            searchView.SetOnQueryTextListener(this);
            searchView.SetOnCloseListener(this);
        }

        #endregion

        #region Utility methods

        async Task RefreshData(bool forceRefresh = false)
        {
            if (!Folder.HasSubFolders)
            {
                return;
            }

            refreshLayout.Post(() => refreshLayout.Refreshing = true); //Not a good way, but it's a bug, fixed in support library v 24.2.0 (issue 77712)

            if (availableSections.Contains(Section.Remote))
            {
                await RefreshOffline(forceRefresh);
            }
            if (availableSections.Contains(Section.Favourites))
            {
                await RefreshFavorites();
            }
            if (availableSections.Contains(Section.Local))
            {
                RefreshLocal();
            }

            refreshLayout.Post(() => refreshLayout.Refreshing = false); //Not a good way, but it's a bug, fixed in support library v 24.2.0 (issue 77712)
        }

        async Task RefreshOffline(bool forceRefresh = false)
        {
            if (forceRefresh || !Folder.SubFolders.Any())
            {
                try
                {
                    var folders = await Managers.FoldersManager.GetFoldersAsync(Folder, 2);
                    Folder.SubFolders.Clear();
                    Folder.SubFolders = folders;

                    adapter.Refresh(folders, Section.Remote);
                }
                catch (Exception ex)
                {
                    await Dialogs.ShowErrorDialogAsync(Activity, ex);
                }
            }
            else
            {
                adapter.Refresh(Folder.SubFolders, Section.Remote);
            }
        }

        async Task RefreshFavorites()
        {
            if (FavouriteRootFolder == null)
            {
                FavouriteRootFolder = Folder.RootPerModule(Folder.Module, true);
            }

            var folders = await Managers.FoldersManager.GetFavoriteFoldersAsync(Folder.Module);
            FavouriteRootFolder.SubFolders.Clear();
            FavouriteRootFolder.SubFolders = folders;

            adapter.Refresh(folders, Section.Favourites);

        }

        void RefreshLocal()
        {
            var localRootFolder = Folder.LocalRootPerModule(Folder.Module);
            adapter.Refresh(localRootFolder.SubFolders, Section.Local);
        }

        void RestoreSelection()
        {
            if (recoveredSelectedItemsPosition != null && recoveredSelectedItemsPosition.Any())
            {
                actionMode = Activity.StartActionMode(this);
                adapter.SetSelection(recoveredSelectedItemsPosition);
                actionMode.Title = adapter.SelectedItemsCount.ToString();
                actionMode.Invalidate();
            }
        }

        void SetSections()
        {
            if (Folder.Root)
            {
                availableSections = new List<Section> { Section.Favourites, Section.Remote };
                if (Folder.Module == ModuleType.Documents)
                {
                    availableSections.Add(Section.Local);
                }
            }
            else
            {
                availableSections = new List<Section> { Section.Remote };
            }

            adapter.SetSections(availableSections);
        }

        void NavigateInFolder(Folder folder)
        {
            var fragmentManager = ((AppCompatActivity)Activity).SupportFragmentManager;

            var foldersListFragment = new FoldersListFragment
            {
                Folder = folder,
            };

            var tag = foldersListFragment.GenerateTag();
            var ft = fragmentManager.BeginTransaction();
            ft.SetTransition((int)FragmentTransit.FragmentOpen);
            ft.Replace(Resource.Id.fragment_container, foldersListFragment, tag);
            ft.AddToBackStack(tag);
            ft.Commit();
        }

        #endregion

        #region List item event handlers

        void Adapter_ExpandClicked(object sender, int position)
        {
            NavigateInFolder(CurrentAdapter.GetItemAtPosition(position));
        }

        void Adapter_ItemClicked(object sender, int position)
        {
            if (actionMode == null)
            {
                var folder = CurrentAdapter.GetItemAtPosition(position);

                if (folder.Module == ModuleType.Documents)
                {
                    var i = new Intent(Activity, typeof(DocumentsListActivity));
                    i.PutExtra(DocumentsListActivity.FolderIntentKey, SerializationUtils.Serialize(folder));
                    StartActivity(i);
                }
                if (folder.Module == ModuleType.Contacts)
                {
                    var i = new Intent(Activity, typeof(ContactsListActivity));
                    i.PutExtra(ContactsListActivity.FolderIntentKey, SerializationUtils.Serialize(folder));
                    StartActivity(i);
                }
                if (folder.Module == ModuleType.Shortcodes)
                {
                    var i = new Intent(Activity, typeof(ShortcodesListActivity));
                    i.PutExtra(ShortcodesListActivity.FolderIntentKey, SerializationUtils.Serialize(folder));
                    StartActivity(i);
                }
            }
            else
            {
                if (CurrentAdapter.GetItemAtPosition(position).Local)
                {
                    return;
                }

                var sectionForSelectedItems = CurrentAdapter.GetSectionForSelectedItems();
                if (sectionForSelectedItems != null && sectionForSelectedItems != CurrentAdapter.GetSectionForPosition(position))
                {
                    return;
                }

                ToggleSelection(position);
            }
        }

        void Adapter_ItemLongClicked(object sender, int position)
        {
            if (CurrentAdapter.GetItemAtPosition(position).Local)
            {
                return;
            }

            var sectionForSelectedItems = CurrentAdapter.GetSectionForSelectedItems();
            if (sectionForSelectedItems != null && sectionForSelectedItems != CurrentAdapter.GetSectionForPosition(position))
            {
                return;
            }

            if (actionMode == null)
            {
                actionMode = Activity.StartActionMode(this);
            }

            ToggleSelection(position);
        }

        void ToggleSelection(int position)
        {
            CurrentAdapter.TogggleSelection(position);

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

        bool ActionMode.ICallback.OnActionItemClicked(ActionMode mode, IMenuItem item)
        {
            bool favouritesAction = false;

            switch (item.ItemId)
            {
                case MenuItemActions.AddToFavourites:
                    SetFolderFavouriteStatusForSelection(true);
                    favouritesAction = true;
                    break;
                case MenuItemActions.RemoveFromFavourites:
                    SetFolderFavouriteStatusForSelection(false);
                    favouritesAction = true;
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

            mode.Finish();

            if (favouritesAction && availableSections.Contains(Section.Favourites))
            {
                RefreshFavorites().Wait();
            }

            return true;
        }

        bool ActionMode.ICallback.OnCreateActionMode(ActionMode mode, IMenu menu)
        {
            return true;
        }

        void ActionMode.ICallback.OnDestroyActionMode(ActionMode mode)
        {
            CurrentAdapter.ClearSelections();
            actionMode = null;
        }

        bool ActionMode.ICallback.OnPrepareActionMode(ActionMode mode, IMenu menu)
        {
            var selectedFolders = CurrentAdapter.GetSelectedItems().ToList();
            if (!selectedFolders.Any())
            {
                return false;
            }

            var section = CurrentAdapter.GetSectionForSelectedItems();

            menu.Clear();

            menu.Add(Menu.None, MenuItemActions.AddToFavourites, MenuItemActions.AddToFavourites, "Add to favourites").SetShowAsAction(ShowAsAction.Never);
            menu.Add(Menu.None, MenuItemActions.RemoveFromFavourites, MenuItemActions.RemoveFromFavourites, "Remove from favourites").SetShowAsAction(ShowAsAction.Never);

            if (section != Section.Favourites)
            {
                menu.Add(Menu.None, MenuItemActions.EnableOffline, MenuItemActions.EnableOffline, "Enable offline mode").SetShowAsAction(ShowAsAction.Never);
                menu.Add(Menu.None, MenuItemActions.DisableOffline, MenuItemActions.DisableOffline, "Disable offline mode").SetShowAsAction(ShowAsAction.Never);

                if (Folder.Module == ModuleType.Documents)
                {
                    menu.Add(Menu.None, MenuItemActions.Subscribe, MenuItemActions.Subscribe, "Subscribe").SetShowAsAction(ShowAsAction.Never);
                    menu.Add(Menu.None, MenuItemActions.Unsubscribe, MenuItemActions.Unsubscribe, "Unsubscribe").SetShowAsAction(ShowAsAction.Never);

                }
            }
            return true;
        }

        FolderListAdapter CurrentAdapter
        {
            get { return searchEnabled ? searchAdapter : adapter; }
        }

        static class MenuItemActions
        {
            public const int AddToFavourites = 10;
            public const int RemoveFromFavourites = 20;
            public const int Subscribe = 30;
            public const int Unsubscribe = 40;
            public const int EnableOffline = 50;
            public const int DisableOffline = 60;
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

            var token = PlatformConfig.Preferences.PushNotificationToken;
            if (string.IsNullOrEmpty(token))
            {
                Dialogs.ShowConfirmDialog(Activity, Resource.String.subscription_token_missing_title, Resource.String.subscription_token_missing_content);
                return;
            }

            var module = selectedFolders.First().Module;
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(this.Activity, enabled ? Resource.String.subscribing_folders : Resource.String.unsubscribing_folders, Resource.String.please_wait);

            Task.Run(async () =>
            {
                await Managers.NotificationsManager.SetFoldersNotificationsAsync(DeviceType.Android, PlatformConfig.Preferences.PushNotificationToken, module, selectedFolders, enabled);

            }).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    dismissAction();

                    CommonConfig.Logger.Error($"{(enabled ? "Subscription" : "Unsubscription")}  failed", t.Exception);
                    Dialogs.ShowErrorDialog(Activity, t.Exception);
                }
                else
                {
                    dismissAction();
                    adapter.RefreshFolders(selectedFolders, enabled);
                    searchAdapter.RefreshFolders(selectedFolders, enabled);
                }

            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        void SetFolderOfflineStatusForSelection(bool offline)
        {
            var selectedFolders = CurrentAdapter.GetSelectedItems().ToList();
            if (!selectedFolders.Any())
            {
                return;
            }

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
            }).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    CommonConfig.Logger.Error($"Error while changing offline status for folders", t.Exception);
                    Dialogs.ShowErrorDialog(Activity, t.Exception);
                }
                else
                {
                    adapter.RefreshFolders(selectedFolders);
                    searchAdapter.RefreshFolders(selectedFolders);
                }
            });
        }

        void SetFolderFavouriteStatusForSelection(bool favourite)
        {
            var selectedFolders = CurrentAdapter.GetSelectedItems().ToList();
            if (!selectedFolders.Any())
            {
                return;
            }

            foreach (var folder in selectedFolders)
            {
                if (favourite)
                {
                    AsyncHelpers.RunSync(() => Managers.FoldersManager.AddFavoriteFolderAsync(folder.Module, folder));
                }
                else
                {
                    AsyncHelpers.RunSync(() => Managers.FoldersManager.RemoveFavoriteFolderAsync(folder.Module, folder));
                }
            }
        }

        #endregion

        #region SwipeRefresLayout event handlers

        async void RefreshLayout_Refresh(object sender, EventArgs e)
        {
            await RefreshData(true);
        }

        #endregion

        #region Filtering 

        void View.IOnClickListener.OnClick(View v)
        {
            if (v == searchView)
            {
                searchEnabled = true;
                // refreshLayout.Enabled = false;
                adapter.ClearSelections();
                recyclerView.SwapAdapter(searchAdapter, true);
            }
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
                    var matchingFolders = GetMatchingFolders(newText);
                    searchAdapter.RefreshSearch(matchingFolders);
                }
            }, 500);
            return false;
        }

        bool SearchView.IOnQueryTextListener.OnQueryTextSubmit(string query)
        {
            return false;
        }

        bool SearchView.IOnCloseListener.OnClose()
        {
            searchAdapter.Clear();
            recyclerView.SwapAdapter(adapter, true);
            refreshLayout.Enabled = true;
            searchEnabled = false;
            return false;
        }

        List<Folder> GetMatchingFolders(string query)
        {
            var folder = Folder.RootPerModule(Folder.Module);
            var flattenedFolders = folder.SubFolders.Flatten(f => f.SubFolders);
            return flattenedFolders.Where(f => f.Name.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0).ToList();
        }

        #endregion

        #region Retained Fragment methods

        public override string GenerateTag()
        {
            return $"{nameof(FoldersListFragment)} [FolderId={Folder.Id}, ModuleType={Folder.Module}]";
        }

        public override IRetainableState OnRetainInstanceState()
        {
            return new FolderListFragmentState
            {
                Folder = Folder,
                FavouriteRootFolder = FavouriteRootFolder,
                SelectedItemPositions = new List<int>(adapter.SelectedItemPositions),
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var flfs = restoredState as FolderListFragmentState;
            if (flfs != null)
            {
                Folder = flfs.Folder;
                FavouriteRootFolder = flfs.FavouriteRootFolder;
                recoveredSelectedItemsPosition = flfs.SelectedItemPositions;
            }
        }

        class FolderListFragmentState : IRetainableState
        {
            public Folder Folder { get; set; }
            public Folder FavouriteRootFolder { get; set; }
            public List<int> SelectedItemPositions { get; set; }
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

    #region RecyclerView Adapter

    class FolderListAdapter : RecyclerView.Adapter
    {
        public static class ViewType
        {
            public const int FolderView = 0;
            public const int SectionView = 1;
        }

        protected List<Section> sectionsInView = new List<Section>();
        protected Dictionary<Section, List<Folder>> foldersInSection = new Dictionary<Section, List<Folder>>();

        readonly RecyclerView parentView;
        readonly List<int> selectedItemPositions = new List<int>();
        readonly int sectionHeight;

        public event EventHandler<int> ExpandIconClicked = delegate { };
        public event EventHandler<int> ItemClicked = delegate { };
        public event EventHandler<int> ItemLongClicked = delegate { };

        public FolderListAdapter(RecyclerView parentRecyclerView)
        {
            parentView = parentRecyclerView;
            sectionHeight = ConversionUtils.ConvertDpToPixels(48);
        }

        public override int ItemCount
        {
            get
            {
                return foldersInSection.Sum(f => f.Value.Count) + (sectionsInView.Count == 1 ? 0 : sectionsInView.Count);
            }
        }

        public int SelectedItemsCount
        {
            get
            {
                return selectedItemPositions.Count;
            }
        }

        public List<int> SelectedItemPositions
        {
            get
            {
                return selectedItemPositions;
            }
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

                var isFolderSubscribed = folder.Subscribed;

                var sectionForPosition = GetSectionForPosition(position);
                if (sectionForPosition == Section.Favourites || sectionForPosition == Section.None)
                {
                    fh.FolderNameSubTitle.Text = folder.Path;
                }
                else
                {
                    var isFolderAvailableOffline = AsyncHelpers.RunSync(() => Managers.FoldersManager.IsFolderOfflineAsync(folder.Module, folder));

                    var subtitleStrings = new List<string>();
                    if (isFolderSubscribed)
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
                {
                    fh.FolderIcon.SetImageResource(Resource.Drawable.folder_worktray);
                }
                else if (folder.Type == FolderType.Spam)
                {
                    fh.FolderIcon.SetImageResource(Resource.Drawable.folder_spam);
                }
                else if (folder.Type == FolderType.Draft)
                {
                    fh.FolderIcon.SetImageResource(Resource.Drawable.folder_draft);
                }
                else
                {
                    fh.FolderIcon.SetImageResource(Resource.Drawable.folder); //TODO need to add icon for local
                }

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
                            title = "Favourites";
                            break;
                        case Section.Remote:
                            title = "Remote";
                            break;
                        case Section.Local:
                            title = "Local";
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
                View itemView = LayoutInflater.From(parent.Context).
                                  Inflate(Resource.Layout.list_item_folder, parent, false);

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
                View itemView = LayoutInflater.From(parent.Context).
                                              Inflate(Resource.Layout.list_item_section, parent, false);
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

        public void TogggleSelection(int position)
        {
            if (IsItemSelected(position))
            {
                selectedItemPositions.Remove(position);
            }
            else
            {
                selectedItemPositions.Add(position);
            }

            NotifyItemChanged(position);
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

    class SearchFolderListAdapter : FolderListAdapter
    {
        public SearchFolderListAdapter(RecyclerView parentRecyclerView) : base(parentRecyclerView)
        {
            sectionsInView = new List<Section> { Section.None };
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
        public View FrameLayout { get; private set; }

        public event EventHandler<View> ExpandClicked = delegate { };
        public event EventHandler<View> ItemClicked = delegate { };
        public event EventHandler<View> ItemLongClicked = delegate { };

        public FolderViewHolder(View itemView) : base(itemView)
        {
            FrameLayout = itemView;

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

        public SectionViewHolder(View itemView) : base(itemView)
        {
            // Locate and cache view references
            SectionTitle = itemView as AppCompatTextView;
        }
    }

    #endregion



}

