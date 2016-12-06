//
// Project: Mark5.Mobile.Droid
// File: PhysicalAddressesSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Linq;
using System.Text;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.Common;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{

    public class PhysicalAddressesSubview : ContactView
    {
        public event EventHandler<PhysicalAddress> PhysicalAddressClicked = delegate { };

        public PhysicalAddressesSubview(Context context)
            : base(context)
        {
            Orientation = Vertical;
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            SetPadding(DistanceVeryLarge, 0, 0, 0);
        }

        public override void RefreshView()
        {
            if (Contact != null && Contact.PhysicalAddresses.Any())
            {
                Visibility = ViewStates.Visible;

                RemoveAllViews();
                var lastAddress = Contact.PhysicalAddresses.Last();
                foreach (var address in Contact.PhysicalAddresses)
                {
                    var subview = new PhysicalAddressesSubSubview(Context, address, DistanceSmall, DistanceLarge, DistanceVeryLarge, lastAddress != address);
                    subview.Click += (sender, e) => PhysicalAddressClicked(this, address);
                    AddView(subview);
                }
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }

        class PhysicalAddressesSubSubview : LinearLayoutCompat
        {

            public PhysicalAddressesSubSubview(Context context, PhysicalAddress physicalAddress, int distanceSmall, int distanceLarge, int distanceVeryLarge, bool addDivider = false)
                : base(context)
            {
                Orientation = Vertical;
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

                var typedArray = Context.ObtainStyledAttributes(new int[] { Resource.Attribute.selectableItemBackground });
                SetBackgroundResource(typedArray.GetResourceId(0, 0));
                typedArray.Recycle();

                Clickable = true;

                var typeTextView = new AppCompatTextView(context);
                typeTextView.SetPadding(0, 0, distanceVeryLarge, 0);
                typeTextView.SetTextAppearanceCompat(context, Resource.Style.fontSmallLight);
                typeTextView.Text = physicalAddress.Type.Name;
                var typeTextViewLayoutParams = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                typeTextViewLayoutParams.TopMargin = distanceLarge;
                AddView(typeTextView, typeTextViewLayoutParams);

                var addressTextView = new AppCompatTextView(context);
                addressTextView.SetPadding(0, 0, distanceVeryLarge, 0);
                addressTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
                addressTextView.Text = GetAddressText(physicalAddress);
                var addressTextViewLayoutParams = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                addressTextViewLayoutParams.TopMargin = distanceSmall;
                addressTextViewLayoutParams.BottomMargin = distanceLarge;
                AddView(addressTextView, addressTextViewLayoutParams);

                if (addDivider)
                {
                    var divider = new Divider(context);
                    AddView(divider);
                }
            }

            string GetAddressText(PhysicalAddress address)
            {
                var sb = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(address.Street))
                {
                    sb.Append(address.Street);
                }
                if (!string.IsNullOrWhiteSpace(address.Area))
                {
                    sb.AppendLine().Append(address.Area);
                }
                if (!string.IsNullOrWhiteSpace(address.ZipCode))
                {
                    sb.AppendLine().Append(address.ZipCode);
                }
                if (!string.IsNullOrWhiteSpace(address.City))
                {
                    sb.Append(" ").Append(address.City);
                }
                if (address.Country != null && address.Country.Id != 0)
                {
                    sb.AppendLine().Append(address.Country.Name);
                }
                return sb.ToString();
            }
        }
    }
}
