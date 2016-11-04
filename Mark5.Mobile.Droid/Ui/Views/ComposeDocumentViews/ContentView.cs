//
// Project: Mark5.Mobile.Droid
// File: ContentView.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.Common;

namespace Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public class ContentView : ComposeDocumentView
    {
        readonly AppCompatEditText newContentTextView;
        readonly AppCompatButton showOldContentButton;
        readonly CustomWebView oldContentWebView;

        public ContentView(Context context)
            : base(context)
        {
            Orientation = Horizontal;
            SetPadding(DistanceNormal, DistanceNormal, DistanceNormal, DistanceNormal);

            newContentTextView = new AppCompatEditText(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
            };
            newContentTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            newContentTextView.Hint = "Content"; //TODO decide text and put it in resources
            newContentTextView.SetBackgroundColor(Android.Graphics.Color.Transparent);
            AddView(newContentTextView);

            showOldContentButton = new AppCompatButton(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
            };
            showOldContentButton.Text = "Show old content"; //TODO need to use resources and find a good text
            showOldContentButton.Visibility = ViewStates.Gone;
            AddView(showOldContentButton);

            oldContentWebView = new CustomWebView(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
            };
            oldContentWebView.SetWebViewClient(new CustomWebViewClient());
            oldContentWebView.Settings.SetSupportZoom(true);
            oldContentWebView.Settings.BuiltInZoomControls = true;
            oldContentWebView.Settings.DisplayZoomControls = false;
            oldContentWebView.Settings.JavaScriptEnabled = false;
            oldContentWebView.VerticalScrollBarEnabled = false;
            oldContentWebView.HorizontalScrollBarEnabled = false;
            oldContentWebView.Visibility = ViewStates.Gone;
            AddView(oldContentWebView);
        }
    }
}
