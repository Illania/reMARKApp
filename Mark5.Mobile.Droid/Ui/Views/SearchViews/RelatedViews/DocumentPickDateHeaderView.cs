//
// Project: Mark5.Mobile.Droid
// File: DocumentPickDateSearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Android.Animation;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    
    public class DocumentPickDateHeaderView : LinearLayoutCompat
    {
        
        long fromTimestamp = -1;
        long toTimestamp = -1;

        readonly AppCompatTextView dateRangeFromTextView;
        readonly AppCompatTextView dateRangeToTextView;

        AppCompatTextView fromTitleTextView;
        AppCompatTextView toTitleTextView;

        readonly LinearLayoutCompat fromLayout;
        readonly LinearLayoutCompat toLayout;

        public event EventHandler FromClicked = delegate { };
        public event EventHandler ToClicked = delegate { };

        int textStyleTopLineResourceId = Resource.Style.searchViewDateTopLine;
        int textStyleBottomLineResourceId = Resource.Style.searchViewDateBottomLine;

        int textStyleTopLineSelectedResourceId = Resource.Style.searchViewDateTopLineSelected;
        int textStyleBottomLineSelectedResourceId = Resource.Style.searchViewDateBottomLineSelected;

        public DocumentPickDateHeaderView(Context context) : base(context)
        {
            Orientation = Horizontal;
            SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkerblue)));
            LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            LayoutTransition = new LayoutTransition();

            var bigPaddingValue = ConversionUtils.ConvertDpToPixels(16f);
            var mediumPaddingValue = ConversionUtils.ConvertDpToPixels(8f);
            SetPadding(bigPaddingValue, bigPaddingValue, bigPaddingValue, 0);

            fromLayout = new LinearLayoutCompat(context)
            {
                Orientation = Vertical,
                LayoutParameters = new LayoutParams(0, ViewGroup.LayoutParams.WrapContent)
                {
                    Weight = 1.0f,
                },
            };
            fromLayout.Clickable = true;
            fromLayout.Click += FromClicked;
            fromLayout.SetPadding(0, mediumPaddingValue, 0, mediumPaddingValue);
            AddView(fromLayout);

            fromTitleTextView = new AppCompatTextView(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Gravity = GravityFlags.Center
            };
            fromTitleTextView.Text = Context.GetString(Resource.String.search_document_date_from);
            fromLayout.AddView(fromTitleTextView);

            dateRangeFromTextView = new AppCompatTextView(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Gravity = GravityFlags.Center
            };
            fromLayout.AddView(dateRangeFromTextView);

            var marginValue = ConversionUtils.ConvertDpToPixels(16);
            var separator = new AppCompatTextView(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent)
                {
                    LeftMargin = marginValue,
                    RightMargin = marginValue
                },
                Gravity = GravityFlags.Center
            };
            separator.Text = "\u2014";
            separator.SetTextAppearanceCompat(context, textStyleBottomLineResourceId);
            AddView(separator);

            toLayout = new LinearLayoutCompat(context)
            {
                Orientation = Vertical,
                LayoutParameters = new LayoutParams(0, ViewGroup.LayoutParams.MatchParent)
                {
                    Weight = 1.0f
                }
            };
            toLayout.Clickable = true;
            toLayout.Click += ToClicked;
            toLayout.SetPadding(0, mediumPaddingValue, 0, mediumPaddingValue);
            AddView(toLayout);

            toTitleTextView = new AppCompatTextView(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Gravity = GravityFlags.Center
            };
            toTitleTextView.Text = Context.GetString(Resource.String.search_document_date_to);

            toLayout.AddView(toTitleTextView);

            dateRangeToTextView = new AppCompatTextView(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Gravity = GravityFlags.Center
            };

            toLayout.AddView(dateRangeToTextView);

            UpdateText();
        }

        public void PickFrom()
        {
            fromLayout.SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.lightblue)));
            fromTitleTextView.SetTextAppearanceCompat(Context, textStyleTopLineSelectedResourceId);
            dateRangeFromTextView.SetTextAppearanceCompat(Context, textStyleBottomLineSelectedResourceId);

            toLayout.SetBackgroundColor(Color.Transparent);
            toTitleTextView.SetTextAppearanceCompat(Context, textStyleTopLineResourceId);
            dateRangeToTextView.SetTextAppearanceCompat(Context, textStyleBottomLineResourceId);
        }

        public void PickTo()
        {
            fromLayout.SetBackgroundColor(Color.Transparent);
            fromTitleTextView.SetTextAppearanceCompat(Context, textStyleTopLineResourceId);
            dateRangeFromTextView.SetTextAppearanceCompat(Context, textStyleBottomLineResourceId);

            toLayout.SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.lightblue)));
            toTitleTextView.SetTextAppearanceCompat(Context, textStyleTopLineSelectedResourceId);
            dateRangeToTextView.SetTextAppearanceCompat(Context, textStyleBottomLineSelectedResourceId);
        }

        void UpdateText()
        {
            dateRangeFromTextView.Text = fromTimestamp == -1 ? "-" : fromTimestamp.ConvertTimestampMillisecondsToDateTime()
                                                                                    .ConvertUtcToServerTime()
                                                                                    .ConvertDateTimeToTimestampMilliseconds()
                                                                                    .FormatServerTimestampAsDateString(Context);
            dateRangeToTextView.Text = toTimestamp == -1 ? "-" : toTimestamp.ConvertTimestampMillisecondsToDateTime()
                                                                                    .ConvertUtcToServerTime()
                                                                                    .ConvertDateTimeToTimestampMilliseconds()
                                                                                    .FormatServerTimestampAsDateString(Context);
        }

        public void SetFromText(long timestamp)
        {
            dateRangeFromTextView.Text = timestamp == -1 ? "-" : timestamp.ConvertTimestampMillisecondsToDateTime()
                                                                                    .ConvertUtcToServerTime()
                                                                                    .ConvertDateTimeToTimestampMilliseconds()
                                                                                    .FormatServerTimestampAsDateString(Context);
        }

        public void SetToText(long timestamp)
        {
            dateRangeToTextView.Text = timestamp == -1 ? "-" : timestamp.ConvertTimestampMillisecondsToDateTime()
                                                                                    .ConvertUtcToServerTime()
                                                                                    .ConvertDateTimeToTimestampMilliseconds()
                                                                                    .FormatServerTimestampAsDateString(Context);
        }
    }
}
