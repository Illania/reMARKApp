//
// Project: 
// File: DescriptionSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public class DescriptionSubview : ContactViewBaseTextSubview
    {
        public DescriptionSubview(Android.Content.Context context) : base(context)
        {
            SetTitle("Description");
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(ContactPreview?.Description))
            {
                Visibility = ViewStates.Visible;
                SetContent(ContactPreview.Description);
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }
    }
}
