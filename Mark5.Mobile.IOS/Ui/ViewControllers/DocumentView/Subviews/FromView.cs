//
// Project: Mark5.Mobile.IOS
// File: FromView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//

using System;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.Subviews
{
    public class FromView : RecipientsView
    {
        public FromView()
            : base(DocumentAddressType.From)
        {
        }
    }
}