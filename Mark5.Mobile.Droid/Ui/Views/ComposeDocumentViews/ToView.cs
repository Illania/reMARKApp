//
// Project: Mark5.Mobile.Droid
// File: ToView.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public class ToView : RecipientsView
    {
        public ToView(Context context)
            : base(context, DocumentAddressType.To)
        {

        }
    }
}
