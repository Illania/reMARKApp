//
// Project: 
// File: BirthdateSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public class BirthdateSubview : ContactViewBaseTextSubview
    {
        public BirthdateSubview(Android.Content.Context context) : base(context)
        {
            SetTitle("Birthdate");
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(Contact?.Ledger))
            {
                Visibility = ViewStates.Visible;
                SetContent(Contact.Ledger);
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }
    }
}
