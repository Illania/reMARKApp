//
// Project: Mark5.Mobile.Droid
// File: DocumentDateRangeSearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
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
        bool inFragment;

        public DocumentDateRangeSearchView(Context context, DocumentSearchCriteriaFragment f, bool inFragment = false) : base(context)
        {
            Orientation = Horizontal;
            SetBackgroundColor(inFragment ? new Color(ContextCompat.GetColor(Context, Resource.Color.darkerblue)) : BackgroundColorNormalState);

            parentFragment = f;
            this.inFragment = inFragment;

            Clickable = true;
            Click += DocumentDateRangeSearchView_Click;

            var leftLayout = new LinearLayoutCompat(context)
            {
                Orientation = Vertical,
                LayoutParameters = new LayoutParams(0, ViewGroup.LayoutParams.WrapContent)
                {
                    Weight = 1.0f
                }
            };
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
            dateRangeFromTextView.Click += DateRangeFromTextView_Click;
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
            dateRangeToTextView.Click += DateRangeToTextView_Click;

            dateRangeToTextView.SetTextAppearanceCompat(context, TextStyleBottomLineResourceId);
            rightLayout.AddView(dateRangeToTextView);

            UpdateText();
        }

        void DocumentDateRangeSearchView_Click(object sender, EventArgs e)
        {
            if (inFragment)
                return;

            OpenDateRangeFragment();
        }

        void OpenDateRangeFragment()
        {
            var f = new PickDateRangeFragment();
            //TODO complete
            parentFragment.PushDropdownViewFragment(f, f.GenerateTag());
        }

        void DateRangeFromTextView_Click(object sender, EventArgs e)
        {
            if (!inFragment)
                OpenDateRangeFragment();


            //var todayTimeStamp = DateTime.UtcNow.Date.ConvertDateTimeToTimestampMilliseconds();
            //var maxTimestamp = toTimestamp == -1 ? todayTimeStamp : toTimestamp;
            //fromTimestamp = await Dialogs.ShowDatePicker(Context, fromTimestamp, maxTimestamp: maxTimestamp);

            //UpdateText();
        }

        void DateRangeToTextView_Click(object sender, EventArgs e)
        {
            if (!inFragment)
                OpenDateRangeFragment();
            //var todayTimeStamp = DateTime.UtcNow.Date.ConvertDateTimeToTimestampMilliseconds();
            //toTimestamp = await Dialogs.ShowDatePicker(Context, toTimestamp, fromTimestamp, todayTimeStamp); //TODO need to check which timestamp we get when we select today (the hour)

            //UpdateText();
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

        public override void FromCriteria(SearchDocumentsCriteria criteria)
        {
            //TODO
        }

        public override void ToCriteria(SearchDocumentsCriteria criteria)
        {
            //TODO
        }
    }
}