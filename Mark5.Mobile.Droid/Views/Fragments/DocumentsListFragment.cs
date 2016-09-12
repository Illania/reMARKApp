//
// Project: Mark5.Mobile.Droid
// File: DocumentsListFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.OS;
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
            return inflater.Inflate(Resource.Layout.fragment_list_documents, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var dlfs = restoredState as DocumentsListFragmentState;
            if (dlfs != null)
            {
                Folder = dlfs.Folder;
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

