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

        FolderListAdapter adapter;
        RecyclerView recyclerView;
        SwipeRefreshLayout refreshLayout;
        ActionMode actionMode;

        #region Overrides

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
            refreshLayout.Refresh += RefreshLayout_Refresh;

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recyclerView);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.HasFixedSize = true;

            adapter = new FolderListAdapter(recyclerView);
            adapter.expandIconClicked += Adapter_ExpandClicked;
            adapter.itemClicked += Adapter_ItemClicked;
            adapter.itemLongClicked += Adapter_ItemLongClicked;

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

                var folders = await Managers.FoldersManager.GetFoldersAsync(Folder.Root ? null : Folder, 0); //TODO do we do this check here or in the manager?
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

        void NavigateInFolder(ModuleType moduleType, Folder folder)
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
            NavigateInFolder(Folder.Module, adapter.GetItemAtPosition(position));
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
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var flfs = restoredState as FolderListFragmentState;
            if (flfs != null)
            {
                Folder = flfs.Folder;
            }
        }

        class FolderListFragmentState : IRetainableState
        {
            public Folder Folder { get; set; }
        }

        #endregion

    }

    #region RecyclerView Adapter/ViewHolder

    class FolderListAdapter : RecyclerView.Adapter
    {
        readonly List<Folder> foldersInView = new List<Folder>();
        readonly RecyclerView parentView;
        readonly List<int> selectedItemPositions = new List<int>();

        public event EventHandler<int> expandIconClicked = delegate { }; //TODO case
        public event EventHandler<int> itemClicked = delegate { };
        public event EventHandler<int> itemLongClicked = delegate { };

        public FolderListAdapter(RecyclerView parentRecyclerView)
        {
            this.parentView = parentRecyclerView;
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


        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {

            //Binding of actual parameters, the view is already created
            var fh = holder as FolderViewHolder;
            var folder = foldersInView[position];

            fh.FolderName.Text = folder.Name;
            fh.ExpandButtonLayout.Visibility = folder.HasSubFolders ? ViewStates.Visible : ViewStates.Gone;
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

        public void ClearSelection() //TODO put in the right place
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

        bool IsItemSelected(int position) //TODO put in the right position, check also the accessbility of all the objects
        {
            return selectedItemPositions.Contains(position);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).
                                          Inflate(Resource.Layout.list_item_folder, parent, false);

            var folderViewHolder = new FolderViewHolder(itemView);
            folderViewHolder.expandClicked += FolderViewHolder_ExpandClicked;
            folderViewHolder.itemClicked += FolderViewHolder_ItemClicked;
            folderViewHolder.itemLongClicked += FolderViewHolder_ItemLongClicked;
            return folderViewHolder;
        }

        public void Refresh(List<Folder> folders)
        {
            foldersInView.Clear();
            foldersInView.AddRange(folders);
            NotifyDataSetChanged();
        }

        void FolderViewHolder_ExpandClicked(object sender, View view)
        {
            var position = parentView.GetChildLayoutPosition(view);
            expandIconClicked(view, position);
        }

        void FolderViewHolder_ItemClicked(object sender, View view)
        {
            var position = parentView.GetChildLayoutPosition(view);
            itemClicked(view, position);
        }

        void FolderViewHolder_ItemLongClicked(object sender, View view)
        {
            var position = parentView.GetChildLayoutPosition(view);
            itemLongClicked(view, position);
        }
    }

    class FolderViewHolder : RecyclerView.ViewHolder
    {
        public LinearLayoutCompat ExpandButtonLayout { get; private set; }
        public TextView FolderName { get; private set; }
        public ImageView FolderIcon { get; private set; }
        public View SelectedOverlay { get; private set; }

        public event EventHandler<View> expandClicked = delegate { };
        public event EventHandler<View> itemClicked = delegate { };
        public event EventHandler<View> itemLongClicked = delegate { };

        public FolderViewHolder(View itemView) : base(itemView)
        {
            // Locate and cache view references
            ExpandButtonLayout = itemView.FindViewById<LinearLayoutCompat>(Resource.Id.expandButtonLayout);
            ExpandButtonLayout.Click += (sender, e) => { expandClicked(this, itemView); };

            FolderName = itemView.FindViewById<TextView>(Resource.Id.folderName);
            FolderIcon = itemView.FindViewById<ImageView>(Resource.Id.folderIcon);

            var internalContainerLayout = itemView.FindViewById<LinearLayoutCompat>(Resource.Id.internalContainerLayout);
            internalContainerLayout.Click += (sender, e) => itemClicked(this, itemView);
            internalContainerLayout.LongClick += (sender, e) => itemLongClicked(this, itemView);

            SelectedOverlay = itemView.FindViewById<View>(Resource.Id.selected_overlay);
            SelectedOverlay.Background.Alpha = 125;
        }
    }

    #endregion

}

