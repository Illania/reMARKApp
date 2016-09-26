//
// Project: 
// File: DescriptionSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Android.Runtime;

namespace Mark5.Mobile.Droid.UI.Views.ContactView.BaseSubviews
{
    public class DescriptionSubview : ContactViewBaseTextSubview
    {
        [Register("contactView.descriptionSubView")]
        public DescriptionSubview(Android.Content.Context context, Android.Util.IAttributeSet attrs) : base(context, attrs)
        {
        }

        public override void UpdateView()
        {
            SetTitle("Description");
            SetContent(ContactPreview.Description);
        }
    }
}
