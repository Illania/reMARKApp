//
// Project: Mark5.Mobile.Droid
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
        public LedgerSubview(Context context)
            : base(context)
        {
            Title = context.GetString(Resource.String.ledger);
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