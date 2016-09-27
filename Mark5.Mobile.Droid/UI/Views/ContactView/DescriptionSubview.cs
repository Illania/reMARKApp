//
// Project: 
// File: DescriptionSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Android.Runtime;
using Mark5.Mobile.Droid.Ui.Views.ContactView.BaseSubviews;

namespace Mark5.Mobile.Droid.Ui.Views.ContactView
{
    [Register("ContactView.DescriptionSubview")]
    public class DescriptionSubview : ContactViewBaseTextSubview
    {
        public DescriptionSubview(Android.Content.Context context, Android.Util.IAttributeSet attrs) : base(context, attrs)
        {
            SetTitle("Description");
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(ContactPreview?.Description))
            {
                SetVisibility(true);
                SetContent(ContactPreview.Description);
            }
            else
            {
                SetVisibility(false);
            }
        }
    }
}
