//
// Project: Mark5.Mobile.IOS
// File: ToView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.Subviews
{
    //TODO do we consider the reply to in Android?
    public class ToView : RecipientsView
    {
        public ToView() : base(DocumentAddressType.To)
        {
        }
    }
}
