//
// Project: Mark5.Mobile.Droid
// File: BirthdateSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public class BirthdateSubview : DescriptionCardSubview
    {
        public BirthdateSubview(Android.Content.Context context) : base(context)
        {
            Title = "Birthdate";
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(Contact?.Ledger))
            {
                Visibility = ViewStates.Visible;
                Content = Contact.Ledger;
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }
    }
}
