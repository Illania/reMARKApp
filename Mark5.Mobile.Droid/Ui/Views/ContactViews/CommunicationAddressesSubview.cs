//
// Project: 
// File: CommunicationAddressesSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Linq;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public class CommunicationAddressesSubview : ContactContentSubview
    {
        CommunicationAddressType addressType;

        public CommunicationAddressesSubview(Android.Content.Context context, CommunicationAddressType type) : base(context)
        {
            addressType = type;
            SetTitle(type.ToString());
        }

        public override void RefreshView()
        {
            var communicationAddressesForType = Contact?.CommunicationAddresses.Where(ca => ca.Type == addressType);
            if (Contact != null && communicationAddressesForType.Any())
            {
                Visibility = ViewStates.Visible;

                foreach (var address in communicationAddressesForType)
                {
                    var subsubview = new CommunicationAddressesSubSubview(Context, address);
                    AddView(subsubview);
                }
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }
    }

    class CommunicationAddressesSubSubview : LinearLayoutCompat
    {
        public CommunicationAddressesSubSubview(Android.Content.Context context, CommunicationAddress communicationAddress) : base(context)
        {
            Orientation = Horizontal;
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

            var addressTextView = new AppCompatTextView(context);
            addressTextView.Text = communicationAddress.Address;

            var descriptionTextView = new AppCompatTextView(context);
            descriptionTextView.Text = communicationAddress.Description;

            AddView(addressTextView);
            AddView(descriptionTextView);
        }
    }
}
