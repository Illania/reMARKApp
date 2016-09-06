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
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.Droid.Views.Fragments
{

    public class FoldersListFragment : Fragment
    {
        ModuleType moduleType;
        Folder currentFolder;

        FolderListAdapter adapter;
        RecyclerView recyclerView;
        SwipeRefreshLayout refreshLayout;
        List<Folder> savedFoldersInView = new List<Folder>();
        View rootView;

        const string ModuleTypeBundleString = "moduleTypeBundleString";
        const string CurrentFolderBundleString = "currentFolderBundleString";
        const string FoldersListBundleString = "foldersListBundleString";

        bool restored;

        #region Factory method

        public static FoldersListFragment Create(ModuleType moduleType, Folder currentFolder)
        {
            var fragment = new FoldersListFragment();
            var arguments = new Bundle();

            arguments.PutString(ModuleTypeBundleString, SerializationUtils.Serialize(moduleType));
            arguments.PutString(CurrentFolderBundleString, SerializationUtils.Serialize(currentFolder));
            fragment.Arguments = arguments;

            return fragment;
        }

        #endregion

        #region Overrides

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RecoverState(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (rootView != null)
            {
                return rootView;
            }

            rootView = inflater.Inflate(Resource.Layout.fragment_list_folders, container, false);

            refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
            refreshLayout.Refresh += RefreshLayout_Refresh;

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.folderRecyclerView);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.HasFixedSize = true;

            adapter = new FolderListAdapter(recyclerView);
            adapter.expandIconClicked += Adapter_ExpandIconClicked;
            adapter.folderNameClicked += Adapter_FolderNameClicked;
            adapter.itemLongClicked += Adapter_ItemLongClicked;

            recyclerView.SetAdapter(adapter);

            return rootView;
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            RecoverState(savedInstanceState);

            if (savedFoldersInView != null && savedFoldersInView.Any())
            {
                adapter.Refresh(savedFoldersInView);
            }

            var subtitle = currentFolder != null ? currentFolder.Name : string.Empty;
            (Activity as IFoldersListFragmentSelectedListener).SetTitles(moduleType.ToString(), subtitle);
        }

        public async override void OnStart()
        {
            base.OnStart();

            if (!restored)
            {
                await RefreshData();
            }
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            var foldersToSave = adapter != null ? adapter.foldersInView : savedFoldersInView;
            if (adapter != null)
            {
                outState.PutString(FoldersListBundleString, SerializationUtils.Serialize(foldersToSave));
            }
        }

        #endregion

        #region Utility methods

        async Task RefreshData()
        {
            refreshLayout.Post(() => refreshLayout.Refreshing = true); //Not a good way, but it's a bug, fixed in support library v 24.2.0 (issue 77712)

            var folders = await Managers.FoldersManager.GetFoldersAsync(moduleType, currentFolder);
            adapter.Refresh(folders);

            refreshLayout.Refreshing = false;
        }

        void RecoverState(Bundle savedInstanceState)
        {
            if (Arguments != null)
            {
                moduleType = SerializationUtils.Deserialize<ModuleType>(Arguments.GetString(ModuleTypeBundleString));
                currentFolder = SerializationUtils.Deserialize<Folder>(Arguments.GetString(CurrentFolderBundleString));
            }
            if (savedInstanceState != null)
            {
                restored = true;
                savedFoldersInView = SerializationUtils.Deserialize<List<Folder>>(savedInstanceState.GetString(FoldersListBundleString));
            }
        }

        #endregion

        #region List item event handlers

        void Adapter_ExpandIconClicked(object sender, Folder folder)
        {
            (Activity as IFoldersListFragmentSelectedListener).NavigateInFolder(moduleType, folder);
        }

        void Adapter_FolderNameClicked(object sender, Folder folder)
        {

        }

        void Adapter_ItemLongClicked(object sender, Folder folder)
        {
            var itemView = sender as View;
            itemView.Selected = true;
        }

        #endregion

        #region SwipeRefresLayout event handlers

        async void RefreshLayout_Refresh(object sender, EventArgs e)
        {
            await RefreshData();
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
        public event EventHandler<Folder> folderNameClicked = delegate { };
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
            fh.ExpandIcon.Visibility = folder.HasSubFolders ? ViewStates.Visible : ViewStates.Gone;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).
                                          Inflate(Resource.Layout.folder_list_item, parent, false);

            var folderViewHolder = new FolderViewHolder(itemView);
            folderViewHolder.expandIconClicked += FolderViewHolder_ExpandIconClicked;
            folderViewHolder.folderNameClicked += FolderViewHolder_FolderNameClicked;
            folderViewHolder.itemLongClicked += FolderViewHolder_ItemLongClicked;
            return folderViewHolder;
        }

        public void Refresh(List<Folder> folders)
        {
            foldersInView.Clear();
            foldersInView.AddRange(folders);
            NotifyDataSetChanged();
        }

        void FolderViewHolder_ExpandIconClicked(object sender, View view)
        {
            var position = parentView.GetChildLayoutPosition(view);
            var folder = foldersInView[position];
            expandIconClicked(view, folder);
        }

        void FolderViewHolder_FolderNameClicked(object sender, View view)
        {
            var position = parentView.GetChildLayoutPosition(view);
            var folder = foldersInView[position];
            folderNameClicked(view, folder);
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
        public ImageView ExpandIcon { get; private set; }
        public TextView FolderName { get; private set; }

        public event EventHandler<View> expandIconClicked = delegate { };
        public event EventHandler<View> folderNameClicked = delegate { };
        public event EventHandler<View> itemLongClicked = delegate { };

        public FolderViewHolder(View itemView) : base(itemView)
        {
            // Locate and cache view references
            ExpandIcon = itemView.FindViewById<ImageView>(Resource.Id.expandIcon);
            ExpandIcon.Click += (sender, e) => { expandIconClicked(this, itemView); };
            FolderName = itemView.FindViewById<TextView>(Resource.Id.folderName);
            itemView.Click += (sender, e) => { folderNameClicked(this, itemView); };
            itemView.LongClick += (sender, e) => itemLongClicked(this, itemView);
        }
    }


}

