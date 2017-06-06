//
// Project: Mark5.Mobile.Droid
// File: AbstractEditableLargeSearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//

using System;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Views.InputMethods;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public abstract class AbstractEditableLargeSearchView<T> : AbstractSearchView<T>
    {
        readonly AppCompatTextView topTextView;
        readonly AppCompatEditText bottomEditText;

        protected AbstractEditableLargeSearchView(Android.Content.Context context, int topTextResId, int bottomEditResId)
            : base(context)
        {
            Orientation = Vertical;
            SetBackgroundColor(BackgroundColorNormalState);

            topTextView = new AppCompatTextView(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent)
                {
                    Gravity = (int) GravityFlags.Start,
                }
            };
            topTextView.Text = context.GetString(topTextResId);
            topTextView.Gravity = GravityFlags.CenterVertical;
            topTextView.SetTextAppearanceCompat(context, TextStyleTopLineResourceId);
            AddView(topTextView);

            //Cannot define cursor color programmatically otherwise
            bottomEditText = LayoutInflater.From(context).Inflate(Resource.Layout.search_edit_text_layout, null).FindViewById<AppCompatEditText>(Resource.Id.search_edit_text);
            bottomEditText.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            {
                Gravity = (int) GravityFlags.End,
            };
            bottomEditText.SetPadding(0, 0, 0, 0);
            bottomEditText.SetBackgroundColor(Color.Transparent);
            bottomEditText.SetTextAppearanceCompat(context, TextStyleBottomLineResourceId);
            bottomEditText.SetHintTextColor(ViewUtilities.GetColorStateList(context, Resource.Drawable.search_edit_text_selector));
            bottomEditText.Hint = context.GetString(bottomEditResId);
            bottomEditText.EditorAction += (sender, e) =>
            {
                if (e.ActionId == ImeAction.Done)
                {
                    bottomEditText.ClearFocus();
                }
            };
            AddView(bottomEditText);
        }

        public void SetText(string text)
        {
            bottomEditText.Text = text;
        }

        public string GetText()
        {
            return bottomEditText.Text;
        }
    }
}