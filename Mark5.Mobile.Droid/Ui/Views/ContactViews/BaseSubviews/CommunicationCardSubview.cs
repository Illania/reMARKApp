//
// Project: 
// File: ContactViewBaseSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public abstract class CommunicationCardSubview : BaseCardSubview
    {
        AppCompatTextView titleTextView;
        protected LinearLayoutCompat internalLayout;
        protected LinearLayoutCompat contentLayout;
        protected AppCompatImageView iconImageView;

        protected CommunicationCardSubview(Context context) : base(context)
        {
            Orientation = Vertical;
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

            internalLayout = new LinearLayoutCompat(context);
            internalLayout.Orientation = Horizontal;
            internalLayout.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            AddView(internalLayout);

            var iconImageViewLayout = new LinearLayoutCompat(context);
            iconImageViewLayout.Orientation = Vertical;
            iconImageViewLayout.LayoutParameters = new LayoutParams(ConversionUtils.ConvertDpToPixels(56), ViewGroup.LayoutParams.MatchParent);
            var paddingValue = ConversionUtils.ConvertDpToPixels(16);
            iconImageViewLayout.SetPadding(paddingValue, ConversionUtils.ConvertDpToPixels(8), paddingValue, 0);

            internalLayout.AddView(iconImageViewLayout);

            iconImageView = new AppCompatImageView(context);
            var imageViewSize = ConversionUtils.ConvertDpToPixels(24);
            iconImageViewLayout.AddView(iconImageView, new LayoutParams(imageViewSize, imageViewSize));

            contentLayout = new LinearLayoutCompat(context);
            contentLayout.Orientation = Vertical;
            contentLayout.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent, 1.0f);

            internalLayout.AddView(contentLayout);

            Divider = new PaddingDivider(Context, 56, 0);
            AddView(Divider);

            titleTextView = new AppCompatTextView(Context);
        }

        public void SetTitle(string title)
        {
            titleTextView.Text = title;
        }

        protected class CommunicationCardSubSubview : LinearLayoutCompat
        {
            public CommunicationCardSubSubview(Context context, string primaryText, string descriptionText) : base(context)
            {
                Orientation = Vertical;
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                var paddingValue = ConversionUtils.ConvertDpToPixels(8);
                SetPadding(0, paddingValue, 0, paddingValue);

                var primaryTextView = new AppCompatTextView(context);
                primaryTextView.Text = primaryText;

                primaryTextView.SetTextAppearanceCompat(context, Resource.Style.contactPrimary);

                AddView(primaryTextView, new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

                if (!string.IsNullOrEmpty(descriptionText))
                {
                    var descriptionTextView = new AppCompatTextView(context);
                    descriptionTextView.Text = descriptionText;
                    var descriptionTextViewLayoutParams = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1.0f);
                    descriptionTextViewLayoutParams.TopMargin = ConversionUtils.ConvertDpToPixels(2);
                    descriptionTextView.SetTextAppearanceCompat(context, Resource.Style.contactSecondary);
                    AddView(descriptionTextView, descriptionTextViewLayoutParams);
                }
            }
        }

    }

}
