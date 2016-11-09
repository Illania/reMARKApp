//
// Project: Mark5.Mobile.Droid
// File: RecipientsView.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public class RecipientsView : ComposeDocumentView
    {
        readonly AppCompatEditText contentTextView;
        readonly DocumentAddressType type;

        public RecipientsView(Context context, DocumentAddressType type)
            : base(context)
        {
            this.type = type;

            Orientation = Horizontal;
            SetPadding(DistanceNormal, DistanceNormal, DistanceNormal, DistanceNormal);

            var titleTextView = new AppCompatTextView(context)
            {
                LayoutParameters = new LayoutParams(ConversionUtils.ConvertDpToPixels(40), ViewGroup.LayoutParams.WrapContent),
            };
            titleTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimaryBold);
            titleTextView.Text = type.ToString();
            AddView(titleTextView);


            contentTextView = new AppCompatEditText(context);
            contentTextView.SetPadding(0, 0, 0, 0);
            var contentLayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            contentLayoutParameters.Weight = 1;
            contentTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            contentTextView.SetBackgroundColor(Android.Graphics.Color.Transparent);
            AddView(contentTextView, contentLayoutParameters);
        }

        public override Task RefreshView()
        {
            throw new NotImplementedException();
        }

        public override Task UpdateDocument()
        {
            throw new NotImplementedException();
        }
    }
}
