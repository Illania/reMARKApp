//
// Project: Mark5.Mobile.IOS
// File: RecipientsView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.Subviews
{
    public class RecipientsView : DocumentView
    {
        protected DocumentAddressType AddressType;

        public RecipientsView(DocumentAddressType type)
        {
            //constraintsStash = new Dictionary<UIView, NSLayoutConstraint[]>();
            AddressType = type;
            //Initialize();
        }

        public override Task RefreshView()
        {
            throw new NotImplementedException();
        }
    }
}
