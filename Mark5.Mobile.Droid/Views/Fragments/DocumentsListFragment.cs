//
// Project: Mark5.Mobile.Droid
// File: DocumentsListFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Views.Common;

namespace Mark5.Mobile.Droid.Views.Fragments
{

    public class DocumentsListFragment : RetainableStateFragment
    {

        public Folder Folder
        {
            get;
            set;
        }

        public static DocumentsListFragment Create(Folder folder)
        {
            return new DocumentsListFragment
            {
                Folder = folder
            };
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            ((AppCompatActivity)Activity).SupportActionBar.Title = Folder?.Name;
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = GetString(Resource.String.documents);

            return inflater.Inflate(Resource.Layout.fragment_list_documents, container, false);
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var dlfs = restoredState as DocumentsListFragmentState;
            if (dlfs != null)
            {
                Folder = dlfs.Folder;

                ((AppCompatActivity)Activity).SupportActionBar.Title = Folder?.Name;
                ((AppCompatActivity)Activity).SupportActionBar.Subtitle = GetString(Resource.String.documents);
            }
        }

        public override IRetainableState OnRetainInstanceState()
        {
            return new DocumentsListFragmentState
            {
                Folder = Folder
            };
        }

        public string GenerateTag()
        {
            return $"{nameof(DocumentsListFragment)} [FolderId={Folder.Id}, FolderName={Folder.Name}]";
        }

        class DocumentsListFragmentState : IRetainableState
        {

            public Folder Folder { get; set; }
        }
    }
}

