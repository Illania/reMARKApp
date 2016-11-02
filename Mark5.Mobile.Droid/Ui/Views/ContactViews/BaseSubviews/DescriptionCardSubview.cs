//
// Project: Mark5.Mobile.Droid
// File: ContactViewBaseTextSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public abstract class DescriptionCardSubview : ContactView
    {
        readonly AppCompatTextView titleTextView;
        readonly AppCompatTextView contentTextView;

        public string Content
        {
            set
            {
                contentTextView.Text = value;
            }
        }

        public string Title
        {
            set
            {
                titleTextView.Text = value;
            }
        }

        protected DescriptionCardSubview(Context context) : base(context)
        {
            Orientation = Vertical;
            SetPadding(DistanceVeryLarge, 0, 0, 0);
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

            titleTextView = new AppCompatTextView(context);
            titleTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            titleTextView.SetPadding(0, 0, DistanceVeryLarge, 0);
            var titleTextViewLayoutParams = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            titleTextViewLayoutParams.TopMargin = DistanceLarge;
            AddView(titleTextView, titleTextViewLayoutParams);

            contentTextView = new AppCompatTextView(context);
            contentTextView.SetTextAppearanceCompat(context, Resource.Style.fontSmallLight);
            contentTextView.SetPadding(0, 0, DistanceVeryLarge, 0);
            var contentTextViewLayoutParams = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            contentTextViewLayoutParams.TopMargin = ConversionUtils.ConvertDpToPixels(3);
            contentTextViewLayoutParams.BottomMargin = DistanceLarge;
            AddView(contentTextView, contentTextViewLayoutParams);

            Divider = new Divider(Context);
            AddView(Divider);
        }
    }

}
