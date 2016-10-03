//
// Project: 
// File: LedgerSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

using Android.Content;
using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public class LedgerSubview : DescriptionCardSubview
    {
        public LedgerSubview(Context context) :
            base(context)
        {
            SetTitle("Ledger");
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
