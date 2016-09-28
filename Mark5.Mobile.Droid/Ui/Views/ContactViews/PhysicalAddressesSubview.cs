//
// Project: 
// File: PhysicalAddressesSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Linq;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public class PhysicalAddressesSubview : ContactContentSubview
    {

        public PhysicalAddressesSubview(Android.Content.Context context) : base(context)
        {
            SetTitle("Addresses"); //TODO check
        }

        public override void RefreshView()
        {
            if (Contact != null && Contact.PhysicalAddresses.Any())
            {
                Visibility = ViewStates.Visible;

                foreach (var address in Contact.PhysicalAddresses)
                {
                    var subsubview = new PhysicalAddressesSubSubview(Context, address);
                    contentLayout.AddView(subsubview);
                }
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }
    }

    class PhysicalAddressesSubSubview : LinearLayoutCompat
    {
        public PhysicalAddressesSubSubview(Android.Content.Context context, PhysicalAddress physicalAddress) : base(context)
        {
            Orientation = Horizontal;
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            var paddingValue = ConversionUtils.ConvertDpToPixels(4);
            SetPadding(0, paddingValue, paddingValue, paddingValue);

            var typeTextView = new AppCompatTextView(context);
            typeTextView.Text = physicalAddress.Type.Name;
            AddView(typeTextView, new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent));

            var addressTextView = new AppCompatTextView(context);
            var formattedAddress = $"{physicalAddress.Country} - {physicalAddress.City}, {physicalAddress.Area}, {physicalAddress.Street}, {physicalAddress.ZipCode}"; //TODO need to do a good formatting
            AddView(addressTextView, new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent, 1.0f));
            addressTextView.Text = formattedAddress;

            var button = new AppCompatImageView(context);
            button.SetImageResource(Resource.Drawable.common_plus_signin_btn_icon_dark_normal);
            AddView(button, new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent));
        }
    }
}
