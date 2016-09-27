//
// Project: 
// File: VatSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Android.Runtime;
using Mark5.Mobile.Droid.Ui.Views.ContactView.BaseSubviews;

namespace Mark5.Mobile.Droid.Ui.Views.ContactView
{
    [Register("ContactView.VatSubview")]
    public class VatSubview : ContactViewBaseTextSubview
    {
        public VatSubview(Android.Content.Context context, Android.Util.IAttributeSet attrs) : base(context, attrs)
        {
            SetTitle("VAT");
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(Contact?.Vat))
            {
                SetVisibility(true);
                SetContent(Contact.Vat);
            }
            else
            {
                SetVisibility(false);
            }
        }
    }
}
