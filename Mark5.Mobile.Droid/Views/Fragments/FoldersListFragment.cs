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

        #region Fragment overrides

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
            refreshLayout.Refresh += async (sender, e) => await RefreshData(true);

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recyclerView);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.HasFixedSize = true;

            adapter = new FolderListAdapter(recyclerView);
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
        }

        #endregion

        #region RetainedFragment overrides

        public override IRetainableState OnRetainInstanceState()
        {
            return new FolderListFragmentState
            {
                Folder = Folder
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

        public override string GenerateTag()
        {
            return $"{nameof(FoldersListFragment)} [FolderId={Folder.Id}, ModuleType={Folder.Module}]";
        }

        #endregion

        #region Utility methods

        async Task RefreshData(bool forceRefresh = false)
        {
            if (forceRefresh || !Folder.SubFolders.Any())
            {
                refreshLayout.Post(() => refreshLayout.Refreshing = true); //Bug: fixed in support library v 24.2.0 (issue 77712)

                var folders = await Managers.FoldersManager.GetFoldersAsync(Folder, 2);
                Folder.SubFolders.Clear();
                Folder.SubFolders = folders;

                adapter.Refresh(folders);
                refreshLayout.Post(() => refreshLayout.Refreshing = false); //Bug: fixed in support library v 24.2.0 (issue 77712)
            }
            else
            {
                adapter.Refresh(Folder.SubFolders);
            }
        }

        #endregion

        #region Adapter event handlers

        void Adapter_ItemClicked(object sender, Folder folder)
        {
            var i = new Intent(Activity, typeof(DocumentsListActivity));
            i.PutExtra(DocumentsListActivity.FolderIntentKey, SerializationUtils.Serialize(folder.ShallowCopy()));
            StartActivity(i);
        }

        void Adapter_ItemLongClicked(object sender, Folder folder)
        {
            if (actionMode != null)
            {
                return;
            }

            var itemView = sender as View;
            itemView.Selected = true;
            itemView.Activated = true;

            actionMode = Activity.StartActionMode(this);
        }

        void Adapter_ExpandClicked(object sender, Folder folder)
        {
            var foldersListFragment = new FoldersListFragment
            {
                Folder = folder
            };

            var tag = foldersListFragment.GenerateTag();

            var ft = ((AppCompatActivity)Activity).SupportFragmentManager.BeginTransaction();
            ft.SetTransition((int)FragmentTransit.FragmentOpen);
            ft.Replace(Resource.Id.fragment_container, foldersListFragment, tag);
            ft.AddToBackStack(tag);
            ft.Commit();
        }

        #endregion

        #region Action menu

        bool ActionMode.ICallback.OnPrepareActionMode(ActionMode mode, IMenu menu)
        {
            return false;
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
            actionMode = null;
        }

        bool ActionMode.ICallback.OnActionItemClicked(ActionMode mode, IMenuItem item)
        {
            switch (item.ItemId)
            {
                case 1:
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

        #endregion

        #region State class

        class FolderListFragmentState : IRetainableState
        {

            public Folder Folder { get; set; }

            public ModuleType Module { get; set; }
        }

        #endregion

        #region RecyclerView Adapter/ViewHolder

        class FolderListAdapter : RecyclerView.Adapter
        {

            public override int ItemCount
            {
                get
                {
                    return foldersInView.Count;
                }
            }

            readonly List<Folder> foldersInView = new List<Folder>();
            readonly RecyclerView parentRecyclerView;

            public event EventHandler<Folder> ExpandIconClicked = delegate { };
            public event EventHandler<Folder> ItemClicked = delegate { };
            public event EventHandler<Folder> ItemLongClicked = delegate { };

            public FolderListAdapter(RecyclerView parentRecyclerView)
            {
                this.parentRecyclerView = parentRecyclerView;
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var fvh = holder as FolderViewHolder;
                var folder = foldersInView[position];

                fvh.FolderName.Text = folder.Name;
                fvh.ExpandButton.Visibility = folder.HasSubFolders ? ViewStates.Visible : ViewStates.Gone;
                if (folder.InternalType == FolderInternalType.Worktray)
                {
                    fvh.FolderIcon.SetImageResource(Resource.Drawable.folder_worktray);
                }
                else if (folder.Type == FolderType.Spam)
                {
                    fvh.FolderIcon.SetImageResource(Resource.Drawable.folder_spam);
                }
                else if (folder.Type == FolderType.Draft)
                {
                    fvh.FolderIcon.SetImageResource(Resource.Drawable.folder_draft);
                }
                else
                {
                    fvh.FolderIcon.SetImageResource(Resource.Drawable.folder);
                }
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                View itemView = LayoutInflater.From(parent.Context).
                                              Inflate(Resource.Layout.list_item_folder, parent, false);

                var folderViewHolder = new FolderViewHolder(itemView);
                folderViewHolder.ExpandClicked += (sender, view) =>
                {
                    var position = parentRecyclerView.GetChildLayoutPosition(view);
                    var folder = foldersInView[position];
                    ExpandIconClicked(view, folder);
                };
                folderViewHolder.ItemClicked += (sender, view) =>
                {
                    var position = parentRecyclerView.GetChildLayoutPosition(view);
                    var folder = foldersInView[position];
                    ItemClicked(view, folder);
                };
                folderViewHolder.ItemLongClicked += (sender, view) =>
                {
                    var position = parentRecyclerView.GetChildLayoutPosition(view);
                    var folder = foldersInView[position];
                    ItemLongClicked(view, folder);
                };
                return folderViewHolder;
            }

            public void Refresh(List<Folder> folders)
            {
                foldersInView.Clear();
                foldersInView.AddRange(folders);
                NotifyDataSetChanged();
            }
        }

        class FolderViewHolder : RecyclerView.ViewHolder
        {

            public Button ExpandButton { get; private set; }
            public TextView FolderName { get; private set; }
            public ImageView FolderIcon { get; private set; }

            public event EventHandler<View> ExpandClicked = delegate { };
            public event EventHandler<View> ItemClicked = delegate { };
            public event EventHandler<View> ItemLongClicked = delegate { };

            public FolderViewHolder(View itemView) : base(itemView)
            {
                ExpandButton = itemView.FindViewById<Button>(Resource.Id.list_item_folder_expand);
                ExpandButton.Click += (sender, e) => ExpandClicked(this, itemView);

                FolderName = itemView.FindViewById<TextView>(Resource.Id.list_item_folder_name);
                FolderIcon = itemView.FindViewById<ImageView>(Resource.Id.list_item_folder_icon);

                var internalLayout = itemView.FindViewById<LinearLayoutCompat>(Resource.Id.list_item_folder_internal_Layout);
                internalLayout.Click += (sender, e) => ItemClicked(this, itemView);
                internalLayout.LongClick += (sender, e) => ItemLongClicked(this, itemView);
            }
        }

        #endregion

    }
}

