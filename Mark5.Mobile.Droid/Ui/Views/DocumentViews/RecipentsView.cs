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

namespace Mark5.Mobile.Droid.Ui.Views.DocumentViews
{

    public class RecipentsView : DocumentView
    {

        AppCompatTextView letter;
        AppCompatTextView line1;
        AppCompatTextView line2;
        AppCompatTextView line3;

        public RecipentsView(Context context)
            : base(context)
        {
            InitializeView();
        }

        void InitializeView()
        {
            Orientation = Vertical;
            SetPadding(PaddingLarge, PaddingLarge, PaddingLarge, PaddingLarge);

            var innerLayout = new LinearLayoutCompat(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Orientation = Horizontal
            };
            innerLayout.SetGravity((int)GravityFlags.CenterVertical);
            AddView(innerLayout);

            var size = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 40.0f, Resources.DisplayMetrics) + 0.5f);
            letter = new AppCompatTextView(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(size, size),
                Background = ContextCompat.GetDrawable(Context, Resource.Drawable.circle),
                Gravity = GravityFlags.Center,
            };
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                letter.SetTextAppearance(Context, Resource.Style.fontCircle);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                letter.SetTextAppearance(Resource.Style.fontCircle);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            letter.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.white)));
            innerLayout.AddView(letter);

            var toFromLayout = new LinearLayoutCompat(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Orientation = Vertical
            };
            toFromLayout.SetPadding(PaddingLarge, PaddingNone, PaddingNone, PaddingNone);
            innerLayout.AddView(toFromLayout);

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
            toFromLayout.AddView(line1);

            line2 = new AppCompatTextView(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Ellipsize = TextUtils.TruncateAt.End
            };
            line2.SetSingleLine(true);

            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                line2.SetTextAppearance(Context, Resource.Style.fontSecondary);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                line2.SetTextAppearance(Resource.Style.fontSecondary);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            toFromLayout.AddView(line2);

            line3 = new AppCompatTextView(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Ellipsize = TextUtils.TruncateAt.End
            };
            line3.SetSingleLine(true);

            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                line3.SetTextAppearance(Context, Resource.Style.fontSecondary);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                line3.SetTextAppearance(Resource.Style.fontSecondary);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            toFromLayout.AddView(line3);
        }

        public override void RefreshView()
        {
            if (DocumentPreview != null && Document != null)
            {
                Visibility = ViewStates.Visible;

                if (DocumentPreview.Direction == DocumentDirection.Incoming)
                {
                    var addressFrom = DocumentPreview.Addresses?.Where(da => da.AddressType == DocumentAddressType.From).FirstOrDefault();
                    var from = string.IsNullOrWhiteSpace(addressFrom?.Name) ? addressFrom?.Address : addressFrom?.Name;

                    var addressTo = DocumentPreview.Addresses?.Where(da => da.AddressType == DocumentAddressType.To).FirstOrDefault();
                    var to = string.IsNullOrWhiteSpace(addressTo?.Name) ? addressTo?.Address : addressTo?.Name;

                    letter.Text = from.Substring(0, 1).ToUpper();
                    line1.Text = from;
                    line2.Text = Context.GetString(Resource.String.to) + " " + to;
                }
                else
                {
                    var from = Document.Lines.FirstOrDefault()?.Name;

                    var addressTo = DocumentPreview.Addresses?.Where(da => da.AddressType == DocumentAddressType.To).FirstOrDefault();
                    var to = string.IsNullOrWhiteSpace(addressTo?.Name) ? addressTo?.Address : addressTo?.Name;

                    letter.Text = from.Substring(0, 1).ToUpper();
                    line1.Text = from;
                    line2.Text = Context.GetString(Resource.String.to) + " " + to;
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

                line3.Text = dateText + ", " + (DateFormat.Is24HourFormat(Context) ? dateReceived.ToString("HH:mm") : dateReceived.ToString("hh:mm tt"));
            }
            else
            {
                Visibility = ViewStates.Gone;

                letter.Text = string.Empty;
                line1.Text = string.Empty;
                line2.Text = string.Empty;
                line3.Text = string.Empty;
            }
        }
    }
}
