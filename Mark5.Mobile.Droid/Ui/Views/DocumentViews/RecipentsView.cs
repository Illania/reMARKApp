//
// Project: Mark5.Mobile.Droid
// File: RecipentsView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Text.Format;
using Android.Util;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Utilities;
using Android.Widget;
using System.Collections.Generic;

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

        List<TableRow> extraFieldsTableRows = new List<TableRow>();

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
            compactLayout.SetGravity((int)GravityFlags.CenterVertical);
            AddView(compactLayout);

            var size = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 40.0f, Resources.DisplayMetrics) + 0.5f);
            letter = new AppCompatTextView(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(size, size),
                Background = ContextCompat.GetDrawable(Context, Resource.Drawable.circle),
                Gravity = GravityFlags.Center
            };
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                letter.SetTextAppearance(Context, Resource.Style.fontLarge);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                letter.SetTextAppearance(Resource.Style.fontLarge);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
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
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                line1.SetTextAppearance(Context, Resource.Style.fontPrimary);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                line1.SetTextAppearance(Resource.Style.fontPrimary);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            innerLayout.AddView(line1);

            line2 = new AppCompatTextView(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Ellipsize = TextUtils.TruncateAt.End
            };
            line2.SetSingleLine(true);
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                line2.SetTextAppearance(Context, Resource.Style.fontSmallLight);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                line2.SetTextAppearance(Resource.Style.fontSmallLight);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            innerLayout.AddView(line2);

            line3 = new AppCompatTextView(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Ellipsize = TextUtils.TruncateAt.End
            };
            line3.SetSingleLine(true);
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                line3.SetTextAppearance(Context, Resource.Style.fontSmallLight);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                line3.SetTextAppearance(Resource.Style.fontSmallLight);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            innerLayout.AddView(line3);

            line4 = new AppCompatTextView(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Ellipsize = TextUtils.TruncateAt.End
            };
            line4.SetSingleLine(true);
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                line4.SetTextAppearance(Context, Resource.Style.fontSmallLight);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                line4.SetTextAppearance(Resource.Style.fontSmallLight);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            innerLayout.AddView(line4);

            showButton = new AppCompatButton(Context, null, Resource.Style.Widget_AppCompat_Button_Borderless_Colored)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Text = Context.GetString(Resource.String.show_details)
            };
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                showButton.SetTextAppearance(Context, Resource.Style.fontSmall);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                showButton.SetTextAppearance(Resource.Style.fontSmall);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            showButton.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.brown)));
            innerLayout.AddView(showButton);
        }

        void InitializeExtendedView()
        {
            extendedLayout = new TableLayout(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Visibility = ViewStates.Gone,
            };
            AddView(extendedLayout);

            tableRowLine = new TableRow(Context);
            var lineLabel = new AppCompatTextView(Context)
            {
                Text = Context.GetString(Resource.String.lines)
            };
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                lineLabel.SetTextAppearance(Context, Resource.Style.fontPrimary);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                lineLabel.SetTextAppearance(Resource.Style.fontPrimary);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            tableRowLine.AddView(lineLabel);
            lineValue = new AppCompatTextView(Context);
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                lineValue.SetTextAppearance(Context, Resource.Style.fontPrimaryLight);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                lineValue.SetTextAppearance(Resource.Style.fontPrimaryLight);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            lineValue.SetPadding(DistanceNormal, DistanceNone, DistanceNone, DistanceNone);
            tableRowLine.AddView(lineValue);
            extendedLayout.AddView(tableRowLine);

            tableRowReferenceNumber = new TableRow(Context);
            tableRowReferenceNumber.SetPadding(DistanceNone, DistanceSmall, DistanceNone, DistanceNone);
            var referenceNumberLabel = new AppCompatTextView(Context)
            {
                Text = Context.GetString(Resource.String.reference)
            };
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                referenceNumberLabel.SetTextAppearance(Context, Resource.Style.fontPrimary);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                referenceNumberLabel.SetTextAppearance(Resource.Style.fontPrimary);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            tableRowReferenceNumber.AddView(referenceNumberLabel);
            referenceNumberValue = new AppCompatTextView(Context);
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                referenceNumberValue.SetTextAppearance(Context, Resource.Style.fontPrimaryLight);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                referenceNumberValue.SetTextAppearance(Resource.Style.fontPrimaryLight);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            referenceNumberValue.SetPadding(DistanceNormal, DistanceNone, DistanceNone, DistanceNone);
            tableRowReferenceNumber.AddView(referenceNumberValue);
            extendedLayout.AddView(tableRowReferenceNumber);

            tableRowFrom = new TableRow(Context);
            tableRowFrom.SetPadding(DistanceNone, DistanceSmall, DistanceNone, DistanceNone);
            var fromLabel = new AppCompatTextView(Context)
            {
                Text = Context.GetString(Resource.String.from)
            };
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                fromLabel.SetTextAppearance(Context, Resource.Style.fontPrimary);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                fromLabel.SetTextAppearance(Resource.Style.fontPrimary);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            tableRowFrom.AddView(fromLabel);
            fromValue = new AppCompatTextView(Context);
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                fromValue.SetTextAppearance(Context, Resource.Style.fontPrimaryLight);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                fromValue.SetTextAppearance(Resource.Style.fontPrimaryLight);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            fromValue.SetPadding(DistanceNormal, DistanceNone, DistanceNone, DistanceNone);
            tableRowFrom.AddView(fromValue);
            extendedLayout.AddView(tableRowFrom);

            tableRowTo = new TableRow(Context);
            tableRowTo.SetPadding(DistanceNone, DistanceSmall, DistanceNone, DistanceNone);
            var toLabel = new AppCompatTextView(Context)
            {
                Text = Context.GetString(Resource.String.to)
            };
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                toLabel.SetTextAppearance(Context, Resource.Style.fontPrimary);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                toLabel.SetTextAppearance(Resource.Style.fontPrimary);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            tableRowTo.AddView(toLabel);
            toValue = new AppCompatTextView(Context);
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                toValue.SetTextAppearance(Context, Resource.Style.fontPrimaryLight);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                toValue.SetTextAppearance(Resource.Style.fontPrimaryLight);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            toValue.SetPadding(DistanceNormal, DistanceNone, DistanceNone, DistanceNone);
            tableRowTo.AddView(toValue);
            extendedLayout.AddView(tableRowTo);

            tableRowCc = new TableRow(Context);
            tableRowCc.SetPadding(DistanceNone, DistanceSmall, DistanceNone, DistanceNone);
            var ccLabel = new AppCompatTextView(Context)
            {
                Text = Context.GetString(Resource.String.cc)
            };
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                ccLabel.SetTextAppearance(Context, Resource.Style.fontPrimary);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                ccLabel.SetTextAppearance(Resource.Style.fontPrimary);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            tableRowCc.AddView(ccLabel);
            ccValue = new AppCompatTextView(Context);
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                ccValue.SetTextAppearance(Context, Resource.Style.fontPrimaryLight);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                ccValue.SetTextAppearance(Resource.Style.fontPrimaryLight);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            ccValue.SetPadding(DistanceNormal, DistanceNone, DistanceNone, DistanceNone);
            tableRowCc.AddView(ccValue);
            extendedLayout.AddView(tableRowCc);

            tableRowBcc = new TableRow(Context);
            tableRowBcc.SetPadding(DistanceNone, DistanceSmall, DistanceNone, DistanceNone);
            var bccLabel = new AppCompatTextView(Context)
            {
                Text = Context.GetString(Resource.String.bcc)
            };
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                bccLabel.SetTextAppearance(Context, Resource.Style.fontPrimary);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                bccLabel.SetTextAppearance(Resource.Style.fontPrimary);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            tableRowBcc.AddView(bccLabel);
            bccValue = new AppCompatTextView(Context);
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                bccValue.SetTextAppearance(Context, Resource.Style.fontPrimaryLight);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                bccValue.SetTextAppearance(Resource.Style.fontPrimaryLight);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            bccValue.SetPadding(DistanceNormal, DistanceNone, DistanceNone, DistanceNone);
            tableRowBcc.AddView(bccValue);
            extendedLayout.AddView(tableRowBcc);

            tableRowReplyTo = new TableRow(Context);
            tableRowReplyTo.SetPadding(DistanceNone, DistanceSmall, DistanceNone, DistanceNone);
            var replyToLabel = new AppCompatTextView(Context)
            {
                Text = Context.GetString(Resource.String.reply_to)
            };
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                replyToLabel.SetTextAppearance(Context, Resource.Style.fontPrimary);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                replyToLabel.SetTextAppearance(Resource.Style.fontPrimary);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            tableRowReplyTo.AddView(replyToLabel);
            replyToValue = new AppCompatTextView(Context);
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                replyToValue.SetTextAppearance(Context, Resource.Style.fontPrimaryLight);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                replyToValue.SetTextAppearance(Resource.Style.fontPrimaryLight);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            replyToValue.SetPadding(DistanceNormal, DistanceNone, DistanceNone, DistanceNone);
            tableRowReplyTo.AddView(replyToValue);
            extendedLayout.AddView(tableRowReplyTo);

            tableRowReadBy = new TableRow(Context);
            tableRowReadBy.SetPadding(DistanceNone, DistanceSmall, DistanceNone, DistanceNone);
            var readByLabel = new AppCompatTextView(Context)
            {
                Text = Context.GetString(Resource.String.read_by)
            };
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                readByLabel.SetTextAppearance(Context, Resource.Style.fontPrimary);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                readByLabel.SetTextAppearance(Resource.Style.fontPrimary);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            tableRowReadBy.AddView(readByLabel);
            readByValue = new AppCompatTextView(Context);
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                readByValue.SetTextAppearance(Context, Resource.Style.fontPrimaryLight);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                readByValue.SetTextAppearance(Resource.Style.fontPrimaryLight);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            readByValue.SetPadding(DistanceNormal, DistanceNone, DistanceNone, DistanceNone);
            tableRowReadBy.AddView(readByValue);
            extendedLayout.AddView(tableRowReadBy);

            tableRowDateReceived = new TableRow(Context);
            tableRowDateReceived.SetPadding(DistanceNone, DistanceSmall, DistanceNone, DistanceNone);
            var dateReceivedLabel = new AppCompatTextView(Context)
            {
                Text = Context.GetString(Resource.String.date)
            };
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                dateReceivedLabel.SetTextAppearance(Context, Resource.Style.fontPrimary);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                dateReceivedLabel.SetTextAppearance(Resource.Style.fontPrimary);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            tableRowDateReceived.AddView(dateReceivedLabel);
            dateReceivedValue = new AppCompatTextView(Context);
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                dateReceivedValue.SetTextAppearance(Context, Resource.Style.fontPrimaryLight);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                dateReceivedValue.SetTextAppearance(Resource.Style.fontPrimaryLight);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            dateReceivedValue.SetPadding(DistanceNormal, DistanceNone, DistanceNone, DistanceNone);
            tableRowDateReceived.AddView(dateReceivedValue);
            extendedLayout.AddView(tableRowDateReceived);

            tableRowCreator = new TableRow(Context);
            tableRowCreator.SetPadding(DistanceNone, DistanceSmall, DistanceNone, DistanceNone);
            var creatorLabel = new AppCompatTextView(Context)
            {
                Text = Context.GetString(Resource.String.creator)
            };
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                creatorLabel.SetTextAppearance(Context, Resource.Style.fontPrimary);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                creatorLabel.SetTextAppearance(Resource.Style.fontPrimary);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            tableRowCreator.AddView(creatorLabel);
            creatorValue = new AppCompatTextView(Context);
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                creatorValue.SetTextAppearance(Context, Resource.Style.fontPrimaryLight);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                creatorValue.SetTextAppearance(Resource.Style.fontPrimaryLight);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            creatorValue.SetPadding(DistanceNormal, DistanceNone, DistanceNone, DistanceNone);
            tableRowCreator.AddView(creatorValue);
            extendedLayout.AddView(tableRowCreator);

            hideButton = new AppCompatButton(Context, null, Resource.Style.Widget_AppCompat_Button_Borderless_Colored)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Text = Context.GetString(Resource.String.hide_details)
            };
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                hideButton.SetTextAppearance(Context, Resource.Style.fontSmall);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                hideButton.SetTextAppearance(Resource.Style.fontSmall);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
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
                    var addressFrom = DocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.From).FirstOrDefault();
                    var from = string.IsNullOrWhiteSpace(addressFrom.Name) ? addressFrom.Address : addressFrom.Name;
                    var addressesTo = DocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.To || da.AddressType == DocumentAddressType.Cc || da.AddressType == DocumentAddressType.Bcc).OrderBy(da => da.AddressType);
                    var to = string.Join(", ", addressesTo.Select(at => string.IsNullOrWhiteSpace(at.Name) ? at.Address : at.Name));

                    letter.Text = from.Substring(0, 1).ToUpper();
                    line1.Text = from;
                    line2.Text = Context.GetString(Resource.String.to_prefix) + " " + to;
                }
                else
                {
                    var from = Document.Lines.FirstOrDefault().Name;

                    var addressesTo = DocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.To);
                    var to = string.Join(", ", addressesTo.Select(at => string.IsNullOrWhiteSpace(at.Name) ? at.Address : at.Name));

                    letter.Text = from.Substring(0, 1).ToUpper();
                    line1.Text = from;
                    line2.Text = Context.GetString(Resource.String.to_prefix) + " " + to;
                }

                if (Document.ReadByUserNames.Count > 0)
                {
                    line3.Visibility = ViewStates.Visible;
                    line3.Text = Context.GetString(Resource.String.read_by_prefix) + " " + string.Join(", ", Document.ReadByUserNames.Values).ToUpper();
                }
                else
                {
                    line3.Visibility = ViewStates.Gone;
                    line3.Text = string.Empty;
                }

                var dateReceived = DocumentPreview.DateReceived.ToServerTime();

                string dateText;
                if (DateTime.Now.Date == dateReceived.Date)
                {
                    dateText = Context.GetString(Resource.String.today);
                }
                else if (DateTime.Now.AddDays(-1).Date == dateReceived.Date)
                {
                    dateText = Context.GetString(Resource.String.yesterday);
                }
                else
                {
                    var dfo = DateFormat.GetDateFormatOrder(Context);
                    dateText = dateReceived.ToString($"{dfo[0]}{dfo[0]}/{dfo[1]}{dfo[1]}/{dfo[2]}{dfo[2]}{dfo[2]}{dfo[2]}");
                }

                line4.Text = dateText + ", " + (DateFormat.Is24HourFormat(Context) ? dateReceived.ToString("HH:mm") : dateReceived.ToString("hh:mm tt"));
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

                var dateReceived = DocumentPreview.DateReceived.ToServerTime();
                var dfo = DateFormat.GetDateFormatOrder(Context);
                var dateText = dateReceived.ToString($"{dfo[0]}{dfo[0]}/{dfo[1]}{dfo[1]}/{dfo[2]}{dfo[2]}{dfo[2]}{dfo[2]}");
                var timeText = (DateFormat.Is24HourFormat(Context) ? dateReceived.ToString("HH:mm") : dateReceived.ToString("hh:mm tt"));
                var dateReceivedText = timeText + " " + dateText;
                tableRowDateReceived.Visibility = string.IsNullOrWhiteSpace(dateReceivedText) ? ViewStates.Gone : ViewStates.Visible;
                dateReceivedValue.Text = dateReceivedText;

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
                    if (Build.VERSION.SdkInt < BuildVersionCodes.M)
                    {
#pragma warning disable CS0618 // Type or member is obsolete
                        extraFieldLabel.SetTextAppearance(Context, Resource.Style.fontPrimary);
#pragma warning restore CS0618 // Type or member is obsolete
                    }
                    else
                    {
#pragma warning disable XA0001 // Find issues with Android API usage
                        extraFieldLabel.SetTextAppearance(Resource.Style.fontPrimary);
#pragma warning restore XA0001 // Find issues with Android API usage
                    }
                    tableRowExtraField.AddView(extraFieldLabel);
                    var extraFieldValue = new AppCompatTextView(Context)
                    {
                        Text = extraField.Value
                    };
                    if (Build.VERSION.SdkInt < BuildVersionCodes.M)
                    {
#pragma warning disable CS0618 // Type or member is obsolete
                        extraFieldValue.SetTextAppearance(Context, Resource.Style.fontPrimaryLight);
#pragma warning restore CS0618 // Type or member is obsolete
                    }
                    else
                    {
#pragma warning disable XA0001 // Find issues with Android API usage
                        extraFieldValue.SetTextAppearance(Resource.Style.fontPrimaryLight);
#pragma warning restore XA0001 // Find issues with Android API usage
                    }
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
