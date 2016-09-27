//
// Project: 
// File: BirthdateSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Android.Runtime;
using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Views.ContactView
{
    [Register("ContactView.BirthdateSubview")]
    public class BirthdateSubview : ContactViewBaseTextSubview
    {
        public BirthdateSubview(Android.Content.Context context) : base(context)
        {
            SetTitle("Birthdate");
        }

        public override void RefreshView()
        {
            if (Contact?.BirthDate != null && Contact.BirthDate != default(DateTime))
            {
                Visibility = ViewStates.Visible;
                SetContent(Contact.BirthDate.ToString()); //TODO need to write it in a better way
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }
    }
}
