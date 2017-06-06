//
// Project: Mark5.Mobile.Droid
// File: AbstractDropdownSearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//

using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public abstract class AbstractDropdownSearchView<T> : AbstractSearchView<T>
    {
        readonly protected AppCompatTextView BottomTextView;
        readonly protected AppCompatTextView TextView;
        readonly protected ISearchCriteriaFragment ParentFragment;

        readonly string emptyText;

        protected AbstractDropdownSearchView(Context context, int titleResId, int emptyResId, ISearchCriteriaFragment f)
            : base(context)
        {
            ParentFragment = f;
            emptyText = context.GetString(emptyResId);

            Orientation = Vertical;
            SetBackgroundColor(BackgroundColorNormalState);

            TextView = new AppCompatTextView(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Gravity = GravityFlags.Center
            };
            TextView.Text = context.GetString(titleResId);
            TextView.SetTextAppearanceCompat(context, TextStyleTopLineResourceId);
            AddView(TextView);

            BottomTextView = new AppCompatTextView(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Gravity = GravityFlags.Center
            };
            BottomTextView.SetTextAppearanceCompat(context, TextStyleBottomLineResourceId);
            BottomTextView.Click += (s, e) => ClickAction();
            AddView(BottomTextView);

            Reset();
        }

        public void UpdateBottomTextView(int selectedCount)
        {
            if (selectedCount == 0)
            {
                Reset();
            }
            else
            {
                BottomTextView.Text = selectedCount.ToString();
            }
        }

        public void UpdateBottomTextView(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                Reset();
            }
            else
            {
                BottomTextView.Text = text;
            }
        }

        void Reset()
        {
            BottomTextView.Text = emptyText;
        }

        protected abstract void ClickAction();
    }
}