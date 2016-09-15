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
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Views.Activities;
using Mark5.Mobile.Droid.Views.Common;

namespace Mark5.Mobile.Droid.Views.Fragments
{
    public class FirstFolderListFragment : RetainableStateFragment, ActionMode.ICallback
    {
        public Folder Folder { get; set; }

        FirstFolderListAdapter adapter;
        RecyclerView recyclerView;
        SwipeRefreshLayout refreshLayout;
        ActionMode actionMode;
        List<int> recoveredSelectedItemsPosition;

        #region Overrides

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
            refreshLayout.Refresh += RefreshLayout_Refresh;

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recyclerView);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));
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

            if (forceRefresh || !Folder.SubFolders.Any())
            {
                refreshLayout.Post(() => refreshLayout.Refreshing = true); //Not a good way, but it's a bug, fixed in support library v 24.2.0 (issue 77712)

                var folders = await Managers.FoldersManager.GetFoldersAsync(Folder, 2);
                Folder.SubFolders.Clear();
                Folder.SubFolders = folders;

                adapter.Refresh(folders);
                refreshLayout.Post(() => refreshLayout.Refreshing = false); //Not a good way, but it's a bug, fixed in support library v 24.2.0 (issue 77712)
            }
            else
            {
                adapter.Refresh(Folder.SubFolders);
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

        bool ActionMode.ICallback.OnActionItemClicked(ActionMode mode, IMenuItem item)
        {
            switch (item.ItemId)
            {
                case 1:
                    foreach (var folder in adapter.GetSelectedItems())
                    {
                        CommonConfig.Logger.Error("FOLDER SELECTED: " + folder);
                    }
                    mode.Finish();
                    return true;
                case 2:
                    mode.Finish();
                    return true;
                case 3:
                    mode.Finish();
                    return true;
                default:
                    return false;
            }
        }

        bool ActionMode.ICallback.OnCreateActionMode(ActionMode mode, IMenu menu)
        {
            menu.Add(Menu.None, 1, Menu.None, "First").SetIcon(Resource.Drawable.abc_ic_menu_share_mtrl_alpha);
            menu.Add(Menu.None, 6, Menu.None, "Six").SetIcon(Resource.Drawable.abc_ic_menu_copy_mtrl_am_alpha);
            menu.Add(Menu.None, 2, Menu.None, "Second");
            menu.Add(Menu.None, 3, Menu.None, "Third");
            menu.Add(Menu.None, 4, Menu.None, "Four");
            menu.Add(Menu.None, 5, Menu.None, "Five");

            return true;
        }

        void ActionMode.ICallback.OnDestroyActionMode(ActionMode mode)
        {
            adapter.ClearSelection();
            actionMode = null;
        }

        bool ActionMode.ICallback.OnPrepareActionMode(ActionMode mode, IMenu menu)
        {
            return false;
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
                SelectedItemPositions = new List<int>(adapter.SelectedItemPositions),
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var flfs = restoredState as FolderListFragmentState;
            if (flfs != null)
            {
                Folder = flfs.Folder;
                recoveredSelectedItemsPosition = flfs.SelectedItemPositions;
            }
        }

        class FolderListFragmentState : IRetainableState
        {
            public Folder Folder { get; set; }
            public List<int> SelectedItemPositions { get; set; }
        }

        #endregion
    }

    #region RecyclerView Adapter/ViewHolder

    class FirstFolderListAdapter : RecyclerView.Adapter
    {
        public static class ViewType
        {
            public const int FolderView = 0;
            public const int SectionView = 1;
        }

        public static class Section
        {
            public const int Favourites = 0;
            public const int Remote = 1;
            public const int Local = 2;
        }

        readonly List<Folder> foldersInView = new List<Folder>();
        readonly RecyclerView parentView;
        readonly List<int> selectedItemPositions = new List<int>();

        public event EventHandler<int> ExpandIconClicked = delegate { };
        public event EventHandler<int> ItemClicked = delegate { };
        public event EventHandler<int> ItemLongClicked = delegate { };

        public FirstFolderListAdapter(RecyclerView parentRecyclerView)
        {
            parentView = parentRecyclerView;
        }

        public override int ItemCount
        {
            get
            {
                return foldersInView.Count;
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

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            //Binding of actual parameters, the view is already created
            var fh = holder as FolderViewHolder;
            var folder = foldersInView[position];

            fh.FolderNameTitle.Text = folder.Name;
            fh.FolderNameSubTitle.Text = folder.Name + "sdf";

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

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
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

        #endregion

        public void Refresh(List<Folder> folders)
        {
            foldersInView.Clear();
            foldersInView.AddRange(folders);
            NotifyDataSetChanged();
        }

        #region Selection methods

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
            return foldersInView[position];
        }

        public IEnumerable<Folder> GetSelectedItems()
        {
            return selectedItemPositions.Select(i => foldersInView[i]);
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

        bool IsItemSelected(int position)
        {
            return selectedItemPositions.Contains(position);
        }

        #endregion
    }

    #endregion

}

