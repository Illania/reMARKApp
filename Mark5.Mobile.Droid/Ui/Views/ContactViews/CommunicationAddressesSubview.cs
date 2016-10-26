//
// Project: Mark5.Mobile.Droid
// File: CommunicationAddressesSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Linq;
using System.Text;
using Android.Support.V4.Content;
using Android.Views;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public class CommunicationAddressesSubview : CommunicationCardSubview
    {
        CommunicationAddressType addressType;

        public event EventHandler<CommunicationAddress> AddressClicked = delegate { };

        public CommunicationAddressesSubview(Android.Content.Context context, CommunicationAddressType type) : base(context)
        {
            addressType = type;
            Title = type.ToString();
            IconImageView.SetImageResource(Resource.Drawable.email);
        }

        public override void RefreshView()
        {
            if (Contact.PreferrableType == addressType)
            {
                IconImageView.SetColorFilter(new Android.Graphics.Color(ContextCompat.GetColor(Context, Resource.Color.brown)));
            }

            var communicationAddressesForType = Contact?.CommunicationAddresses.Where(ca => ca.Type == addressType);
            if (Contact != null && communicationAddressesForType.Any())
            {
                ContentLayout.RemoveAllViews();
                Visibility = ViewStates.Visible;

                foreach (var communicationAddress in communicationAddressesForType.OrderBy(ad => ad.IsPrimary != true))
                {
                    if (addressType == CommunicationAddressType.IM)
                    {
                        var handleAndType = ParseIm(communicationAddress.Address);
                        var subsubview = new CommunicationAddressesSubSubview(Context, this, communicationAddress, handleAndType.Item1, handleAndType.Item2);
                        ContentLayout.AddView(subsubview);
                    }
                    else
                    {
                        var formattedAddress = FormatAddress(communicationAddress.Address);
                        var subsubview = new CommunicationAddressesSubSubview(Context, this, communicationAddress, formattedAddress, communicationAddress.Description);
                        ContentLayout.AddView(subsubview);
                    }
                }
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }

        string FormatAddress(string address)
        {
            if (new[] { CommunicationAddressType.Fax, CommunicationAddressType.Phone, CommunicationAddressType.Mobile, CommunicationAddressType.Telex }.Contains(addressType))
            {
                var stringBuilder = new StringBuilder();
                var addressParts = address.Split('|');

                var countryPrefix = addressParts[0];
                var firstPart = addressParts[1];
                var secondPart = addressParts[2];

                if (!string.IsNullOrEmpty(countryPrefix))
                {
                    stringBuilder.Append($"+{countryPrefix} ");
                }
                if (!string.IsNullOrEmpty(firstPart))
                {
                    stringBuilder.Append($"{firstPart} ");
                }
                if (!string.IsNullOrEmpty(secondPart))
                {
                    stringBuilder.Append(secondPart);
                }

                return stringBuilder.ToString();
            }
            return address;

        }

        Tuple<string, string> ParseIm(string address)
        {
            var addressParts = address.Split('|');

            var imHandle = addressParts[0];
            var imTypeIndex = short.Parse(addressParts[1]);

            string imTypeString = string.Empty;

            switch (imTypeIndex)
            {
                case 0:
                    imTypeString = "Other";
                    break;
                case 1:
                    imTypeString = "MSN";
                    break;
                case 2:
                    imTypeString = "Yahoo";
                    break;
                case 3:
                    imTypeString = "ICQ";
                    break;
            }

            return Tuple.Create(imHandle, imTypeString);
        }

        class CommunicationAddressesSubSubview : CommunicationCardSubSubview
        {
            public CommunicationAddressesSubSubview(Android.Content.Context context, CommunicationAddressesSubview parentView, CommunicationAddress communicationAddress, string address, string description)
                : base(context, address, description, communicationAddress.IsPrimary)
            {
                Click += (sender, e) => parentView.AddressClicked(this, communicationAddress);
            }
        }
    }

}
