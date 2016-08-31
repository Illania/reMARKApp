//
// Project: Mark5.Mobile.Droid
// File: FoldersListFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

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

            var text = Activity.FindViewById<TextView>(Resource.Id.text);
            text.Text = Text.ToString();
        }
    }
}

