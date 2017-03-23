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
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.Common;
using Mark5.Mobile.Droid.Utilities;

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
        }

        public override void RefreshView()
        {
            if (Contact != null && Contact.PhysicalAddresses.Any())
            {
                RemoveAllViews();
                foreach (var address in Contact.PhysicalAddresses)
                {
                    var subview = new PhysicalAddressesSubSubview(Context, address, DistanceSmall, DistanceNormal, DistanceLarge, DistanceVeryLarge);
                    subview.Click += (sender, e) => PhysicalAddressClicked(this, address);
                    AddView(subview);
                }

                Visibility = ViewStates.Visible;
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }

        class PhysicalAddressesSubSubview : LinearLayoutCompat
        {

            public PhysicalAddressesSubSubview(Context context, PhysicalAddress physicalAddress, int distanceSmall, int distanceNormal, int distanceLarge, int distanceVeryLarge)
                : base(context)
            {
                Clickable = true;
                
                var typedArray = Context.ObtainStyledAttributes(new int[] { Resource.Attribute.selectableItemBackground });
                SetBackgroundResource(typedArray.GetResourceId(0, 0));
                typedArray.Recycle();

                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                SetPadding(distanceVeryLarge, distanceNormal, distanceNormal, distanceNormal);

                var iconImageView = new AppCompatImageView(context);
                iconImageView.SetImageResource(Resource.Drawable.map);
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

                var addressTextView = new AppCompatTextView(context);
                addressTextView.Text = GetAddressText(physicalAddress);
                addressTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
                innerLayout.AddView(addressTextView, new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
                
                var typeTextView = new AppCompatTextView(context);
                typeTextView.Text = physicalAddress.Type.Name;
                typeTextView.SetTextAppearanceCompat(context, Resource.Style.fontSmallLight);
                AddView(innerLayout);
                innerLayout.AddView(typeTextView, new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

                LongClickable = true;
                LongClick += (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(addressTextView.Text))
                        Integration.CopyToClipboard(context, addressTextView.Text);
                };
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
