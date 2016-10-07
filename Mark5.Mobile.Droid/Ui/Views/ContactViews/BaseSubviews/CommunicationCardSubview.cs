//
// Project: 
// File: ContactViewBaseSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Droid.Ui.Views.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public abstract class CommunicationCardSubview : ContactView
    {
        AppCompatTextView titleTextView;
        protected LinearLayoutCompat contentLayout;
        protected AppCompatImageView iconImageView;

        protected CommunicationCardSubview(Context context) : base(context)
        {
            Orientation = Vertical;
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

            var internalLayout = new LinearLayoutCompat(context);
            internalLayout.Orientation = Horizontal;
            internalLayout.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            AddView(internalLayout);

            var iconImageViewLayout = new LinearLayoutCompat(context);
            iconImageViewLayout.Orientation = Vertical;
            iconImageViewLayout.LayoutParameters = new LayoutParams(ConversionUtils.ConvertDpToPixels(56), ViewGroup.LayoutParams.MatchParent);
            iconImageViewLayout.SetPadding(DistanceLarge, DistanceNormal, DistanceLarge, 0);

            internalLayout.AddView(iconImageViewLayout);

            iconImageView = new AppCompatImageView(context);
            iconImageView.SetColorFilter(new Android.Graphics.Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));
            iconImageViewLayout.AddView(iconImageView, new LayoutParams(DistanceVeryLarge, DistanceVeryLarge));

            var contentExternalLayout = new LinearLayoutCompat(context);
            contentExternalLayout.Orientation = Vertical;
            contentExternalLayout.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            internalLayout.AddView(contentExternalLayout);

            contentLayout = new LinearLayoutCompat(context);
            contentLayout.Orientation = Vertical;
            contentLayout.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent, 1.0f);

            contentExternalLayout.AddView(contentLayout);

            Divider = new Divider(Context);
            contentExternalLayout.AddView(Divider);

            titleTextView = new AppCompatTextView(Context);
        }

        public void SetTitle(string title)
        {
            titleTextView.Text = title;
        }

        protected class CommunicationCardSubSubview : LinearLayoutCompat
        {
            public CommunicationCardSubSubview(Context context, string primaryText, string descriptionText, bool primary = false) : base(context)
            {
                Orientation = Vertical;
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                var paddingValue = ConversionUtils.ConvertDpToPixels(8);
                SetPadding(0, paddingValue, 0, paddingValue);

                var primaryTextView = new AppCompatTextView(context);
                primaryTextView.Text = primaryText;
                primaryTextView.SetTextAppearanceCompat(context, primary ? Resource.Style.fontPrimaryBold : Resource.Style.fontPrimary);

                AddView(primaryTextView, new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

                if (!string.IsNullOrEmpty(descriptionText))
                {
                    var descriptionTextView = new AppCompatTextView(context);
                    descriptionTextView.Text = descriptionText;
                    var descriptionTextViewLayoutParams = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1.0f);
                    descriptionTextViewLayoutParams.TopMargin = ConversionUtils.ConvertDpToPixels(2);
                    descriptionTextView.SetTextAppearanceCompat(context, Resource.Style.fontSmallLight); //TODO decide if it makes sense to modify it when primary
                    AddView(descriptionTextView, descriptionTextViewLayoutParams);
                }
            }
        }

    }

}
