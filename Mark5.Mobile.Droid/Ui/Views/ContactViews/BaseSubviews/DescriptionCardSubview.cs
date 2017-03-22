//
// Project: Mark5.Mobile.Droid
// File: ContactViewBaseTextSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{

    public abstract class DescriptionCardSubview : ContactView
    {

        readonly AppCompatTextView titleTextView;
        readonly AppCompatTextView contentTextView;

        public string Title
        {
            set
            {
                titleTextView.Text = value;
            }
        }

        public string Content
        {
            set
            {
                contentTextView.Text = value;
            }
        }

        protected DescriptionCardSubview(Context context) : base(context)
        {
            Orientation = Vertical;

            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            SetPadding(DistanceVeryLarge, DistanceNormal, DistanceNormal, DistanceNormal);

            LongClickable = true;
            LongClick += (sender, e) =>
            {
                if (!string.IsNullOrWhiteSpace(contentTextView.Text))
                {
                    Integration.CopyToClipboard(context, contentTextView.Text);
                }
            };

            titleTextView = new AppCompatTextView(context);
            titleTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            AddView(titleTextView, new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            contentTextView = new AppCompatTextView(context);
            contentTextView.SetTextAppearanceCompat(context, Resource.Style.fontSmallLight);
            AddView(contentTextView, new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
        }
    }

}
