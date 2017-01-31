//
// Project: Mark5.Mobile.Droid
// File: RecipentsView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.DocumentViews
{

    public class RecipentsView : DocumentView
    {

        LinearLayoutCompat compactLayout;
        AppCompatTextView letter;
        AppCompatTextView line1;
        AppCompatTextView line2;
        AppCompatTextView line3;
        AppCompatTextView line4;
        AppCompatButton showButton;

        TableLayout extendedLayout;
        TableRow tableRowLine;
        TableRow tableRowReferenceNumber;
        TableRow tableRowFrom;
        TableRow tableRowTo;
        TableRow tableRowCc;
        TableRow tableRowBcc;
        TableRow tableRowReplyTo;
        TableRow tableRowReadBy;
        TableRow tableRowDateReceived;
        TableRow tableRowCreator;
        AppCompatTextView lineValue;
        AppCompatTextView referenceNumberValue;
        AppCompatTextView fromValue;
        AppCompatTextView toValue;
        AppCompatTextView ccValue;
        AppCompatTextView bccValue;
        AppCompatTextView replyToValue;
        AppCompatTextView readByValue;
        AppCompatTextView dateReceivedValue;
        AppCompatTextView creatorValue;
        AppCompatButton hideButton;

        readonly List<TableRow> extraFieldsTableRows = new List<TableRow>();

        public RecipentsView(Context context)
            : base(context)
        {
            InitializeView();
        }

        void InitializeView()
        {
            Orientation = Vertical;
            SetPadding(DistanceLarge, DistanceLarge, DistanceLarge, DistanceLarge);

            var typedArray = Context.ObtainStyledAttributes(new int[] { Resource.Attribute.selectableItemBackground });
            SetBackgroundResource(typedArray.GetResourceId(0, 0));
            typedArray.Recycle();

            Clickable = true;
            Click += (sender, e) =>
            {
                compactLayout.Visibility = compactLayout.Visibility == ViewStates.Visible ? ViewStates.Gone : ViewStates.Visible;
                extendedLayout.Visibility = extendedLayout.Visibility == ViewStates.Visible ? ViewStates.Gone : ViewStates.Visible;
            };

            InitializeCompactView();
            InitializeExtendedView();
        }

        void InitializeCompactView()
        {
            compactLayout = new LinearLayoutCompat(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Orientation = Horizontal
            };
            compactLayout.Gravity = (int)GravityFlags.CenterVertical;
            AddView(compactLayout);

            var size = ConversionUtils.ConvertDpToPixels(40.0f);
            letter = new AppCompatTextView(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(size, size),
                Background = ContextCompat.GetDrawable(Context, Resource.Drawable.circle),
                Gravity = GravityFlags.Center
            };
            letter.SetTextAppearanceCompat(Context, Resource.Style.fontLarge);

            letter.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.white)));
            compactLayout.AddView(letter);

            var innerLayout = new LinearLayoutCompat(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Orientation = Vertical
            };
            innerLayout.SetPadding(DistanceLarge, DistanceNone, DistanceNone, DistanceNone);
            compactLayout.AddView(innerLayout);

            line1 = new AppCompatTextView(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Ellipsize = TextUtils.TruncateAt.End
            };
            line1.SetSingleLine(true);
            line1.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);

            innerLayout.AddView(line1);

            line2 = new AppCompatTextView(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Ellipsize = TextUtils.TruncateAt.End
            };
            line2.SetSingleLine(true);
            line2.SetTextAppearanceCompat(Context, Resource.Style.fontSmallLight);

            innerLayout.AddView(line2);

            line3 = new AppCompatTextView(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Ellipsize = TextUtils.TruncateAt.End
            };
            line3.SetSingleLine(true);
            line3.SetTextAppearanceCompat(Context, Resource.Style.fontSmallLight);

            innerLayout.AddView(line3);

            line4 = new AppCompatTextView(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Ellipsize = TextUtils.TruncateAt.End
            };
            line4.SetSingleLine(true);
            line4.SetTextAppearanceCompat(Context, Resource.Style.fontSmallLight);

            innerLayout.AddView(line4);

            showButton = new AppCompatButton(Context, null, Resource.Style.Widget_AppCompat_Button_Borderless_Colored)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Text = Context.GetString(Resource.String.show_details)
            };
            showButton.SetTextAppearanceCompat(Context, Resource.Style.fontSmall);

            showButton.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.brown)));
            innerLayout.AddView(showButton);
        }

        void InitializeExtendedView()
        {
            extendedLayout = new TableLayout(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Visibility = ViewStates.Gone
            };
            AddView(extendedLayout);

            tableRowLine = new TableRow(Context);
            var lineLabel = new AppCompatTextView(Context)
            {
                Text = Context.GetString(Resource.String.lines)
            };
            lineLabel.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);

            tableRowLine.AddView(lineLabel);
            lineValue = new AppCompatTextView(Context);
            lineValue.SetTextAppearanceCompat(Context, Resource.Style.fontPrimaryLight);

            lineValue.SetPadding(DistanceNormal, DistanceNone, DistanceNone, DistanceNone);
            tableRowLine.AddView(lineValue);
            extendedLayout.AddView(tableRowLine);

            tableRowReferenceNumber = new TableRow(Context);
            tableRowReferenceNumber.SetPadding(DistanceNone, DistanceSmall, DistanceNone, DistanceNone);
            var referenceNumberLabel = new AppCompatTextView(Context)
            {
                Text = Context.GetString(Resource.String.reference)
            };
            referenceNumberLabel.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);

            tableRowReferenceNumber.AddView(referenceNumberLabel);
            referenceNumberValue = new AppCompatTextView(Context);
            referenceNumberValue.SetTextAppearanceCompat(Context, Resource.Style.fontPrimaryLight);

            referenceNumberValue.SetPadding(DistanceNormal, DistanceNone, DistanceNone, DistanceNone);
            tableRowReferenceNumber.AddView(referenceNumberValue);
            extendedLayout.AddView(tableRowReferenceNumber);

            tableRowFrom = new TableRow(Context);
            tableRowFrom.SetPadding(DistanceNone, DistanceSmall, DistanceNone, DistanceNone);
            var fromLabel = new AppCompatTextView(Context)
            {
                Text = Context.GetString(Resource.String.from) + ":"
            };
            fromLabel.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);

            tableRowFrom.AddView(fromLabel);
            fromValue = new AppCompatTextView(Context);
            fromValue.SetTextAppearanceCompat(Context, Resource.Style.fontPrimaryLight);

            fromValue.SetPadding(DistanceNormal, DistanceNone, DistanceNone, DistanceNone);
            tableRowFrom.AddView(fromValue);
            extendedLayout.AddView(tableRowFrom);

            tableRowTo = new TableRow(Context);
            tableRowTo.SetPadding(DistanceNone, DistanceSmall, DistanceNone, DistanceNone);
            var toLabel = new AppCompatTextView(Context)
            {
                Text = Context.GetString(Resource.String.to) + ":"
            };
            toLabel.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);

            tableRowTo.AddView(toLabel);
            toValue = new AppCompatTextView(Context);
            toValue.SetTextAppearanceCompat(Context, Resource.Style.fontPrimaryLight);

            toValue.SetPadding(DistanceNormal, DistanceNone, DistanceNone, DistanceNone);
            tableRowTo.AddView(toValue);
            extendedLayout.AddView(tableRowTo);

            tableRowCc = new TableRow(Context);
            tableRowCc.SetPadding(DistanceNone, DistanceSmall, DistanceNone, DistanceNone);
            var ccLabel = new AppCompatTextView(Context)
            {
                Text = Context.GetString(Resource.String.cc) + ":"
            };
            ccLabel.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);

            tableRowCc.AddView(ccLabel);
            ccValue = new AppCompatTextView(Context);
            ccValue.SetTextAppearanceCompat(Context, Resource.Style.fontPrimaryLight);

            ccValue.SetPadding(DistanceNormal, DistanceNone, DistanceNone, DistanceNone);
            tableRowCc.AddView(ccValue);
            extendedLayout.AddView(tableRowCc);

            tableRowBcc = new TableRow(Context);
            tableRowBcc.SetPadding(DistanceNone, DistanceSmall, DistanceNone, DistanceNone);
            var bccLabel = new AppCompatTextView(Context)
            {
                Text = Context.GetString(Resource.String.bcc) + ":"
            };
            bccLabel.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);

            tableRowBcc.AddView(bccLabel);
            bccValue = new AppCompatTextView(Context);
            bccValue.SetTextAppearanceCompat(Context, Resource.Style.fontPrimaryLight);

            bccValue.SetPadding(DistanceNormal, DistanceNone, DistanceNone, DistanceNone);
            tableRowBcc.AddView(bccValue);
            extendedLayout.AddView(tableRowBcc);

            tableRowReplyTo = new TableRow(Context);
            tableRowReplyTo.SetPadding(DistanceNone, DistanceSmall, DistanceNone, DistanceNone);
            var replyToLabel = new AppCompatTextView(Context)
            {
                Text = Context.GetString(Resource.String.reply_to)
            };
            replyToLabel.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);

            tableRowReplyTo.AddView(replyToLabel);
            replyToValue = new AppCompatTextView(Context);
            replyToValue.SetTextAppearanceCompat(Context, Resource.Style.fontPrimaryLight);

            replyToValue.SetPadding(DistanceNormal, DistanceNone, DistanceNone, DistanceNone);
            tableRowReplyTo.AddView(replyToValue);
            extendedLayout.AddView(tableRowReplyTo);

            tableRowReadBy = new TableRow(Context);
            tableRowReadBy.SetPadding(DistanceNone, DistanceSmall, DistanceNone, DistanceNone);
            var readByLabel = new AppCompatTextView(Context)
            {
                Text = Context.GetString(Resource.String.read_by)
            };
            readByLabel.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);

            tableRowReadBy.AddView(readByLabel);
            readByValue = new AppCompatTextView(Context);
            readByValue.SetTextAppearanceCompat(Context, Resource.Style.fontPrimaryLight);

            readByValue.SetPadding(DistanceNormal, DistanceNone, DistanceNone, DistanceNone);
            tableRowReadBy.AddView(readByValue);
            extendedLayout.AddView(tableRowReadBy);

            tableRowDateReceived = new TableRow(Context);
            tableRowDateReceived.SetPadding(DistanceNone, DistanceSmall, DistanceNone, DistanceNone);
            var dateReceivedLabel = new AppCompatTextView(Context)
            {
                Text = Context.GetString(Resource.String.date)
            };
            dateReceivedLabel.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);

            tableRowDateReceived.AddView(dateReceivedLabel);
            dateReceivedValue = new AppCompatTextView(Context);
            dateReceivedValue.SetTextAppearanceCompat(Context, Resource.Style.fontPrimaryLight);

            dateReceivedValue.SetPadding(DistanceNormal, DistanceNone, DistanceNone, DistanceNone);
            tableRowDateReceived.AddView(dateReceivedValue);
            extendedLayout.AddView(tableRowDateReceived);

            tableRowCreator = new TableRow(Context);
            tableRowCreator.SetPadding(DistanceNone, DistanceSmall, DistanceNone, DistanceNone);
            var creatorLabel = new AppCompatTextView(Context)
            {
                Text = Context.GetString(Resource.String.creator)
            };
            creatorLabel.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);

            tableRowCreator.AddView(creatorLabel);
            creatorValue = new AppCompatTextView(Context);
            creatorValue.SetTextAppearanceCompat(Context, Resource.Style.fontPrimaryLight);

            creatorValue.SetPadding(DistanceNormal, DistanceNone, DistanceNone, DistanceNone);
            tableRowCreator.AddView(creatorValue);
            extendedLayout.AddView(tableRowCreator);

            hideButton = new AppCompatButton(Context, null, Resource.Style.Widget_AppCompat_Button_Borderless_Colored)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Text = Context.GetString(Resource.String.hide_details)
            };
            hideButton.SetTextAppearanceCompat(Context, Resource.Style.fontSmall);

            hideButton.SetPadding(DistanceNone, DistanceSmall, DistanceNone, DistanceNone);
            hideButton.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.brown)));
            extendedLayout.AddView(hideButton);

            extendedLayout.SetColumnStretchable(1, true);
            extendedLayout.SetColumnShrinkable(1, true);
        }

        public override void RefreshView()
        {
            if (DocumentPreview != null && Document != null)
            {
                Visibility = ViewStates.Visible;

            }
            else
            {
                Visibility = ViewStates.Gone;
            }

            compactLayout.Visibility = compactLayout.Visibility = ViewStates.Visible;
            extendedLayout.Visibility = extendedLayout.Visibility = ViewStates.Gone;

            RefreshCompactView();
            RefreshExtendedView();
        }

        void RefreshCompactView()
        {
            if (DocumentPreview != null && Document != null)
            {
                if (DocumentPreview.Direction == DocumentDirection.Incoming)
                {
                    var addressFrom = DocumentPreview.Addresses.FirstOrDefault(da => da.AddressType == DocumentAddressType.From);
                    var from = string.IsNullOrWhiteSpace(addressFrom.Name) ? addressFrom.Address : addressFrom.Name;
                    var addressesTo = DocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.To || da.AddressType == DocumentAddressType.Cc || da.AddressType == DocumentAddressType.Bcc).OrderBy(da => da.AddressType);
                    var to = string.Join(", ", addressesTo.Select(at => string.IsNullOrWhiteSpace(at.Name) ? at.Address : at.Name));

                    letter.Text = from?.SafeSubstring(0, 1)?.ToUpper();
                    line1.Text = from;
                    line2.Text = Context.GetString(Resource.String.to_prefix) + " " + to;

                    line1.Visibility = string.IsNullOrWhiteSpace(from) ? ViewStates.Gone : ViewStates.Visible;
                    line2.Visibility = string.IsNullOrWhiteSpace(to) ? ViewStates.Gone : ViewStates.Visible;
                }
                else
                {
                    var from = Document.Lines.FirstOrDefault()?.Name;

                    var addressesTo = DocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.To);
                    var to = string.Join(", ", addressesTo.Select(at => string.IsNullOrWhiteSpace(at.Name) ? at.Address : at.Name));

                    letter.Text = from?.SafeSubstring(0, 1)?.ToUpper();
                    line1.Text = from;
                    line2.Text = Context.GetString(Resource.String.to_prefix) + " " + to;

                    line1.Visibility = string.IsNullOrWhiteSpace(from) ? ViewStates.Gone : ViewStates.Visible;
                    line2.Visibility = string.IsNullOrWhiteSpace(to) ? ViewStates.Gone : ViewStates.Visible;
                }

                if (Document.ReadByUserNames.Count > 0)
                {
                    line3.Visibility = ViewStates.Visible;
                    line3.Text = $"{Context.GetString(Resource.String.read_by_prefix)} {string.Join(", ", Document.ReadByUserNames.Values.OrderBy(s => s)).ToUpper()}";
                }
                else
                {
                    line3.Visibility = ViewStates.Gone;
                    line3.Text = string.Empty;
                }

                line4.Text = DocumentPreview.DateReceivedTimestamp
                    .ConvertTimestampMillisecondsToDateTime()
                    .ConvertUtcToServerTime()
                    .ConvertDateTimeToTimestampMilliseconds()
                    .FormatServerTimestampAsCompactLongDateTimeString(Context);
            }
            else
            {
                letter.Text = string.Empty;
                line1.Text = string.Empty;
                line2.Text = string.Empty;
                line3.Visibility = ViewStates.Gone;
                line3.Text = string.Empty;
                line4.Text = string.Empty;
            }
        }

        void RefreshExtendedView()
        {
            if (DocumentPreview != null && Document != null)
            {
                var lineText = string.Join(", ", Document.Lines.Select(d => d.Name));
                tableRowLine.Visibility = string.IsNullOrWhiteSpace(lineText) ? ViewStates.Gone : ViewStates.Visible;
                lineValue.Text = lineText;

                var referenceNumbertext = DocumentPreview.ReferenceNumber;
                tableRowReferenceNumber.Visibility = string.IsNullOrWhiteSpace(referenceNumbertext) ? ViewStates.Gone : ViewStates.Visible;
                referenceNumberValue.Text = referenceNumbertext;

                Func<DocumentAddress, string> addressText = (da) =>
                {
                    if (!string.IsNullOrWhiteSpace(da.Name) && string.IsNullOrWhiteSpace(da.Address))
                    {
                        return da.Name;
                    }
                    if (!string.IsNullOrWhiteSpace(da.Name) && !string.IsNullOrWhiteSpace(da.Address))
                    {
                        return da.Name + " <" + da.Address + ">";
                    }
                    if (string.IsNullOrWhiteSpace(da.Name) && !string.IsNullOrWhiteSpace(da.Address))
                    {
                        return da.Address;
                    }

                    return string.Empty;
                };

                var fromTextArray = DocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.From).Select(addressText);
                var fromText = string.Join(",\n", fromTextArray);
                tableRowFrom.Visibility = string.IsNullOrWhiteSpace(fromText) ? ViewStates.Gone : ViewStates.Visible;
                fromValue.Text = fromText;

                var toTextArray = DocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.To).Select(addressText);
                var toText = string.Join(",\n", toTextArray);
                tableRowTo.Visibility = string.IsNullOrWhiteSpace(toText) ? ViewStates.Gone : ViewStates.Visible;
                toValue.Text = toText;

                var ccTextArray = DocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.Cc).Select(addressText);
                var ccText = string.Join(",\n", ccTextArray);
                tableRowCc.Visibility = string.IsNullOrWhiteSpace(ccText) ? ViewStates.Gone : ViewStates.Visible;
                ccValue.Text = ccText;

                var bccTextArray = DocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.Bcc).Select(addressText);
                var bccText = string.Join(",\n", bccTextArray);
                tableRowBcc.Visibility = string.IsNullOrWhiteSpace(bccText) ? ViewStates.Gone : ViewStates.Visible;
                bccValue.Text = bccText;

                var replyToTextArray = DocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.ReplyTo).Select(addressText);
                var replyToText = string.Join(",\n", replyToTextArray);
                tableRowReplyTo.Visibility = string.IsNullOrWhiteSpace(replyToText) ? ViewStates.Gone : ViewStates.Visible;
                replyToValue.Text = replyToText;

                var readByText = string.Join(", ", Document.ReadByUserNames.Values).ToUpper();
                tableRowReadBy.Visibility = string.IsNullOrWhiteSpace(readByText) ? ViewStates.Gone : ViewStates.Visible;
                readByValue.Text = readByText;

                var processedDateReceivedTimestamp = DocumentPreview.DateReceivedTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToServerTime().ConvertDateTimeToTimestampMilliseconds();
                tableRowDateReceived.Visibility = DocumentPreview.DateReceivedTimestamp < 0 ? ViewStates.Gone : ViewStates.Visible;
                dateReceivedValue.Text = processedDateReceivedTimestamp.FormatServerTimestampAsTimeAndDateString(Context);

                var creatorText = DocumentPreview.Creator;
                tableRowCreator.Visibility = string.IsNullOrWhiteSpace(creatorText) ? ViewStates.Gone : ViewStates.Visible;
                creatorValue.Text = creatorText;

                extraFieldsTableRows.ForEach(extendedLayout.RemoveView);
                extraFieldsTableRows.Clear();

                foreach (var extraField in Document.ExtraFields.Where(kv => kv.Key != null && !string.IsNullOrWhiteSpace(kv.Value)).OrderBy(kv => kv.Key.Name))
                {
                    var tableRowExtraField = new TableRow(Context);
                    tableRowExtraField.SetPadding(DistanceNone, DistanceSmall, DistanceNone, DistanceNone);
                    var extraFieldLabel = new AppCompatTextView(Context)
                    {
                        Text = extraField.Key.Name + ":"
                    };
                    extraFieldLabel.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);

                    tableRowExtraField.AddView(extraFieldLabel);
                    var extraFieldValue = new AppCompatTextView(Context)
                    {
                        Text = extraField.Value
                    };
                    extraFieldValue.SetTextAppearanceCompat(Context, Resource.Style.fontPrimaryLight);

                    extraFieldValue.SetPadding(DistanceNormal, DistanceNone, DistanceNone, DistanceNone);
                    tableRowExtraField.AddView(extraFieldValue);
                    extendedLayout.AddView(tableRowExtraField, extendedLayout.ChildCount - 1);
                }
            }
            else
            {
                tableRowLine.Visibility = ViewStates.Gone;
                tableRowReferenceNumber.Visibility = ViewStates.Gone;
                tableRowFrom.Visibility = ViewStates.Gone;
                tableRowTo.Visibility = ViewStates.Gone;
                tableRowCc.Visibility = ViewStates.Gone;
                tableRowBcc.Visibility = ViewStates.Gone;
                tableRowReplyTo.Visibility = ViewStates.Gone;
                tableRowReadBy.Visibility = ViewStates.Gone;
                tableRowDateReceived.Visibility = ViewStates.Gone;
                tableRowCreator.Visibility = ViewStates.Gone;

                lineValue.Text = string.Empty;
                referenceNumberValue.Text = string.Empty;
                fromValue.Text = string.Empty;
                toValue.Text = string.Empty;
                ccValue.Text = string.Empty;
                bccValue.Text = string.Empty;
                replyToValue.Text = string.Empty;
                readByValue.Text = string.Empty;
                dateReceivedValue.Text = string.Empty;
                creatorValue.Text = string.Empty;

                extraFieldsTableRows.ForEach(extendedLayout.RemoveView);
                extraFieldsTableRows.Clear();
            }
        }
    }
}
