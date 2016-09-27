//
// Project: 
// File: AccountSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Views.ContactView
{
    public class AccountSubview : ContactViewBaseTextSubview
    {
        public AccountSubview(Android.Content.Context context) : base(context)
        {
            SetTitle("Account");
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(Contact?.Account))
            {
                Visibility = ViewStates.Visible;
                SetContent(Contact.Account);
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }
    }
}
