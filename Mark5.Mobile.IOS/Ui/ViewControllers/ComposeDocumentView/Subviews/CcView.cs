//
// Project: Mark5.Mobile.IOS
// File: CcView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentViews.Subviews
{
    public class CcView : RecipientsView
    {
        public CcView()
            : base(DocumentAddressType.Cc)
        {
        }
    }
}