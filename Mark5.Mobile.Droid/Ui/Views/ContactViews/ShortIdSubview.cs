//
// Project: 
// File: ShortIdSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public class ShortIdSubview : ContactTextSubview
    {
        public ShortIdSubview(Context context) :
            base(context)
        {
            SetTitle("Short Id"); //TODO check
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(ContactPreview?.ShortId))
            {
                Visibility = ViewStates.Visible;
                SetContent(ContactPreview?.ShortId);
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }

    }
}
