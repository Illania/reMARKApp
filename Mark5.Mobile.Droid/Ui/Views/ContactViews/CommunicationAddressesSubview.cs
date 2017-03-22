//
// Project: Mark5.Mobile.Droid
// File: CommunicationAddressesSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

using System;
using System.Globalization;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{

    public class CommunicationAddressesSubview : ContactView
    {

        readonly CommunicationAddressType addressType;

        public event EventHandler<CommunicationAddress> AddressClicked = delegate { };

        public CommunicationAddressesSubview(Context context, CommunicationAddressType addressType)
            : base(context)
        {
            this.addressType = addressType;

            Orientation = Vertical;
            LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        }

        public override void RefreshView()
        {
            var communicationAddressesForType = Contact?.CommunicationAddresses.Where(ca => ca.Type == addressType).OrderBy(ad => ad.IsPrimary != true).ToList();
            if (communicationAddressesForType?.Count > 0)
            {
                RemoveAllViews();
                foreach (var communicationAddress in communicationAddressesForType)
                {
                    var subsubview = new CommunicationAddressesSubSubview(Context, communicationAddress, DistanceSmall, DistanceNormal, DistanceLarge, DistanceVeryLarge);
                    subsubview.Click += (sender, e) => AddressClicked(this, communicationAddress);
                    AddView(subsubview);
                }

                Visibility = ViewStates.Visible;
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }

        class CommunicationAddressesSubSubview : LinearLayoutCompat
        {

            public CommunicationAddressesSubSubview(Context context, CommunicationAddress communicationAddress, int distanceSmall, int distanceNormal, int distanceLarge, int distanceVeryLarge)
                : base(context)
            {
                var typedArray = Context.ObtainStyledAttributes(new int[] { Resource.Attribute.selectableItemBackground });
                SetBackgroundResource(typedArray.GetResourceId(0, 0));
                typedArray.Recycle();

                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                SetPadding(distanceVeryLarge, distanceNormal, distanceNormal, distanceNormal);

                Clickable = true;

                var iconImageView = new AppCompatImageView(context);
                iconImageView.SetImageResource(GetDrawableIdForAddressType(communicationAddress.Type));
                iconImageView.SetColorFilter(new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));
                AddView(iconImageView);

                var innerLayout = new LinearLayoutCompat(Context)
                {
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                    {
                        LeftMargin = distanceLarge
                    },
                    Orientation = Vertical
                };
                AddView(innerLayout);

                var titleTextView = new AppCompatTextView(context);
                titleTextView.Text = AddressUtilities.FormatCommunicationAddress(communicationAddress);
                titleTextView.SetTextAppearanceCompat(context, communicationAddress.IsPrimary ? Resource.Style.fontPrimaryBold : Resource.Style.fontPrimary);
                innerLayout.AddView(titleTextView, new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

                var typeTextView = new AppCompatTextView(context);
                typeTextView.Text = communicationAddress.Type.ToString().ToLower();
                var typeTextViewLayoutParams = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                typeTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimaryLight);
                innerLayout.AddView(typeTextView, typeTextViewLayoutParams);

                if (!string.IsNullOrWhiteSpace(communicationAddress.Description))
                {
                    var descriptionTextView = new AppCompatTextView(context);
                    descriptionTextView.Text = communicationAddress.Description;
                    var descriptionTextViewLayoutParams = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                    descriptionTextViewLayoutParams.TopMargin = distanceSmall / 2;
                    descriptionTextView.SetTextAppearanceCompat(context, Resource.Style.fontSmallLight);
                    innerLayout.AddView(descriptionTextView, descriptionTextViewLayoutParams);
                }

                LongClickable = true;
                LongClick += (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(titleTextView.Text))
                    {
                        Integration.CopyToClipboard(context, titleTextView.Text);
                    }
                };
            }

            int GetDrawableIdForAddressType(CommunicationAddressType type)
            {
                switch (type)
                {
                    case CommunicationAddressType.Email:
                        return Resource.Drawable.contacts_email;
                    case CommunicationAddressType.Fax:
                        return Resource.Drawable.contacts_fax;
                    case CommunicationAddressType.IM:
                        return Resource.Drawable.contacts_im;
                    case CommunicationAddressType.Internal:
                        return Resource.Drawable.contacts_internal;
                    case CommunicationAddressType.Mobile:
                        return Resource.Drawable.contacts_mobile;
                    case CommunicationAddressType.Skype:
                        return Resource.Drawable.contacts_skype;
                    case CommunicationAddressType.Phone:
                        return Resource.Drawable.contacts_phone;
                    case CommunicationAddressType.Telex:
                        return Resource.Drawable.contacts_telex;
                    default:
                        throw new ArgumentException("Invalid address type!");
                }
            }
        }
    }
}
