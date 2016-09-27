//
// Project: 
// File: VatSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public class VatSubview : ContactTextSubview
    {
        public VatSubview(Android.Content.Context context) : base(context)
        {
            SetTitle("VAT");
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(Contact?.Vat))
            {
                Visibility = ViewStates.Visible;
                SetContent(Contact.Vat);
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }
    }
}
