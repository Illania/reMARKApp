//
// Project: Mark5.Mobile.Droid
// File: BccView.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public class BccView : RecipientsView
    {
        public BccView(Context context)
            : base(context, DocumentAddressType.Bcc)
        {

        }
    }
}
