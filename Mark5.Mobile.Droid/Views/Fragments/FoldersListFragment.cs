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
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Views.Common;

namespace Mark5.Mobile.Droid.Views.Fragments
{

    public class FoldersListFragment : Fragment, ActionMode.ICallback
    {
        public ModuleType moduleType
        {
            get
            {
                return stateFragment.State.Module;
            }
        }

        public Folder currentFolder
        {
            get
            {
                return stateFragment.State;
            }
        }

        FolderListAdapter adapter;
        RecyclerView recyclerView;
        SwipeRefreshLayout refreshLayout;

        IFoldersListFragmentSelectedListener listener;

        RetainStateFragment<Folder> stateFragment;
        string stateFragmentTag;

        const string StateFragmentTagBundleKey = "StateFragmentTagBundleKey";

        #region Factory method

        public static FoldersListFragment Create(FragmentManager fm, ModuleType moduleType, Folder currentFolder)
        {
            var fragment = new FoldersListFragment();

            if (currentFolder == null)
            {
                currentFolder = Folder.RootPerModule(moduleType);
            }

            var tag = $"{typeof(FoldersListFragment)}{moduleType}_{currentFolder.Id}";
            fragment.stateFragmentTag = tag;

            bool stateFragmentCreated;
            fragment.stateFragment = RetainStateFragment<Folder>.FindOrCreate(fm, tag, out stateFragmentCreated);
            if (stateFragmentCreated)
            {
                fragment.stateFragment.SetState(currentFolder);
            }

            return fragment;
        }

        #endregion

        #region Overrides

        public override void OnAttach(Android.Content.Context context)
        {
            base.OnAttach(context);
            listener = context as IFoldersListFragmentSelectedListener;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var rootView = inflater.Inflate(Resource.Layout.fragment_list_folders, container, false);

            refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
            refreshLayout.Refresh += RefreshLayout_Refresh;

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.folderRecyclerView);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.HasFixedSize = true;

            adapter = new FolderListAdapter(recyclerView);
            adapter.expandIconClicked += Adapter_ExpandClicked;
            adapter.itemClicked += Adapter_ItemClicked;
            adapter.itemLongClicked += Adapter_ItemLongClicked;

            recyclerView.SetAdapter(adapter);

            return rootView;
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            if (stateFragment == null)
            {
                bool _fragmentCreated;
                stateFragmentTag = savedInstanceState.GetString(StateFragmentTagBundleKey);
                stateFragment = RetainStateFragment<Folder>.FindOrCreate(Activity.SupportFragmentManager, stateFragmentTag, out _fragmentCreated);
            }
        }

        public async override void OnStart()
        {
            base.OnStart();

            SetTitles();
            await RefreshData();
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutString(StateFragmentTagBundleKey, stateFragmentTag);
        }

        protected override void JavaFinalize()
        {
            CommonConfig.Logger.Error("LOOK AT MEEEEEEE" + stateFragmentTag);
            base.JavaFinalize();
        }

        #endregion

        #region Utility methods

        async Task RefreshData(bool forceRefresh = false)
        {
            if (!currentFolder.HasSubFolders)
            {
                return; //TODO could it happen?
            }

            if (forceRefresh || !currentFolder.SubFolders.Any())
            {
                refreshLayout.Post(() => refreshLayout.Refreshing = true); //Not a good way, but it's a bug, fixed in support library v 24.2.0 (issue 77712)

                var folders = await Managers.FoldersManager.GetFoldersAsync(moduleType, currentFolder.Root ? null : currentFolder, 0); //TODO do we do this check here or in the manager?
                currentFolder.SubFolders.Clear();
                currentFolder.SubFolders = folders;

                adapter.Refresh(folders);
                refreshLayout.Post(() => refreshLayout.Refreshing = false); //Not a good way, but it's a bug, fixed in support library v 24.2.0 (issue 77712)
            }
            else
            {
                adapter.Refresh(currentFolder.SubFolders);
            }
        }

        void SetTitles()
        {
            var subtitle = currentFolder.Root ? string.Empty : currentFolder.Name;
            listener.SetTitles(moduleType.ToString(), subtitle);
        }

        #endregion

        #region List item event handlers

        void Adapter_ExpandClicked(object sender, Folder folder)
        {
            listener.NavigateInFolder(moduleType, folder);
        }

        void Adapter_ItemClicked(object sender, Folder folder)
        {

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

        ActionMode actionMode;

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

        #region Listener interface definition

        public interface IFoldersListFragmentSelectedListener
        {
            void NavigateInFolder(ModuleType moduleType, Folder folder);
            void SetTitles(string title, string subtitle);
        }

        #endregion

    }

    class FolderListAdapter : RecyclerView.Adapter
    {
        public readonly List<Folder> foldersInView = new List<Folder>();
        readonly RecyclerView parentView;

        public event EventHandler<Folder> expandIconClicked = delegate { };
        public event EventHandler<Folder> itemClicked = delegate { };
        public event EventHandler<Folder> itemLongClicked = delegate { };

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
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).
                                          Inflate(Resource.Layout.folder_list_item, parent, false);

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
            var folder = foldersInView[position];
            expandIconClicked(view, folder);
        }

        void FolderViewHolder_ItemClicked(object sender, View view)
        {
            var position = parentView.GetChildLayoutPosition(view);
            var folder = foldersInView[position];

            itemClicked(view, folder);
        }

        void FolderViewHolder_ItemLongClicked(object sender, View view)
        {
            var position = parentView.GetChildLayoutPosition(view);
            var folder = foldersInView[position];
            itemLongClicked(view, folder);
        }
    }

    class FolderViewHolder : RecyclerView.ViewHolder
    {
        public LinearLayoutCompat ExpandButtonLayout { get; private set; }
        public TextView FolderName { get; private set; }
        public ImageView FolderIcon { get; private set; }

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
        }
    }
}

