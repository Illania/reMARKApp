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
        AppCompatEditText contentTextView;

        public RecipientsView(Context context, string title)
            : base(context)
        {
            Orientation = Horizontal;
            SetPadding(DistanceNormal, DistanceSmall, DistanceNormal, DistanceSmall);

            var titleTextView = new AppCompatTextView(context)
            {
                LayoutParameters = new LayoutParams(ConversionUtils.ConvertDpToPixels(40), ViewGroup.LayoutParams.WrapContent),
            };
            titleTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimaryBold);
            titleTextView.Text = title;
            AddView(titleTextView);


            contentTextView = new AppCompatEditText(context);
            contentTextView.SetPadding(0, 0, 0, 0);
            var contentLayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            contentLayoutParameters.Weight = 1;
            contentTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            contentTextView.SetBackgroundColor(Android.Graphics.Color.Transparent);
            AddView(contentTextView, contentLayoutParameters);
        }
    }
}
