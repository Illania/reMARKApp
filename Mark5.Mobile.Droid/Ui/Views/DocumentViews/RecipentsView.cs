using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.DocumentViews
{
    public class RecipentsView : DocumentView
    {
        TableLayout compactLayout;
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
        AppCompatButton showHideButton;

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

            var typedArray = Context.ObtainStyledAttributes(new int[]
            {
                Resource.Attribute.selectableItemBackground
            });
            SetBackgroundResource(typedArray.GetResourceId(0, 0));
            typedArray.Recycle();

            Clickable = true;
            Click += (sender, e) =>
            {
                extendedLayout.Visibility = extendedLayout.Visibility == ViewStates.Visible ? ViewStates.Gone : ViewStates.Visible;
                showHideButton.Text = extendedLayout.Visibility == ViewStates.Visible ? Context.GetString(Resource.String.hide_details) : Context.GetString(Resource.String.show_details);
            };

            InitializeCompactView();
            InitializeExtendedView();
        }

        void InitializeCompactView()
        {
            compactLayout = new TableLayout(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Visibility = ViewStates.Gone
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

            // Read by
            tableRowReadBy = new TableRow(Context);
            tableRowReadBy.SetPadding(DistanceNone, DistanceSmall, DistanceNone, DistanceNone);
            var readByLabel = new AppCompatTextView(Context)
            {
                Text = Context.GetString(Resource.String.read_by)
            };
            readByLabel.SetTextAppearanceCompat(Context, Resource.Style.fontPrimaryLight);

            tableRowReadBy.AddView(readByLabel);
            readByValue = new AppCompatTextView(Context);
            readByValue.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);

            readByValue.SetPadding(DistanceNormal, DistanceNone, DistanceNone, DistanceNone);
            tableRowReadBy.AddView(readByValue);
            compactLayout.AddView(tableRowReadBy);

            // Show button
            showHideButton = new AppCompatButton(Context, null, Resource.Style.Widget_AppCompat_Button_Borderless_Colored)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Text = Context.GetString(Resource.String.show_details)
            };
            showHideButton.SetTextAppearanceCompat(Context, Resource.Style.fontSmall);

            showHideButton.SetPadding(DistanceNone, DistanceNormal, DistanceNone, DistanceNone);
            showHideButton.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));
            compactLayout.AddView(showHideButton);

            compactLayout.SetColumnStretchable(1, true);
            compactLayout.SetColumnShrinkable(1, true);
        }

        void InitializeExtendedView()
        {
            extendedLayout = new TableLayout(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Visibility = ViewStates.Gone
            };
            AddView(extendedLayout);

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
            extendedLayout.AddView(tableRowCc);

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
            extendedLayout.AddView(tableRowBcc);

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
            extendedLayout.AddView(tableRowReplyTo);

            // Line
            tableRowLine = new TableRow(Context);
            var lineLabel = new AppCompatTextView(Context)
            {
                Text = Context.GetString(Resource.String.lines) + ":"
            };
            lineLabel.SetTextAppearanceCompat(Context, Resource.Style.fontPrimaryLight);

            tableRowLine.AddView(lineLabel);
            lineValue = new AppCompatTextView(Context);
            lineValue.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);

            lineValue.SetPadding(DistanceNormal, DistanceNone, DistanceNone, DistanceNone);
            tableRowLine.AddView(lineValue);
            extendedLayout.AddView(tableRowLine);

            // Reference number
            tableRowReferenceNumber = new TableRow(Context);
            tableRowReferenceNumber.SetPadding(DistanceNone, DistanceSmall, DistanceNone, DistanceNone);
            var referenceNumberLabel = new AppCompatTextView(Context)
            {
                Text = Context.GetString(Resource.String.reference) + ":"
            };
            referenceNumberLabel.SetTextAppearanceCompat(Context, Resource.Style.fontPrimaryLight);

            tableRowReferenceNumber.AddView(referenceNumberLabel);
            referenceNumberValue = new AppCompatTextView(Context);
            referenceNumberValue.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);

            referenceNumberValue.SetPadding(DistanceNormal, DistanceNone, DistanceNone, DistanceNone);
            tableRowReferenceNumber.AddView(referenceNumberValue);
            extendedLayout.AddView(tableRowReferenceNumber);

            // Creator
            tableRowCreator = new TableRow(Context);
            tableRowCreator.SetPadding(DistanceNone, DistanceSmall, DistanceNone, DistanceNone);
            var creatorLabel = new AppCompatTextView(Context)
            {
                Text = Context.GetString(Resource.String.creator)
            };
            creatorLabel.SetTextAppearanceCompat(Context, Resource.Style.fontPrimaryLight);

            tableRowCreator.AddView(creatorLabel);
            creatorValue = new AppCompatTextView(Context);
            creatorValue.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);

            creatorValue.SetPadding(DistanceNormal, DistanceNone, DistanceNone, DistanceNone);
            tableRowCreator.AddView(creatorValue);
            extendedLayout.AddView(tableRowCreator);

            extendedLayout.SetColumnStretchable(1, true);
            extendedLayout.SetColumnShrinkable(1, true);
        }

        public override void RefreshView()
        {
            if (DocumentPreview != null && Document != null)
                Visibility = ViewStates.Visible;
            else
                Visibility = ViewStates.Gone;

            compactLayout.Visibility = compactLayout.Visibility = ViewStates.Visible;
            extendedLayout.Visibility = extendedLayout.Visibility = ViewStates.Gone;

            RefreshViewContent();
        }

        void RefreshViewContent()
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
                        return da.Name;
                    if (!string.IsNullOrWhiteSpace(da.Name) && !string.IsNullOrWhiteSpace(da.Address))
                        return da.Name + " <" + da.Address + ">";
                    if (string.IsNullOrWhiteSpace(da.Name) && !string.IsNullOrWhiteSpace(da.Address))
                        return da.Address;

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

                var readByUsernames = Document.ReadByUserNames.Values.SelectMany(s => s.Split('|')).OrderBy(s => s).Select(s => s.ToUpper());
                var readByText = string.Join(", ", readByUsernames);
                tableRowReadBy.Visibility = string.IsNullOrWhiteSpace(readByText) ? ViewStates.Gone : ViewStates.Visible;
                readByValue.Text = readByText;

                var processedDateReceivedTimestamp = DocumentPreview.DateReceivedTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime().ConvertDateTimeToTimestampMilliseconds();
                tableRowDateReceived.Visibility = DocumentPreview.DateReceivedTimestamp < 0 ? ViewStates.Gone : ViewStates.Visible;
                dateReceivedValue.Text = processedDateReceivedTimestamp.FormatUserTimestampAsTimeAndDateString(Context);

                var creatorText = DocumentPreview.Creator;
                tableRowCreator.Visibility = string.IsNullOrWhiteSpace(creatorText) ? ViewStates.Gone : ViewStates.Visible;
                creatorValue.Text = creatorText;

                extraFieldsTableRows.ForEach(extendedLayout.RemoveView);
                extraFieldsTableRows.Clear();

                if (Document != null)
                {
                    foreach (var extraField in Document.ExtraFields.Where(kv => kv.Key != null && !string.IsNullOrWhiteSpace(kv.Value)).OrderBy(kv => kv.Key.Name))
                    {
                        var tableRowExtraField = new TableRow(Context);
                        tableRowExtraField.SetPadding(DistanceNone, DistanceSmall, DistanceNone, DistanceNone);
                        var extraFieldLabel = new AppCompatTextView(Context)
                        {
                            Text = extraField.Key.Name + ":"
                        };
                        extraFieldLabel.SetTextAppearanceCompat(Context, Resource.Style.fontPrimaryLight);

                        tableRowExtraField.AddView(extraFieldLabel);
                        var extraFieldValue = new AppCompatTextView(Context)
                        {
                            Text = extraField.Value
                        };
                        extraFieldValue.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);

                        extraFieldValue.SetPadding(DistanceNormal, DistanceNone, DistanceNone, DistanceNone);
                        tableRowExtraField.AddView(extraFieldValue);
                        extraFieldsTableRows.Add(tableRowExtraField);
                        extendedLayout.AddView(tableRowExtraField, extendedLayout.ChildCount - 1);
                    }
                }
                else
                {
                    extraFieldsTableRows.ForEach(extendedLayout.RemoveView);
                    extraFieldsTableRows.Clear();
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