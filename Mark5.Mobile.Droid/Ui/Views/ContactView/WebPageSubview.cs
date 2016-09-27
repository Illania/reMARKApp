//
// Project: 
// File: WebPageSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Android.Runtime;

namespace Mark5.Mobile.Droid.Ui.Views.ContactView
{
    [Register("ContactView.WebPageSubview")]
    public class WebPageSubview : ContactViewBaseTextSubview
    {
        public WebPageSubview(Android.Content.Context context) : base(context)
        {
            SetTitle("Web page");
        }

        public override void RefreshView()
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
