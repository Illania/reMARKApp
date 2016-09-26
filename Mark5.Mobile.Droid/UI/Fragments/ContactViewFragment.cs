//
// Project: 
// File: ContactViewFragment.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.UI.Views.ContactView.BaseSubviews;
using Mark5.Mobile.Droid.Views.Common;

namespace Mark5.Mobile.Droid.Views.Fragments
{
    public class ContactViewFragment : RetainableStateFragment
    {
        public ContactPreview ContactPreview { get; set; }
        public Contact Contact { get; set; }

        public override Android.Views.View OnCreateView(Android.Views.LayoutInflater inflater, Android.Views.ViewGroup container, Android.OS.Bundle savedInstanceState)
        {
            var rootView = inflater.Inflate(Resource.Layout.contact_view, container, false);
            var baseView = rootView.FindViewById<ContactViewBaseTextSubview>(Resource.Id.base_view);

            baseView.SetContent("content");
            baseView.SetTitle("title");

            return rootView;
        }

        public override string GenerateTag()
        {
            return $"{nameof(ContactViewFragment)} [contactPreview.id={ContactPreview.Id}, contactPreview.name={ContactPreview.Name}]";
        }
    }
}
