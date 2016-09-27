//
// Project: 
// File: IContactView.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.ContactView
{
    interface IContactSubview
    {
        ContactPreview ContactPreview { get; set; }
        Contact Contact { get; set; }

        void RefreshView();
    }
}
