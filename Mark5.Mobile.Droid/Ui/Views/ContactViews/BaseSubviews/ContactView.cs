//
// Project: 
// File: BaseCardSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public abstract class ContactView : LinearLayoutCompat
    {
        protected int DistanceVeryLarge;
        protected int DistanceLarge;
        protected int DistanceNormal;
        protected int DistanceSmall;

        protected View Divider;

        public ContactPreview ContactPreview { get; set; }
        public Contact Contact { get; set; }

        public bool Visible
        {
            get
            {
                return Visibility == ViewStates.Visible;
            }
        }

        protected ContactView(Context context) : base(context)
        {
            DistanceVeryLarge = ConversionUtils.ConvertDpToPixels(24);
            DistanceLarge = ConversionUtils.ConvertDpToPixels(16);
            DistanceNormal = ConversionUtils.ConvertDpToPixels(8);
            DistanceSmall = ConversionUtils.ConvertDpToPixels(4);
        }

        public void HideSeparator()
        {
            if (Divider != null)
            {
                Divider.Visibility = ViewStates.Gone;
            }
        }

        public abstract void RefreshView();
    }
}
