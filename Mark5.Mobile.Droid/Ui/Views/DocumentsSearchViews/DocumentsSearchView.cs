//
// Project: Mark5.Mobile.Droid
// File: DocumentsSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.DocumentsSearchViews
{

    public abstract class DocumentsSearchView : LinearLayoutCompat
    {

        protected int DistanceNone;
        protected int DistanceLarge;
        protected int DistanceNormal;
        protected int DistanceSmall;

        protected DocumentsSearchView(Context context)
            : base(context)
        {
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

            DistanceLarge = ConversionUtils.ConvertDpToPixels(16.0f);
            DistanceNormal = ConversionUtils.ConvertDpToPixels(8.0f);
            DistanceSmall = ConversionUtils.ConvertDpToPixels(4.0f);
        }

        public abstract void FromCriteria(SearchDocumentsCriteria criteria);

        public abstract void ToCriteria(SearchDocumentsCriteria criteria);
    }
}
