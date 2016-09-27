//
// Project: 
// File: WebPageSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Android.Runtime;
using Mark5.Mobile.Droid.Ui.Views.ContactView.BaseSubviews;

namespace Mark5.Mobile.Droid.Ui.Views.ContactView
{
    [Register("ContactView.WebPageSubview")]
    public class WebPageSubview : ContactViewBaseTextSubview
    {
        public WebPageSubview(Android.Content.Context context, Android.Util.IAttributeSet attrs) : base(context, attrs)
        {
            SetTitle("Web page");
        }

        public override void UpdateView()
        {
            if (!string.IsNullOrEmpty(Contact?.WebPageAddress))
            {
                SetVisibility(true);
                SetContent(Contact.WebPageAddress);
            }
            else
            {
                SetVisibility(false);
            }
        }
    }
}
