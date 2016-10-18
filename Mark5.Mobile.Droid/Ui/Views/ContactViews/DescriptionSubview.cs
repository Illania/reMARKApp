//
// Project: Mark5.Mobile.Droid
// File: DescriptionSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public class DescriptionSubview : DescriptionCardSubview
    {
        public DescriptionSubview(Android.Content.Context context) : base(context)
        {
            Title = "Description";
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(ContactPreview?.Description))
            {
                Visibility = ViewStates.Visible;
                Content = ContactPreview.Description;
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }
    }
}
