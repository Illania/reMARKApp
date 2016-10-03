//
// Project: 
// File: ContactViewBaseTextSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public class DescriptionCardSubview : LinearLayoutCompat, IContactSubview
    {
        AppCompatTextView titleTextView;
        AppCompatTextView contentTextView;

        public ContactPreview ContactPreview { get; set; }
        public Contact Contact { get; set; }

        public DescriptionCardSubview(Context context) : base(context)
        {
            Orientation = Vertical;
            Visibility = ViewStates.Visible;
            SetPadding(ConversionUtils.ConvertDpToPixels(24), 0, 0, 0);
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

            titleTextView = new AppCompatTextView(context);
            titleTextView.SetTextAppearanceCompat(context, Resource.Style.contactPrimary);
            var titleTextViewLayoutParams = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            titleTextViewLayoutParams.TopMargin = ConversionUtils.ConvertDpToPixels(16);
            AddView(titleTextView, titleTextViewLayoutParams);

            contentTextView = new AppCompatTextView(context);
            contentTextView.SetTextAppearanceCompat(context, Resource.Style.contactSecondary);
            var contentTextViewLayoutParams = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            contentTextViewLayoutParams.TopMargin = ConversionUtils.ConvertDpToPixels(3);
            contentTextViewLayoutParams.BottomMargin = ConversionUtils.ConvertDpToPixels(16);
            AddView(contentTextView, contentTextViewLayoutParams);

            var divider = new Divider(Context);
            AddView(divider);
        }

        public void SetContent(string title)
        {
            contentTextView.Text = title;
        }

        public void SetTitle(string title)
        {
            titleTextView.Text = title;
        }

        public virtual void RefreshView()
        {

        }


    }

}
