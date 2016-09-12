//
// Project: Mark5.Mobile.Droid
// File: FoldersListFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common.Model;
using Android.Content;
using Mark5.Mobile.Droid.Views.Activities;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.Droid.Views.Fragments
{

    public class FoldersListFragment : Fragment
    {

        public int Text
        {
            get;
            set;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.fragment_list_folders, container, false);
        }

        public override void OnStart()
        {
            base.OnStart();

            var btn = Activity.FindViewById<Button>(Resource.Id.btn1);
            btn.Text = "OpenFolder";
            btn.Click += (sender, e) =>
            {
                var f = new Folder
                {
                    Id = 4,
                    Name = "Browse (all)",
                };

                var i = new Intent(Activity, typeof(DocumentsListActivity));
                i.PutExtra(DocumentsListActivity.FolderIntentKey, SerializationUtils.Serialize(f));
                StartActivity(i);
            };
        }
    }
}

