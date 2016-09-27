//
// Project: 
// File: VatSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Android.Runtime;
using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Views.ContactView
{
    [Register("ContactView.VatSubview")]
    public class VatSubview : ContactViewBaseTextSubview
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
