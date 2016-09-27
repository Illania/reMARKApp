//
// Project: Mark5.Mobile.Droid
// File: RecipentsView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Linq;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.DocumentViews
{

    public class RecipentsView : LinearLayoutCompat, IDocumentView
    {

        public DocumentPreview DocumentPreview
        {
            get;
            set;
        }

        public Document Document
        {
            get;
            set;
        }

        AppCompatTextView line1;
        AppCompatTextView line2;

        public RecipentsView(Context context)
            : base(context)
        {
            InitializeView();
        }

        void InitializeView()
        {
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            Visibility = ViewStates.Gone;
            var paddingLarge = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 16.0f, Resources.DisplayMetrics) + 0.5f);
            var paddingSmall = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 8.0f, Resources.DisplayMetrics) + 0.5f);
            SetPadding(paddingLarge, paddingSmall, paddingLarge, paddingSmall);

            line1 = new AppCompatTextView(Context);
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
            AddView(line1, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            line2 = new AppCompatTextView(Context);
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
            AddView(line2, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
        }

        public void RefreshView()
        {
            if (DocumentPreview != null && Document != null)
            {
                Orientation = Vertical;
                Visibility = ViewStates.Visible;

                if (DocumentPreview.Direction == DocumentDirection.Incoming)
                {
                    var addressFrom = DocumentPreview.Addresses?.Where(da => da.AddressType == DocumentAddressType.From).FirstOrDefault();
                    var from = string.IsNullOrWhiteSpace(addressFrom?.Name) ? addressFrom?.Address : addressFrom?.Name;

                    var addressTo = DocumentPreview.Addresses?.Where(da => da.AddressType == DocumentAddressType.To).FirstOrDefault();
                    var to = string.IsNullOrWhiteSpace(addressTo?.Name) ? addressTo?.Address : addressTo?.Name;

                    line1.Text = from;
                    line2.Text = Context.GetString(Resource.String.to) + " " + to;
                }
                else
                {
                    var from = Document.Lines.FirstOrDefault()?.Name;

                    var addressTo = DocumentPreview.Addresses?.Where(da => da.AddressType == DocumentAddressType.To).FirstOrDefault();
                    var to = string.IsNullOrWhiteSpace(addressTo?.Name) ? addressTo?.Address : addressTo?.Name;

                    line1.Text = from;
                    line2.Text = Context.GetString(Resource.String.to) + " " + to;
                }
            }
            else
            {
                Visibility = ViewStates.Gone;
                line1.Text = string.Empty;
                line2.Text = string.Empty;
            }

            Invalidate();
            RequestLayout();
        }
    }
}
