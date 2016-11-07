//
// Project: Mark5.Mobile.Droid
// File: ContentView.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.Common;

namespace Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public class ContentView : ComposeDocumentView
    {
        readonly AppCompatEditText newContentTextView;
        readonly AppCompatButton showOldContentButton;
        readonly CustomWebView oldContentWebView;

        bool oldContentLoaded;

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
            showOldContentButton.Click += ShowOldContentButton_Click;
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

        public void InsertTemplate(string templateContent, ContentType contentType)
        {
            if (contentType == ContentType.None)
            {
                throw new ArgumentException("Need to use a valide content type");
            }
            else if (contentType == ContentType.PlainText) //TODO eventually need to add spaces before t
            {
                newContentTextView.Text = templateContent;
            }
            else if (contentType == ContentType.Html)
            {
                ISpanned spanned;
                if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                {
#pragma warning disable XA0001 // Find issues with Android API usage
                    spanned = Html.FromHtml(templateContent, FromHtmlOptions.ModeLegacy); //TODO need to check which flag is the more appropriate
#pragma warning restore XA0001 // Find issues with Android API usage
                }
                else
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    spanned = Html.FromHtml(templateContent);
#pragma warning restore CS0618 // Type or member is obsolete
                }
                newContentTextView.EditableText.Append(spanned);
            }
        }

        void ShowOldContentButton_Click(object sender, EventArgs e)
        {
            if (!oldContentLoaded)
            {
                if (PlatformConfig.Preferences.DocumentBodyRequestType == DocumentBodyTypeRequest.PlainTextOnly)
                {
                    oldContentWebView.LoadDataWithBaseURL(null, PreviousDocument.PlainTextBody, "text/plain", "UTF-8", null);
                }
                else if (!string.IsNullOrWhiteSpace(PreviousDocument.HtmlBody))
                {
                    oldContentWebView.LoadDataWithBaseURL(null, PreviousDocument.HtmlBody, "text/html", "UTF-8", null);
                }
                else
                {
                    oldContentWebView.LoadDataWithBaseURL(null, PreviousDocument.PlainTextBody, "text/plain", "UTF-8", null);
                }

                oldContentLoaded = true;
            }

            oldContentWebView.Visibility = ViewStates.Visible;
        }

        public override void RefreshView()
        {
            if (PreviousDocument != null)
            {
                showOldContentButton.Visibility = ViewStates.Visible;
            }
        }


        public override void UpdateDocument()
        {
            throw new NotImplementedException();
        }
    }
}
