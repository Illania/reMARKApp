//
// Project: Mark5.Mobile.Droid
// File: FoldersListFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Views.Fragments
{

    public class FoldersListFragment : Fragment
    {
        public IFoldersListFragmentSelectedListener Listener
        {
            get
            {
                return listener;
            }

            set
            {
                listener = value;
            }
        }

        FolderListAdapter adapter;
        RecyclerView recyclerView;

        IFoldersListFragmentSelectedListener listener;

        public static FoldersListFragment Create(int val)
        {
            var fragment = new FoldersListFragment();
            return fragment;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            adapter = new FolderListAdapter(Activity);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var rootView = inflater.Inflate(Resource.Layout.fragment_list_folders, container, false);

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.folderRecyclerView);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.SetAdapter(adapter);
            recyclerView.HasFixedSize = true;

            return rootView;
        }

        public async override void OnStart()
        {
            base.OnStart();

            var folders = await Managers.FoldersManager.GetFoldersAsync(ModuleType.Documents);
            adapter.Refresh(folders);
        }

        public interface IFoldersListFragmentSelectedListener
        {
            void OpenNext(int val);
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            //TODO to be completed
        }
    }

    class FolderListAdapter : RecyclerView.Adapter
    {
        readonly Android.App.Activity activity;
        List<Folder> folders = new List<Folder>();

        public FolderListAdapter(Android.App.Activity activity)
        {
            this.activity = activity;
        }

        public override int ItemCount
        {
            get
            {
                return folders.Count;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var fh = holder as FolderViewHolder;

            var folder = folders[position];

            fh.FolderName.Text = folder.Name;

            fh.ExpandIcon.Visibility = folder.HasSubFolders ? ViewStates.Visible : ViewStates.Invisible;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            // Inflate the CardView for the photo:
            View itemView = LayoutInflater.From(parent.Context).
                                          Inflate(Resource.Layout.folder_list_item, parent, false);

            // Create a ViewHolder to hold view references inside the CardView:
            var vh = new FolderViewHolder(itemView);
            return vh;
        }

        public void Refresh(List<Folder> folders)
        {
            this.folders.AddRange(folders);
            NotifyDataSetChanged();
        }
    }

    class FolderViewHolder : RecyclerView.ViewHolder
    {
        public ImageView ExpandIcon { get; private set; }
        public TextView FolderName { get; private set; }

        public FolderViewHolder(View itemView) : base(itemView)
        {
            // Locate and cache view references:
            ExpandIcon = itemView.FindViewById<ImageView>(Resource.Id.expandIcon);
            FolderName = itemView.FindViewById<TextView>(Resource.Id.folderName);
        }
    }


}

