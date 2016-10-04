//
// Project: Mark5.Mobile.Droid
// File: AddressesView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Views.Common;

namespace Mark5.Mobile.Droid.Ui.Views.ShortcodeViews
{

    public class AddressesView : ShortcodeView
    {

        readonly DocumentAddressType type;

        public event EventHandler<DocumentAddress> DocumentAddressClicked = delegate { };

        public AddressesView(Context context, DocumentAddressType type)
            : base(context)
        {
            this.type = type;

            InitializeView();
        }

        void InitializeView()
        {
            var titleView = new AppCompatTextView(Context)
            {
                LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                {
                    BottomMargin = DistanceSmall
                },
                Text = type.ToString()
            };
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                titleView.SetTextAppearance(Context, Resource.Style.fontLarge);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                titleView.SetTextAppearance(Resource.Style.fontLarge);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            titleView.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkerblue)));
            titleView.SetPadding(DistanceLarge, 0, DistanceNormal, 0);
            InnerLayout.AddView(titleView);

            InnerLayout.AddView(new Divider(Context));
        }

        public override void RefreshView()
        {
            if (ShortcodePreview != null && Shortcode != null && Shortcode.Addresses.Any(da => da.Type == CommunicationAddressType.Email && da.AddressType == type))
            {
                Visibility = ViewStates.Visible;

                var addresses = Shortcode.Addresses.Where(da => da.Type == CommunicationAddressType.Email && da.AddressType == type).ToArray();
                for (int i = 0; i < addresses.Length; i++)
                {
                    var a = addresses[i];
                    var isNotLast = i != addresses.Length - 1;

                    var av = new AddressView(Context, a, DistanceVeryLarge, DistanceNormal);
                    av.Click += (sender, e) => DocumentAddressClicked(this, a);
                    InnerLayout.AddView(av);

                    if (isNotLast)
                    {
                        InnerLayout.AddView(new Divider(Context, DistanceVeryLarge, 0, 0, 0));
                    }
                }
            }
            else
            {
                Visibility = ViewStates.Gone;

                InnerLayout.RemoveViews(2, InnerLayout.ChildCount - 3);
            }
        }

        class AddressView : LinearLayoutCompat
        {

            public AddressView(Context context, DocumentAddress address, int distanceVeryLarge, int distanceNormal)
                : base(context)
            {
                var typedArray = Context.ObtainStyledAttributes(new int[] { Resource.Attribute.selectableItemBackground });
                SetBackgroundResource(typedArray.GetResourceId(0, 0));
                typedArray.Recycle();

                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                Orientation = Vertical;
                SetPadding(distanceVeryLarge, distanceNormal, distanceVeryLarge, distanceNormal);

                Clickable = true;

                var addressView = new AppCompatTextView(Context)
                {
                    LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                    Text = address.Address
                };
                if (Build.VERSION.SdkInt < BuildVersionCodes.M)
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    addressView.SetTextAppearance(Context, Resource.Style.fontPrimary);
#pragma warning restore CS0618 // Type or member is obsolete
                }
                else
                {
#pragma warning disable XA0001 // Find issues with Android API usage
                    addressView.SetTextAppearance(Resource.Style.fontPrimary);
#pragma warning restore XA0001 // Find issues with Android API usage
                }
                AddView(addressView);

                if (!string.IsNullOrWhiteSpace(address.Name))
                {
                    var nameView = new AppCompatTextView(Context)
                    {
                        LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                        Text = address.Name
                    };
                    if (Build.VERSION.SdkInt < BuildVersionCodes.M)
                    {
#pragma warning disable CS0618 // Type or member is obsolete
                        nameView.SetTextAppearance(Context, Resource.Style.fontSmallLight);
#pragma warning restore CS0618 // Type or member is obsolete
                    }
                    else
                    {
#pragma warning disable XA0001 // Find issues with Android API usage
                        nameView.SetTextAppearance(Resource.Style.fontSmallLight);
#pragma warning restore XA0001 // Find issues with Android API usage
                    }
                    AddView(nameView);
                }
            }
        }
    }
}
