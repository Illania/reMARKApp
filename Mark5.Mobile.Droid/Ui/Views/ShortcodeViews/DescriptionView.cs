//
// Project: Mark5.Mobile.Droid
// File: DescriptionView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Droid.Ui.Common;

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
            titleView.SetTextAppearanceCompat(Context, Resource.Style.fontLarge);

            titleView.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkerblue)));
            InnerLayout.AddView(titleView);

            contentView = new AppCompatTextView(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            contentView.SetTextAppearanceCompat(Context, Resource.Style.fontPrimaryLight);

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
