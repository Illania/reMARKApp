//
// Project: 
// File: CommunicationAddressesSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Linq;
using Android.Views;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.ContactView
{
    public class CommunicationAddressesSubview : ContactViewBaseListSubview
    {
        CommunicationAddressType addressType;

        public CommunicationAddressesSubview(Android.Content.Context context, CommunicationAddressType type) : base(context)
        {
            addressType = type;
            SetTitle("Communication Addresses");
        }

        public override void RefreshView()
        {
            if (Contact != null && Contact.CommunicationAddresses.Any())
            {
                Visibility = ViewStates.Visible;

                foreach (var address in Contact.CommunicationAddresses)
                {
                    address.ty
                }

            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }
    }
}
