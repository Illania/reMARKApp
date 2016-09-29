//
// Project: 
// File: ContactViewBaseSubview.cs
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
    public class ContactSubView : LinearLayoutCompat, IContactSubview
    {
        AppCompatTextView titleTextView;
        protected LinearLayoutCompat internalLayout;

        public ContactPreview ContactPreview { get; set; }
        public Contact Contact { get; set; }

        public ContactSubView(Context context) : base(context)
        {
            Orientation = Vertical;
            Visibility = ViewStates.Gone;
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

            internalLayout = new LinearLayoutCompat(context);
            internalLayout.Orientation = Vertical;
            internalLayout.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

            var internalLayoutPadding = ConversionUtils.ConvertDpToPixels(16);
            internalLayout.SetPadding(internalLayoutPadding, internalLayoutPadding, internalLayoutPadding, internalLayoutPadding);
            AddView(internalLayout);

            var divider = new Divider(Context);
            var dividerPadding = ConversionUtils.ConvertDpToPixels(4);
            divider.SetPadding(dividerPadding, 0, dividerPadding, 0);
            AddView(divider);

            titleTextView = new AppCompatTextView(Context);
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                titleTextView.SetTextAppearance(Context, Resource.Style.fontTitle);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                titleTextView.SetTextAppearance(Resource.Style.fontTitle);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            titleTextView.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            internalLayout.AddView(titleTextView);
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
