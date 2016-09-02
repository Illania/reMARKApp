//
// Project: Mark5.Mobile.Droid
// File: FoldersListFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;

namespace Mark5.Mobile.Droid.Views.Fragments
{

    public class FoldersListFragment : Fragment
    {
        public int Val
        {
            get;
            set;
        }

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

        IFoldersListFragmentSelectedListener listener;

        public static FoldersListFragment Create(int val)
        {
            var fragment = new FoldersListFragment();
            var args = new Bundle();
            args.PutInt("val", val);
            fragment.Arguments = args;
            return fragment;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (savedInstanceState != null)
            {
                Val = savedInstanceState.GetInt("val");
            }
            if (Arguments != null)
            {
                Val = Arguments.GetInt("val");
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.fragment_list_folders, container, false);
        }

        public override void OnStart()
        {
            base.OnStart();

            var text = View.FindViewById<TextView>(Resource.Id.textView1);
            text.Text = Val.ToString();
            var button = View.FindViewById<Button>(Resource.Id.button1);
            button.Click += Button_Click;
        }

        void Button_Click(object sender, EventArgs e)
        {
            Listener.OpenNext(Val + 1);

        }

        public interface IFoldersListFragmentSelectedListener
        {
            void OpenNext(int val);
        }

        public override void OnStop()
        {
            base.OnStop();
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            outState.PutInt("val", Val);
        }

    }


}

