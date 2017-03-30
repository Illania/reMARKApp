//
// Project: Mark5.Mobile.Droid
// File: DocumentDateRangeSearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class DocumentDateRangeSearchView : AbstractSearchView<SearchDocumentsCriteria>
    {
        long fromTimestamp = -1;
        long toTimestamp = -1;

        readonly AppCompatTextView dateRangeFromTextView;
        readonly AppCompatTextView dateRangeToTextView;

        readonly DocumentSearchCriteriaFragment parentFragment;

        public DocumentDateRangeSearchView(Context context, DocumentSearchCriteriaFragment f) : base(context)
        {
            Orientation = Horizontal;
            SetBackgroundColor(BackgroundColorNormalState);

            parentFragment = f;

            var leftLayout = new LinearLayoutCompat(context)
            {
                Orientation = Vertical,
                LayoutParameters = new LayoutParams(0, ViewGroup.LayoutParams.WrapContent)
                {
                    Weight = 1.0f
                }
            };
            leftLayout.Click += From_Click;
            AddView(leftLayout);

            var fromTitleTextView = new AppCompatTextView(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Gravity = GravityFlags.Center
            };
            fromTitleTextView.Text = Context.GetString(Resource.String.search_document_date_from);
            fromTitleTextView.SetTextAppearanceCompat(context, TextStyleTopLineResourceId);
            leftLayout.AddView(fromTitleTextView);

            dateRangeFromTextView = new AppCompatTextView(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Gravity = GravityFlags.Center
            };
            dateRangeFromTextView.SetTextAppearanceCompat(context, TextStyleBottomLineResourceId);
            leftLayout.AddView(dateRangeFromTextView);

            var separator = new AppCompatTextView(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent),
                Gravity = GravityFlags.Center
            };
            separator.Text = "\u2014";
            separator.SetTextAppearanceCompat(context, TextStyleBottomLineResourceId);
            AddView(separator);

            var rightLayout = new LinearLayoutCompat(context)
            {
                Orientation = Vertical,
                LayoutParameters = new LayoutParams(0, ViewGroup.LayoutParams.MatchParent)
                {
                    Weight = 1.0f
                }
            };
            rightLayout.Click += To_Click;
            AddView(rightLayout);

            var toTitleTextView = new AppCompatTextView(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Gravity = GravityFlags.Center
            };
            toTitleTextView.Text = Context.GetString(Resource.String.search_document_date_to);

            toTitleTextView.SetTextAppearanceCompat(context, TextStyleTopLineResourceId);
            rightLayout.AddView(toTitleTextView);

            dateRangeToTextView = new AppCompatTextView(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Gravity = GravityFlags.Center
            };

            dateRangeToTextView.SetTextAppearanceCompat(context, TextStyleBottomLineResourceId);
            rightLayout.AddView(dateRangeToTextView);

            UpdateText();
        }
        void OpenDateRangeFragment(bool startWithTo)
        {
            var f = new PickDateRangeFragment
            {
                FromTimestamp = fromTimestamp,
                ToTimestamp = toTimestamp,
                StartWithToDate = startWithTo,
                CloseRequest = UpdateTimestamps,
            };
            parentFragment.ReplaceFragment(f, f.GenerateTag());
        }

        void UpdateTimestamps(long fromT, long toT)
        {
            fromTimestamp = fromT;
            toTimestamp = toT;
            UpdateText();
            UpdateCriteria();
        }

        void From_Click(object sender, EventArgs e)
        {
            OpenDateRangeFragment(false);
        }

        void To_Click(object sender, EventArgs e)
        {
            OpenDateRangeFragment(true);
        }

        void UpdateText()
        {
            dateRangeFromTextView.Text = fromTimestamp == -1 ? "-" : fromTimestamp.FormatServerTimestampAsDateString(Context);
            dateRangeToTextView.Text = toTimestamp == -1 ? "-" : toTimestamp.FormatServerTimestampAsDateString(Context);
        }

        public void SetFromText(long timestamp)
        {
            dateRangeFromTextView.Text = timestamp == -1 ? "-" : timestamp.FormatServerTimestampAsDateString(Context);
        }

        public void SetToText(long timestamp)
        {
            dateRangeToTextView.Text = timestamp == -1 ? "-" : timestamp.FormatServerTimestampAsDateString(Context);
        }

        public override void Refresh()
        {
            if (Criteria.DateRange != null && Criteria.DateRange.Enabled)
            {
                fromTimestamp = Criteria.DateRange.StartTimestamp;
                toTimestamp = Criteria.DateRange.EndTimestamp;
            }

            UpdateText();
        }

        public override void UpdateCriteria()
        {
            Criteria.DateRange = new DateRange
            {
                Enabled = fromTimestamp != -1 && toTimestamp != -1,
                StartTimestamp = fromTimestamp,
                EndTimestamp = toTimestamp
            };
        }
    }
}