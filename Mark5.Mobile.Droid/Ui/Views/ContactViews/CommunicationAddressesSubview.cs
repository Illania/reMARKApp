//
// Project: Mark5.Mobile.Droid
// File: CommunicationAddressesSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

using System;
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

        protected readonly LinearLayoutCompat ContentLayout;
        protected readonly AppCompatImageView IconImageView;

        readonly CommunicationAddressType addressType;

        public event EventHandler<CommunicationAddress> AddressClicked = delegate { };

        public CommunicationAddressesSubview(Context context, CommunicationAddressType addressType)
            : base(context)
        {
            this.addressType = addressType;

            Orientation = Vertical;
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

            var internalLayout = new LinearLayoutCompat(context);
            internalLayout.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            AddView(internalLayout);

            var iconImageViewLayout = new LinearLayoutCompat(context);
            iconImageViewLayout.Orientation = Vertical;
            iconImageViewLayout.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            iconImageViewLayout.SetPadding(DistanceNormal + DistanceSmall, DistanceLarge, DistanceNormal + DistanceSmall, DistanceLarge);

            internalLayout.AddView(iconImageViewLayout);

            IconImageView = new AppCompatImageView(context);
            IconImageView.SetImageResource(GetDrawableIdForAddressType(addressType));
            IconImageView.SetColorFilter(new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));
            iconImageViewLayout.AddView(IconImageView, new LayoutParams(DistanceVeryLarge, DistanceVeryLarge));

            ContentLayout = new LinearLayoutCompat(context);
            ContentLayout.Orientation = Vertical;
            ContentLayout.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent, 1f);

            internalLayout.AddView(ContentLayout);

            Divider = new Divider(Context);
            AddView(Divider);
        }

        public override void RefreshView()
        {
            if (Contact.PreferrableType == addressType)
            {
                IconImageView.SetColorFilter(new Color(ContextCompat.GetColor(Context, Resource.Color.brown)));
            }

            var communicationAddressesForType = Contact?.CommunicationAddresses.Where(ca => ca.Type == addressType).OrderBy(ad => ad.IsPrimary != true).ToList();
            if (communicationAddressesForType?.Count > 0)
            {
                ContentLayout.RemoveAllViews();

                for (int i = 0; i < communicationAddressesForType.Count; i++)
                {
                    var communicationAddress = communicationAddressesForType[i];
                    var isLast = i == communicationAddressesForType.Count - 1;

                    string titleText;
                    string descriptionText;

                    if (addressType == CommunicationAddressType.IM)
                    {
                        var handleAndType = ParseIm(communicationAddress.Address);
                        titleText = handleAndType.Item1;
                        descriptionText = handleAndType.Item2;
                    }
                    else
                    {
                        titleText = AddressUtilities.FormatCommunicationAddress(communicationAddress);
                        descriptionText = communicationAddress.Description;
                    }

                    var subsubview = new CommunicationAddressesSubSubview(Context, titleText, descriptionText, communicationAddress.IsPrimary, DistanceSmall, DistanceNormal, DistanceLarge);
                    subsubview.Click += (sender, e) => AddressClicked(this, communicationAddress);
                    ContentLayout.AddView(subsubview);

                    if (!isLast)
                    {
                        ContentLayout.AddView(new Divider(Context));
                    }
                }

                Visibility = ViewStates.Visible;
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
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

        class CommunicationAddressesSubSubview : LinearLayoutCompat
        {

            public CommunicationAddressesSubSubview(Context context, string titleText, string descriptionText, bool primary, int distanceSmall, int distanceNormal, int distanceLarge)
                : base(context)
            {
                var typedArray = Context.ObtainStyledAttributes(new int[] { Resource.Attribute.selectableItemBackground });
                SetBackgroundResource(typedArray.GetResourceId(0, 0));
                typedArray.Recycle();

                Orientation = Vertical;
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                SetPadding(distanceNormal, distanceLarge, 0, distanceLarge);

                Clickable = true;

                var titleTextView = new AppCompatTextView(context);
                titleTextView.Text = titleText;
                titleTextView.SetTextAppearanceCompat(context, primary ? Resource.Style.fontPrimaryBold : Resource.Style.fontPrimary);

                AddView(titleTextView, new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

                if (!string.IsNullOrEmpty(descriptionText))
                {
                    var descriptionTextView = new AppCompatTextView(context);
                    descriptionTextView.Text = descriptionText;
                    var descriptionTextViewLayoutParams = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1f);
                    descriptionTextViewLayoutParams.TopMargin = distanceSmall / 2;
                    descriptionTextView.SetTextAppearanceCompat(context, Resource.Style.fontSmallLight);
                    AddView(descriptionTextView, descriptionTextViewLayoutParams);
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
        }
    }
}
