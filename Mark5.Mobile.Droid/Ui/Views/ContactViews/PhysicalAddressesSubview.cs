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
    public class PhysicalAddressesSubview : CommunicationCardSubview
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
                    internalLayout.AddView(subsubview);
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
                Orientation = Horizontal;
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                var paddingValue = ConversionUtils.ConvertDpToPixels(4);
                SetPadding(0, paddingValue, paddingValue, paddingValue);

                var typeTextView = new AppCompatTextView(context);
                typeTextView.Text = physicalAddress.Type.Name;
                AddView(typeTextView, new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent));

                var addressTextView = new AppCompatTextView(context);
                addressTextView.Text = GetAddressText(physicalAddress);
                var addressTextViewLayoutParams = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent, 1.0f);
                addressTextViewLayoutParams.LeftMargin = ConversionUtils.ConvertDpToPixels(6);
                AddView(addressTextView, addressTextViewLayoutParams);

                var button = new AppCompatImageView(context);
                button.SetImageResource(Resource.Drawable.folder_draft);
                var buttonSizes = ConversionUtils.ConvertDpToPixels(16);
                AddView(button, new LayoutParams(buttonSizes, buttonSizes));
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
