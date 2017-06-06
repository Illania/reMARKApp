//
// Project: Mark5.Mobile.Droid
// File: RecipentsView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.MailViewerViews
{
    public class RecipentsView : MailViewerView
    {
        TableLayout compactLayout;
        TableRow tableRowFrom;
        TableRow tableRowTo;
        TableRow tableRowCc;
        TableRow tableRowBcc;
        TableRow tableRowReplyTo;
        TableRow tableRowDateReceived;
        AppCompatTextView fromValue;
        AppCompatTextView toValue;
        AppCompatTextView ccValue;
        AppCompatTextView bccValue;
        AppCompatTextView replyToValue;
        AppCompatTextView dateReceivedValue;

        public RecipentsView(Context context)
            : base(context)
        {
            InitializeView();
        }

        void InitializeView()
        {
            Orientation = Vertical;
            SetPadding(DistanceLarge, DistanceLarge, DistanceLarge, DistanceLarge);

            compactLayout = new TableLayout(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
            };
            AddView(compactLayout);

            // From
            tableRowFrom = new TableRow(Context);
            tableRowFrom.SetPadding(DistanceNone, DistanceSmall, DistanceNone, DistanceNone);
            var fromLabel = new AppCompatTextView(Context)
            {
                Text = Context.GetString(Resource.String.from) + ":"
            };
            fromLabel.SetTextAppearanceCompat(Context, Resource.Style.fontPrimaryLight);

            tableRowFrom.AddView(fromLabel);
            fromValue = new AppCompatTextView(Context);
            fromValue.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);

            fromValue.SetPadding(DistanceNormal, DistanceNone, DistanceNone, DistanceNone);
            tableRowFrom.AddView(fromValue);
            compactLayout.AddView(tableRowFrom);

            // To
            tableRowTo = new TableRow(Context);
            tableRowTo.SetPadding(DistanceNone, DistanceSmall, DistanceNone, DistanceNone);
            var toLabel = new AppCompatTextView(Context)
            {
                Text = Context.GetString(Resource.String.to) + ":"
            };
            toLabel.SetTextAppearanceCompat(Context, Resource.Style.fontPrimaryLight);

            tableRowTo.AddView(toLabel);
            toValue = new AppCompatTextView(Context);
            toValue.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);

            toValue.SetPadding(DistanceNormal, DistanceNone, DistanceNone, DistanceNone);
            tableRowTo.AddView(toValue);
            compactLayout.AddView(tableRowTo);

            // Cc
            tableRowCc = new TableRow(Context);
            tableRowCc.SetPadding(DistanceNone, DistanceSmall, DistanceNone, DistanceNone);
            var ccLabel = new AppCompatTextView(Context)
            {
                Text = Context.GetString(Resource.String.cc) + ":"
            };
            ccLabel.SetTextAppearanceCompat(Context, Resource.Style.fontPrimaryLight);

            tableRowCc.AddView(ccLabel);
            ccValue = new AppCompatTextView(Context);
            ccValue.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);

            ccValue.SetPadding(DistanceNormal, DistanceNone, DistanceNone, DistanceNone);
            tableRowCc.AddView(ccValue);
            compactLayout.AddView(tableRowCc);

            // Bcc
            tableRowBcc = new TableRow(Context);
            tableRowBcc.SetPadding(DistanceNone, DistanceSmall, DistanceNone, DistanceNone);
            var bccLabel = new AppCompatTextView(Context)
            {
                Text = Context.GetString(Resource.String.bcc) + ":"
            };
            bccLabel.SetTextAppearanceCompat(Context, Resource.Style.fontPrimaryLight);

            tableRowBcc.AddView(bccLabel);
            bccValue = new AppCompatTextView(Context);
            bccValue.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);

            bccValue.SetPadding(DistanceNormal, DistanceNone, DistanceNone, DistanceNone);
            tableRowBcc.AddView(bccValue);
            compactLayout.AddView(tableRowBcc);

            // Reply to
            tableRowReplyTo = new TableRow(Context);
            tableRowReplyTo.SetPadding(DistanceNone, DistanceSmall, DistanceNone, DistanceNone);
            var replyToLabel = new AppCompatTextView(Context)
            {
                Text = Context.GetString(Resource.String.reply_to)
            };
            replyToLabel.SetTextAppearanceCompat(Context, Resource.Style.fontPrimaryLight);

            tableRowReplyTo.AddView(replyToLabel);
            replyToValue = new AppCompatTextView(Context);
            replyToValue.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);

            replyToValue.SetPadding(DistanceNormal, DistanceNone, DistanceNone, DistanceNone);
            tableRowReplyTo.AddView(replyToValue);
            compactLayout.AddView(tableRowReplyTo);

            // Date received
            tableRowDateReceived = new TableRow(Context);
            tableRowDateReceived.SetPadding(DistanceNone, DistanceSmall, DistanceNone, DistanceNone);
            var dateReceivedLabel = new AppCompatTextView(Context)
            {
                Text = Context.GetString(Resource.String.date)
            };
            dateReceivedLabel.SetTextAppearanceCompat(Context, Resource.Style.fontPrimaryLight);

            tableRowDateReceived.AddView(dateReceivedLabel);
            dateReceivedValue = new AppCompatTextView(Context);
            dateReceivedValue.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);

            dateReceivedValue.SetPadding(DistanceNormal, DistanceNone, DistanceNone, DistanceNone);
            tableRowDateReceived.AddView(dateReceivedValue);
            compactLayout.AddView(tableRowDateReceived);

            compactLayout.SetColumnStretchable(1, true);
            compactLayout.SetColumnShrinkable(1, true);
        }

        public override void RefreshView()
        {
            if (MailMessage != null)
                Visibility = ViewStates.Visible;
            else
                Visibility = ViewStates.Gone;

            RefreshViewContent();
        }

        void RefreshViewContent()
        {
            if (MailMessage != null)
            {
                var fromText = MailMessage.From.AsString;
                tableRowFrom.Visibility = string.IsNullOrWhiteSpace(fromText) ? ViewStates.Gone : ViewStates.Visible;
                fromValue.Text = fromText;

                var toText = MailMessage.To.AsString;
                tableRowTo.Visibility = string.IsNullOrWhiteSpace(toText) ? ViewStates.Gone : ViewStates.Visible;
                toValue.Text = toText;

                var ccText = MailMessage.Cc.AsString;
                tableRowCc.Visibility = string.IsNullOrWhiteSpace(ccText) ? ViewStates.Gone : ViewStates.Visible;
                ccValue.Text = ccText;

                var bccText = MailMessage.Bcc.AsString;
                tableRowBcc.Visibility = string.IsNullOrWhiteSpace(bccText) ? ViewStates.Gone : ViewStates.Visible;
                bccValue.Text = bccText;

                var replyToText = MailMessage.ReplyTo.AsString;
                tableRowReplyTo.Visibility = string.IsNullOrWhiteSpace(replyToText) ? ViewStates.Gone : ViewStates.Visible;
                replyToValue.Text = replyToText;

                dateReceivedValue.Text = DateTime.SpecifyKind(MailMessage.Date, DateTimeKind.Unspecified).ConvertDateTimeToTimestampMilliseconds().FormatUserTimestampAsTimeAndDateString(Context);
                tableRowDateReceived.Visibility = ViewStates.Visible;
            }
            else
            {
                tableRowFrom.Visibility = ViewStates.Gone;
                tableRowTo.Visibility = ViewStates.Gone;
                tableRowCc.Visibility = ViewStates.Gone;
                tableRowBcc.Visibility = ViewStates.Gone;
                tableRowReplyTo.Visibility = ViewStates.Gone;
                tableRowDateReceived.Visibility = ViewStates.Gone;

                fromValue.Text = string.Empty;
                toValue.Text = string.Empty;
                ccValue.Text = string.Empty;
                bccValue.Text = string.Empty;
                replyToValue.Text = string.Empty;
                dateReceivedValue.Text = string.Empty;
            }
        }
    }
}