//
// Project: Mark5.Mobile.IOS
// File: BccView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//

using System;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.Subviews
{
    public class BccView : RecipientsView
    {
        public BccView()
            : base(DocumentAddressType.Bcc)
        {
        }
    }
}