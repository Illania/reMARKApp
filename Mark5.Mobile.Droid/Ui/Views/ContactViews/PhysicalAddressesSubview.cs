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
    public class PhysicalAddressesSubview : BaseCardSubview
    {
        public PhysicalAddressesSubview(Android.Content.Context context) : base(context)
        {
            Orientation = Vertical;
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        }

        public override void RefreshView()
        {
            if (Contact != null && Contact.PhysicalAddresses.Any())
            {
                Visibility = ViewStates.Visible;

                foreach (var address in Contact.PhysicalAddresses)
                {
                    var subsubview = new PhysicalAddressesSubSubview(Context, address);
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
            public PhysicalAddressesSubSubview(Android.Content.Context context, PhysicalAddress physicalAddress) : base(context)
            {
                Orientation = Vertical;
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                var paddingValue = ConversionUtils.ConvertDpToPixels(4);
                SetPadding(0, paddingValue, paddingValue, paddingValue);

                var addressTextView = new AppCompatTextView(context);
                addressTextView.Text = GetAddressText(physicalAddress);
                addressTextView.SetTextAppearanceCompat(context, Resource.Style.contactPrimary);
                AddView(addressTextView, new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

                var typeTextView = new AppCompatTextView(context);
                typeTextView.SetTextAppearanceCompat(context, Resource.Style.contactSecondary);
                typeTextView.Text = physicalAddress.Type.Name;
                var typeTextViewLayoutParams = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                typeTextViewLayoutParams.TopMargin = ConversionUtils.ConvertDpToPixels(3);
                AddView(typeTextView, typeTextViewLayoutParams);
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
