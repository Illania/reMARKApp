//
// Project: Mark5.Mobile.Droid
// File: PriorityView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.DocumentViews
{
    public class PriorityView : DocumentView
    {

        AppCompatTextView message;

        public PriorityView(Context context)
            : base(context)
        {
            InitializeView();
        }

        void InitializeView()
        {
            SetPadding(DistanceNormal, DistanceNormal, DistanceNormal, DistanceNormal);

            message = new AppCompatTextView(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Gravity = GravityFlags.Center
            };
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                message.SetTextAppearance(Context, Resource.Style.fontSmall);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                message.SetTextAppearance(Resource.Style.fontSmall);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            AddView(message);
        }

        public override void RefreshView()
        {
            if (PlatformConfig.Preferences.DocumentViewPriorityEnabled && DocumentPreview != null)
            {
                switch (DocumentPreview.Priority)
                {
                    case Priority.Urgent:
                        Visibility = ViewStates.Visible;

                        SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.brown)));
                        message.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.lightergray)));
                        message.Text = Context.GetString(Resource.String.high_priority_document);
                        break;
                    case Priority.Low:
                        Visibility = ViewStates.Visible;

                        SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.lightbrown)));
                        message.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.black)));
                        message.Text = Context.GetString(Resource.String.low_priority_document);
                        break;
                    default:
                        Visibility = ViewStates.Gone;

                        Background = null;
                        message.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.black)));
                        message.Text = string.Empty;
                        break;
                }
            }
            else
            {
                Visibility = ViewStates.Gone;

                Background = null;
                message.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.black)));
                message.Text = string.Empty;
            }
        }
    }
}
