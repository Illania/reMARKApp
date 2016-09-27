//
// Project: 
// File: ContactViewBaseListSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Android.Support.V7.Widget;
using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Views.ContactView
{
    public class ContactViewBaseListSubview : ContactViewBaseSubview
    {
        protected LinearLayoutCompat contentLayout;

        public ContactViewBaseListSubview(Android.Content.Context context) : base(context)
        {
            InitializeView();
        }

        void InitializeView()
        {
            contentLayout = new LinearLayoutCompat(Context);
            contentLayout.SetPadding(20, 20, 20, 20); //TODO put the right valued
            contentLayout.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            AddView(contentLayout);

            AddView(separatorView);
        }

    }
}
