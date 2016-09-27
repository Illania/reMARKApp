//
// Project: 
// File: AccountSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Android.Runtime;
using Mark5.Mobile.Droid.Ui.Views.ContactView.BaseSubviews;

namespace Mark5.Mobile.Droid.Ui.Views.ContactView
{
    [Register("ContactView.AccountSubview")]
    public class AccountSubview : ContactViewBaseTextSubview
    {
        public AccountSubview(Android.Content.Context context, Android.Util.IAttributeSet attrs) : base(context, attrs)
        {
            SetTitle("Account");
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(Contact?.Account))
            {
                SetVisibility(true);
                SetContent(Contact.Account);
            }
            else
            {
                SetVisibility(false);
            }
        }
    }
}
