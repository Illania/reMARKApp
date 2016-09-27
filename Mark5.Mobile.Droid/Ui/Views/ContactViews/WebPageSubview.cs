//
// Project: 
// File: WebPageSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public class WebPageSubview : ContactTextSubview
    {
        public WebPageSubview(Android.Content.Context context) : base(context)
        {
            SetTitle("Web page");
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(Contact?.WebPageAddress))
            {
                Visibility = ViewStates.Visible;
                SetContent(Contact.WebPageAddress);
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }
    }
}
