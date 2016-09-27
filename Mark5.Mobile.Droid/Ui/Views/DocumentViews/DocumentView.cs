//
// Project: Mark5.Mobile.Droid
// File: IDocumentView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.DocumentViews
{

    public abstract class DocumentView : LinearLayoutCompat
    {

        public DocumentPreview DocumentPreview { get; set; }

        public Document Document { get; set; }

        protected readonly int PaddingNone;
        protected readonly int PaddingLarge;
        protected readonly int PaddingSmall;

        protected DocumentView(Context context)
            : base(context)
        {
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

            PaddingLarge = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 16.0f, Resources.DisplayMetrics) + 0.5f);
            PaddingSmall = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 8.0f, Resources.DisplayMetrics) + 0.5f);

            Visibility = ViewStates.Gone;
        }

        public abstract void RefreshView();
    }
}

