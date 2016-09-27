//
// Project: 
// File: BirthdateSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Android.Runtime;
using Mark5.Mobile.Droid.Ui.Views.ContactView.BaseSubviews;

namespace Mark5.Mobile.Droid.Ui.Views.ContactView
{
    [Register("ContactView.BirthdateSubview")]
    public class BirthdateSubview : ContactViewBaseTextSubview
    {
        public BirthdateSubview(Android.Content.Context context, Android.Util.IAttributeSet attrs) : base(context, attrs)
        {
            SetTitle("Birthdate");
        }

        public override void RefreshView()
        {
            if (Contact?.BirthDate != null && Contact.BirthDate != default(DateTime))
            {
                SetVisibility(true);
                SetContent(Contact.BirthDate.ToString()); //TODO need to write it in a better way
            }
            else
            {
                SetVisibility(false);
            }
        }
    }
}
