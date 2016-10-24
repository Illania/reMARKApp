//
// Project: Mark5.Mobile.Droid
// File: PriorityView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Utilities;

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
            message.SetTextAppearanceCompat(Context, Resource.Style.fontSmall);

            AddView(message);
        }

        public override void RefreshView()
        {
            if (DocumentPreview != null)
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
