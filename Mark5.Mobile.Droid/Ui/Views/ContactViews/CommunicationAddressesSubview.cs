//
// Project: 
// File: CommunicationAddressesSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System.Linq;
using Android.Views;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public class CommunicationAddressesSubview : CommunicationCardSubview
    {
        CommunicationAddressType addressType;

        public CommunicationAddressesSubview(Android.Content.Context context, CommunicationAddressType type) : base(context)
        {
            addressType = type;
            SetTitle(type.ToString());
            iconImageView.SetImageResource(Resource.Drawable.email);
        }

        public override void RefreshView()
        {
            var communicationAddressesForType = Contact?.CommunicationAddresses.Where(ca => ca.Type == addressType);
            if (Contact != null && communicationAddressesForType.Any())
            {
                Visibility = ViewStates.Visible;

                foreach (var address in communicationAddressesForType)
                {
                    var subsubview = new CommunicationCardSubSubview(Context, address.Address, address.Description);
                    contentLayout.AddView(subsubview);
                }
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }

    }

}
