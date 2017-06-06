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
using Mark5.Mobile.Droid.Utilities;

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
            titleView = new AppCompatTextView(Context)
            {
                LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                {
                    BottomMargin = DistanceSmall
                },
                Text = Context.GetString(Resource.String.description)
            };
            titleView.SetPadding(DistanceVeryLarge, 0, DistanceNormal, 0);
            titleView.SetTextAppearanceCompat(Context, Resource.Style.fontLarge);
            titleView.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));
            InnerLayout.AddView(titleView);

            contentView = new AppCompatTextView(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            contentView.SetTextAppearanceCompat(Context, Resource.Style.fontPrimaryLight);
            contentView.SetPadding(DistanceVeryLarge, 0, DistanceNormal, 0);
            InnerLayout.AddView(contentView);

            LongClickable = true;
            LongClick += (sender, e) =>
            {
                if (!string.IsNullOrWhiteSpace(contentView.Text))
                {
                    Integration.CopyToClipboard(Context, contentView.Text);
                }
            };
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