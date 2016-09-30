//
// Project: 
// File: CommunicationAddressesSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System.Linq;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public class CommunicationAddressesSubview : ContactSubView
    {
        CommunicationAddressType addressType;
        LinearLayoutCompat contentLayout;

        public CommunicationAddressesSubview(Android.Content.Context context, CommunicationAddressType type) : base(context)
        {
            addressType = type;
            SetTitle(type.ToString());

            var imageLayoutView = new LinearLayoutCompat(context);
            imageLayoutView.Orientation = Vertical;
            imageLayoutView.LayoutParameters = new LayoutParams(ConversionUtils.ConvertDpToPixels(72), ViewGroup.LayoutParams.MatchParent);
            var paddingValue = ConversionUtils.ConvertDpToPixels(24);
            imageLayoutView.SetPadding(paddingValue, paddingValue, paddingValue, paddingValue);

            internalLayout.AddView(imageLayoutView);

            var imageView = new AppCompatImageView(context);
            var buttonSizes = ConversionUtils.ConvertDpToPixels(24);
            imageView.SetImageResource(Resource.Drawable.email);
            imageLayoutView.AddView(imageView, new LayoutParams(buttonSizes, buttonSizes));

            contentLayout = new LinearLayoutCompat(context);
            contentLayout.Orientation = Vertical;
            contentLayout.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent, 1.0f);

            internalLayout.AddView(contentLayout);
        }

        public override void RefreshView()
        {
            var communicationAddressesForType = Contact?.CommunicationAddresses.Where(ca => ca.Type == addressType);
            if (Contact != null && communicationAddressesForType.Any())
            {
                Visibility = ViewStates.Visible;

                foreach (var address in communicationAddressesForType)
                {
                    var subsubview = new TrialSubView(Context, address);
                    contentLayout.AddView(subsubview);
                    subsubview = new TrialSubView(Context, address);
                    contentLayout.AddView(subsubview);
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

        class TrialSubView : LinearLayoutCompat
        {
            public TrialSubView(Android.Content.Context context, CommunicationAddress communicationAddress) : base(context)
            {
                Orientation = Vertical;
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                var paddingValue = ConversionUtils.ConvertDpToPixels(16);
                SetPadding(0, paddingValue, 0, paddingValue);

                var addressTextView = new AppCompatTextView(context);
                addressTextView.Text = communicationAddress.Address;

                addressTextView.SetTextAppearanceCompat(context, Resource.Style.contactPrimary);
                addressTextView.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

                AddView(addressTextView, new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

                if (!string.IsNullOrEmpty(communicationAddress.Description))
                {
                    var descriptionTextView = new AppCompatTextView(context);
                    descriptionTextView.Text = communicationAddress.Description;
                    var descriptionTextViewLayoutParams = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1.0f);
                    descriptionTextViewLayoutParams.TopMargin = ConversionUtils.ConvertDpToPixels(3);
                    descriptionTextView.SetTextAppearanceCompat(context, Resource.Style.contactSecondary);
                    AddView(descriptionTextView, descriptionTextViewLayoutParams);
                }
            }
        }
    }

}
