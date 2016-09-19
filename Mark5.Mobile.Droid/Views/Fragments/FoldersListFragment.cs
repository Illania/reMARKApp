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
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Views.Activities;
using Mark5.Mobile.Droid.Views.Common;

namespace Mark5.Mobile.Droid.Views.Fragments
{
    public class FoldersListFragment : RetainableStateFragment, ActionMode.ICallback
    {
        public Folder Folder { get; set; }
        Folder FavouriteRootFolder;

        FirstFolderListAdapter adapter;
        RecyclerView recyclerView;
        SwipeRefreshLayout refreshLayout;
        ActionMode actionMode;
        List<int> recoveredSelectedItemsPosition;
        List<Section> availableSections;

        #region Overrides

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
            refreshLayout.Refresh += RefreshLayout_Refresh;

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recyclerView);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));
            recyclerView.SetItemAnimator(new DefaultItemAnimator());
            recyclerView.HasFixedSize = true;

            adapter = new FirstFolderListAdapter(recyclerView);
            adapter.ExpandIconClicked += Adapter_ExpandClicked;
            adapter.ItemClicked += Adapter_ItemClicked;
            adapter.ItemLongClicked += Adapter_ItemLongClicked;

            recyclerView.SetAdapter(adapter);

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
                if (forceRefresh || !Folder.SubFolders.Any())
                {
                    var folders = await Managers.FoldersManager.GetFoldersAsync(Folder, 2);
                    Folder.SubFolders.Clear();
                    Folder.SubFolders = folders;

                    adapter.Refresh(folders, Section.Remote);
                }
                else
                {
                    adapter.Refresh(Folder.SubFolders, Section.Remote);
                }
            }
            if (availableSections.Contains(Section.Favourites))
            {
                await RefreshFavorites(forceRefresh);
            }
            if (availableSections.Contains(Section.Local))
            {

            }

            refreshLayout.Post(() => refreshLayout.Refreshing = false); //Not a good way, but it's a bug, fixed in support library v 24.2.0 (issue 77712)
        }

        async Task RefreshFavorites(bool forceRefresh = false)
        {
            if (FavouriteRootFolder == null)
            {
                FavouriteRootFolder = Folder.RootPerModule(Folder.Module, true);
            }
            if (forceRefresh || !FavouriteRootFolder.SubFolders.Any())
            {
                var folders = await Managers.FoldersManager.GetFavoriteFoldersAsync(Folder.Module);
                FavouriteRootFolder.SubFolders.Clear();
                FavouriteRootFolder.SubFolders = folders;

                adapter.Refresh(folders, Section.Favourites);
            }
            else
            {
                adapter.Refresh(FavouriteRootFolder.SubFolders, Section.Favourites);
            }
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
                availableSections = new List<Section> { Section.Favourites, Section.Remote, Section.Local };
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
            NavigateInFolder(adapter.GetItemAtPosition(position));
        }

        void Adapter_ItemClicked(object sender, int position)
        {
            if (actionMode == null)
            {
                var i = new Intent(Activity, typeof(DocumentsListActivity));
                i.PutExtra(DocumentsListActivity.FolderIntentKey, SerializationUtils.Serialize(adapter.GetItemAtPosition(position)));
                StartActivity(i);
            }
            else
            {
                ToggleSelection(position);
            }
        }

        void Adapter_ItemLongClicked(object sender, int position)
        {
            if (actionMode == null)
            {
                actionMode = Activity.StartActionMode(this);
            }

            ToggleSelection(position);
        }

        void ToggleSelection(int position)
        {
            adapter.TogggleSelection(position);

            var selectedItemsCount = adapter.SelectedItemsCount;
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
            switch (item.ItemId)
            {
                case MenuItemActions.AddToFavourites:
                    AddSelectionToFavourites();
                    actionMode.Finish();
                    RefreshFavorites(true).Wait();
                    return true;
                case MenuItemActions.RemoveFromFavourites:
                    RemoveSelectionFromFavourites();
                    actionMode.Finish(); //TODO could put the actionModeFinish at the end
                    RefreshFavorites(true).Wait();
                    return true;
                case MenuItemActions.EnableOffline:
                    AddSelectionToAvailableOffline();
                    mode.Finish();
                    return true;
                case MenuItemActions.DisableOffline:
                    RemoveSelectionFromAvailableOffline();
                    mode.Finish();
                    return true;
                case MenuItemActions.Subscribe:
                    SubscribeToSelection();
                    mode.Finish();
                    return true;
                case MenuItemActions.Unsubscribe:
                    UnsubscribeFromSelection();
                    mode.Finish();
                    return true;

                default:
                    return false;
            }
        }


        bool ActionMode.ICallback.OnCreateActionMode(ActionMode mode, IMenu menu)
        {
            return true;
        }

        void ActionMode.ICallback.OnDestroyActionMode(ActionMode mode)
        {
            adapter.ClearSelection();
            actionMode = null;
        }

        bool ActionMode.ICallback.OnPrepareActionMode(ActionMode mode, IMenu menu)
        {
            var selectedFolders = adapter.GetSelectedItems().ToList();
            if (!selectedFolders.Any())
            {
                return false;
            }

            menu.Clear();

            var areAllSelectedFoldersSubscribed = selectedFolders.All(f => f.Subscribed);
            var areAllSelectedFoldersFavourites = selectedFolders.All(f => AsyncHelpers.RunSync(() => Managers.FoldersManager.IsFolderFavouriteAsync(f.Module, f)));
            var areAllSelectedFolderOffline = selectedFolders.All(f => AsyncHelpers.RunSync(() => Managers.FoldersManager.IsFolderOfflineAsync(f.Module, f)));

            var favoritesString = areAllSelectedFoldersFavourites ? "Remove from favourites" : "Add to favourites";
            var offlineString = areAllSelectedFolderOffline ? "Disable offline mode" : "Enable offline mode"; //TODO check wording
            var subscriptionString = areAllSelectedFoldersSubscribed ? "Unsubscribe" : "Subscribe";

            menu.Add(Menu.None, areAllSelectedFoldersFavourites ? MenuItemActions.RemoveFromFavourites : MenuItemActions.AddToFavourites, Menu.None, favoritesString).SetShowAsAction(ShowAsAction.Never);
            menu.Add(Menu.None, areAllSelectedFolderOffline ? MenuItemActions.DisableOffline : MenuItemActions.EnableOffline, Menu.None, offlineString).SetShowAsAction(ShowAsAction.Never);
            menu.Add(Menu.None, areAllSelectedFoldersSubscribed ? MenuItemActions.Unsubscribe : MenuItemActions.Subscribe, Menu.None, subscriptionString).SetShowAsAction(ShowAsAction.Never);

            return true;
        }

        void UnsubscribeFromSelection()
        {
            throw new NotImplementedException();
        }

        void SubscribeToSelection()
        {
            throw new NotImplementedException();
        }

        void RemoveSelectionFromAvailableOffline()
        {
            var selectedFolders = adapter.GetSelectedItems().ToList();
            if (!selectedFolders.Any())
            {
                return;
            }

            foreach (var folder in selectedFolders)
            {
                AsyncHelpers.RunSync(() => Managers.FoldersManager.RemoveOfflineFolderAsync(folder.Module, folder));
                adapter.RefreshFolder(folder);
            }
        }

        void AddSelectionToAvailableOffline()
        {
            var selectedFolders = adapter.GetSelectedItems().ToList();
            if (!selectedFolders.Any())
            {
                return;
            }

            foreach (var folder in selectedFolders)
            {
                AsyncHelpers.RunSync(() => Managers.FoldersManager.AddOfflineFolderAsync(folder.Module, folder));
                adapter.RefreshFolder(folder);
            }

        }

        void AddSelectionToFavourites() //TODO move to a new place
        {
            var selectedFolders = adapter.GetSelectedItems().ToList();
            if (!selectedFolders.Any())
            {
                return;
            }

            foreach (var folder in selectedFolders)
            {
                AsyncHelpers.RunSync(() => Managers.FoldersManager.AddFavoriteFolderAsync(folder.Module, folder));
            }
        }

        void RemoveSelectionFromFavourites() //TODO move to a new place
        {
            var selectedFolders = adapter.GetSelectedItems().ToList();
            if (!selectedFolders.Any())
            {
                return;
            }

            foreach (var folder in selectedFolders)
            {
                AsyncHelpers.RunSync(() => Managers.FoldersManager.RemoveFavoriteFolderAsync(folder.Module, folder));
            }

        }


        static class MenuItemActions
        {
            public const int AddToFavourites = 0;
            public const int RemoveFromFavourites = 1;
            public const int Subscribe = 2;
            public const int Unsubscribe = 3;
            public const int EnableOffline = 4;
            public const int DisableOffline = 5;
        }

        #endregion


        #region SwipeRefresLayout event handlers

        async void RefreshLayout_Refresh(object sender, EventArgs e)
        {
            await RefreshData(true);
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
        Favourites,
        Remote,
        Local,
    }

    #region RecyclerView Adapter

    class FirstFolderListAdapter : RecyclerView.Adapter
    {
        public static class ViewType
        {
            public const int FolderView = 0;
            public const int SectionView = 1;
        }

        List<Section> sectionsInView = new List<Section>();
        Dictionary<Section, List<Folder>> foldersInSection = new Dictionary<Section, List<Folder>>();

        readonly RecyclerView parentView;
        readonly List<int> selectedItemPositions = new List<int>();

        public event EventHandler<int> ExpandIconClicked = delegate { };
        public event EventHandler<int> ItemClicked = delegate { };
        public event EventHandler<int> ItemLongClicked = delegate { };

        public FirstFolderListAdapter(RecyclerView parentRecyclerView)
        {
            parentView = parentRecyclerView;
            foldersInSection[Section.Favourites] = new List<Folder>();
            foldersInSection[Section.Remote] = new List<Folder>();
            foldersInSection[Section.Local] = new List<Folder>();
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
                var isFolderAvailableOffline = AsyncHelpers.RunSync(() => Managers.FoldersManager.IsFolderOfflineAsync(folder.Module, folder));

                fh.FolderNameSubTitle.Text = string.Empty;
                if (isFolderSubscribed)
                {
                    fh.FolderNameSubTitle.Text += "/SUBSCRIBED";
                }
                if (isFolderAvailableOffline)
                {
                    fh.FolderNameSubTitle.Text += "/OFFLINE";
                }

                fh.ExpandButton.Visibility = folder.HasSubFolders ? ViewStates.Visible : ViewStates.Gone;
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

                fh.SelectedOverlay.Visibility = IsItemSelected(position) ? ViewStates.Visible : ViewStates.Invisible;
            }
            else if (holder is SectionViewHolder)
            {
                var sh = holder as SectionViewHolder;
                var section = SectionsPositionToSection()[position];
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
        }

        public void RefreshFolder(Folder folder)
        {
            var offset = sectionsInView.Count == 1 ? 0 : 1;
            var sectionsPositionToSection = SectionsPositionToSection();
            foreach (var section in sectionsInView)
            {
                var index = foldersInSection[section].FindIndex(f => f.Id == folder.Id);
                if (index >= 0)
                {
                    var sectionPosition = sectionsPositionToSection.FirstOrDefault(c => c.Value == section).Key;
                    NotifyItemChanged(sectionPosition + index + offset);
                }
            }
        }

        public void ClearSelection()
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

        public void SetSelection(List<int> positionList)
        {
            ClearSelection();
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

    #endregion


    #region RecyclerView ViewHolders

    class FolderViewHolder : RecyclerView.ViewHolder
    {
        public ImageButton ExpandButton { get; private set; }
        public TextView FolderNameTitle { get; private set; }
        public TextView FolderNameSubTitle { get; private set; }
        public ImageView FolderIcon { get; private set; }
        public View SelectedOverlay { get; private set; }

        public event EventHandler<View> ExpandClicked = delegate { };
        public event EventHandler<View> ItemClicked = delegate { };
        public event EventHandler<View> ItemLongClicked = delegate { };

        public FolderViewHolder(View itemView) : base(itemView)
        {
            // Locate and cache view references
            ExpandButton = itemView.FindViewById<ImageButton>(Resource.Id.list_item_folder_expand);
            ExpandButton.Click += (sender, e) => { ExpandClicked(this, itemView); };

            FolderNameTitle = itemView.FindViewById<TextView>(Resource.Id.list_item_folder_name);
            FolderNameSubTitle = itemView.FindViewById<TextView>(Resource.Id.list_item_folder_subtitle);

            FolderIcon = itemView.FindViewById<ImageView>(Resource.Id.list_item_folder_icon);

            var internalContainerLayout = itemView.FindViewById<LinearLayoutCompat>(Resource.Id.list_item_folder_internal_Layout);
            internalContainerLayout.Click += (sender, e) => ItemClicked(this, itemView);
            internalContainerLayout.LongClick += (sender, e) => ItemLongClicked(this, itemView);

            SelectedOverlay = itemView.FindViewById<View>(Resource.Id.selected_overlay);
            SelectedOverlay.Background.Alpha = 125;
        }
    }

    class SectionViewHolder : RecyclerView.ViewHolder
    {
        public TextView SectionTitle { get; private set; }

        public SectionViewHolder(View itemView) : base(itemView)
        {
            // Locate and cache view references
            SectionTitle = itemView as TextView;
        }
    }

    #endregion



}

