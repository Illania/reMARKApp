//
// Project: 
// File: PhysicalAddressesSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System.Linq;
using System.Text;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public class PhysicalAddressesSubview : ContactView
    {
        public PhysicalAddressesSubview(Android.Content.Context context) : base(context)
        {
            Orientation = Vertical;
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            SetPadding(ConversionUtils.ConvertDpToPixels(24), 0, 0, 0);
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
                    var subsubview = new PhysicalAddressesSubSubview(Context, address, lastAddress != address);
                    AddView(subsubview);
                }
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }

        class PhysicalAddressesSubSubview : LinearLayoutCompat
        {
            public PhysicalAddressesSubSubview(Android.Content.Context context, PhysicalAddress physicalAddress, bool addDivider = true) : base(context)
            {
                Orientation = Vertical;
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

                var addressTextView = new AppCompatTextView(context);
                addressTextView.SetPadding(0, 0, ConversionUtils.ConvertDpToPixels(24), 0);
                addressTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
                addressTextView.Text = GetAddressText(physicalAddress);
                var addressTextViewLayoutParams = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                addressTextViewLayoutParams.TopMargin = ConversionUtils.ConvertDpToPixels(16);
                AddView(addressTextView, addressTextViewLayoutParams);

                var typeTextView = new AppCompatTextView(context);
                typeTextView.SetPadding(0, 0, ConversionUtils.ConvertDpToPixels(24), 0);
                typeTextView.SetTextAppearanceCompat(context, Resource.Style.fontSmallLight);
                typeTextView.Text = physicalAddress.Type.Name;
                var typeTextViewLayoutParams = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                typeTextViewLayoutParams.TopMargin = ConversionUtils.ConvertDpToPixels(3);
                typeTextViewLayoutParams.BottomMargin = ConversionUtils.ConvertDpToPixels(16);
                AddView(typeTextView, typeTextViewLayoutParams);

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
                    sb.AppendLine(address.Street);
                }
                if (!string.IsNullOrWhiteSpace(address.Area))
                {
                    sb.AppendLine(address.Area);
                }
                if (!string.IsNullOrWhiteSpace(address.ZipCode))
                {
                    sb.Append(address.ZipCode);
                    if (string.IsNullOrWhiteSpace(address.City))
                    {
                        sb.AppendLine();
                    }
                }
                if (!string.IsNullOrWhiteSpace(address.City))
                {
                    sb.Append(" ").AppendLine(address.City);
                }
                if (address.Country != null)
                {
                    sb.Append(address.Country.Name);
                }
                return sb.ToString();
            }
        }
    }
}
