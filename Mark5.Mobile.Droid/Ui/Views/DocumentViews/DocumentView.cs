//
// Project: Mark5.Mobile.Droid
// File: DocumentView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.DocumentViews
{

    public abstract class DocumentView : LinearLayoutCompat
    {

        public DocumentPreview DocumentPreview { get; set; }

        public Document Document { get; set; }

        protected int DistanceNone;
        protected int DistanceLarge;
        protected int DistanceNormal;
        protected int DistanceSmall;

        protected DocumentView(Context context)
            : base(context)
        {
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

            DistanceLarge = ConversionUtils.ConvertDpToPixels(16.0f);
            DistanceNormal = ConversionUtils.ConvertDpToPixels(8.0f);
            DistanceSmall = ConversionUtils.ConvertDpToPixels(4.0f);

            Visibility = ViewStates.Gone;
        }

        public abstract void RefreshView();
    }
}

