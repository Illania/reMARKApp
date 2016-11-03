//
// Project: Mark5.Mobile.Droid
// File: RecipientsView.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

using System;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public class RecipientsView : ComposeDocumentView
    {
        public RecipientsView(Context context, string title)
            : base(context)
        {
            Orientation = Horizontal;
            SetPadding(DistanceLarge, DistanceLarge, DistanceLarge, DistanceNormal);

            var titleTextView = new AppCompatTextView(context)
            {
                LayoutParameters = new LayoutParams(ConversionUtils.ConvertDpToPixels(20), ViewGroup.LayoutParams.MatchParent),
            };
            titleTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimaryBold);
            titleTextView.Text = title;

            var contentEditTextView = new AppCompatEditText(context);
            var contentLayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent);
            contentLayoutParameters.Weight = 1;
            contentEditTextView.LayoutParameters = contentLayoutParameters;
            contentEditTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
        }
    }
}
