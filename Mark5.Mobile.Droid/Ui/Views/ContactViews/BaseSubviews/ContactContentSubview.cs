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

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public class ContactContentSubview : ContactSubView
    {
        protected LinearLayoutCompat contentLayout;

        public ContactContentSubview(Android.Content.Context context) : base(context)
        {
            contentLayout = new LinearLayoutCompat(Context);
            contentLayout.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            internalLayout.AddView(contentLayout);
        }
    }
}
