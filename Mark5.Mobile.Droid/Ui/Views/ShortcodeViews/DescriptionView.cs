//
// Project: Mark5.Mobile.Droid
// File: DescriptionView.cs
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

namespace Mark5.Mobile.Droid.Ui.Views.ShortcodeViews
{

    public class DescriptionView : ShortcodeView
    {

        AppCompatTextView titleView;
        AppCompatTextView contentView;

        public DescriptionView(Context context)
            : base(context)
        {
            Initialize();
        }

        void Initialize()
        {
            InnerLayout.SetPadding(DistanceLarge, DistanceLarge, DistanceLarge, DistanceLarge);

            titleView = new AppCompatTextView(Context)
            {
                LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                {
                    BottomMargin = DistanceSmall
                },
                Text = Context.GetString(Resource.String.description)
            };
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                titleView.SetTextAppearance(Context, Resource.Style.fontLarge);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                titleView.SetTextAppearance(Resource.Style.fontLarge);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            titleView.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkerblue)));
            InnerLayout.AddView(titleView);

            contentView = new AppCompatTextView(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                contentView.SetTextAppearance(Context, Resource.Style.fontPrimaryLight);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                contentView.SetTextAppearance(Resource.Style.fontPrimaryLight);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            InnerLayout.AddView(contentView);
        }

        public override void RefreshView()
        {
            if (ShortcodePreview != null && !string.IsNullOrWhiteSpace(ShortcodePreview.Description))
            {
                Visibility = ViewStates.Visible;

                contentView.Text = ShortcodePreview.Description;
            }
            else
            {
                Visibility = ViewStates.Gone;

                contentView.Text = string.Empty;
            }
        }
    }
}
