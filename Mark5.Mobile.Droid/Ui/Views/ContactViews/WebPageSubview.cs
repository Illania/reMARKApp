//
// Project: Mark5.Mobile.Droid
// File: WebPageSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{

    public class WebPageSubview : DescriptionCardSubview
    {

        public WebPageSubview(Context context) : base(context)
        {
            Title = context.GetString(Resource.String.webpage);
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrWhiteSpace(Contact?.WebPageAddress))
            {
                Visibility = ViewStates.Visible;
                Content = Contact.WebPageAddress;
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }
    }
}
