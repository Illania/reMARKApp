//
// Project: 
// File: CommunicationAddressesSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System.Linq;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public class CommunicationAddressesSubview : ContactSubView
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
                    internalLayout.AddView(subsubview);
                }
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }

        class CommunicationAddressesSubSubview : LinearLayoutCompat
        {
            public CommunicationAddressesSubSubview(Android.Content.Context context, CommunicationAddress communicationAddress) : base(context)
            {
                Orientation = Horizontal;
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                var paddingValue = ConversionUtils.ConvertDpToPixels(4);
                SetPadding(0, paddingValue, paddingValue, paddingValue);

                var addressTextView = new AppCompatTextView(context);
                addressTextView.Text = communicationAddress.Address;
                AddView(addressTextView, new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent));

                var descriptionTextView = new AppCompatTextView(context);
                descriptionTextView.Text = communicationAddress.Description;
                var descriptionTextViewLayoutParams = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent, 1.0f);
                descriptionTextViewLayoutParams.LeftMargin = ConversionUtils.ConvertDpToPixels(6);
                AddView(descriptionTextView, descriptionTextViewLayoutParams);

                var button = new AppCompatImageView(context);
                var buttonSizes = ConversionUtils.ConvertDpToPixels(16);
                button.SetImageResource(Resource.Drawable.folder_draft);
                AddView(button, new LayoutParams(buttonSizes, buttonSizes));
            }
        }
    }

}
