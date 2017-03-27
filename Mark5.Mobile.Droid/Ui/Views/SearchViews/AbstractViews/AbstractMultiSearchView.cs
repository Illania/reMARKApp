//
// Project: Mark5.Mobile.Droid
// File: AbstractMultiSearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Collections.Generic;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public abstract class AbstractMultiSearchView<T> : AbstractSearchView<T>
    {
        readonly protected AppCompatSpinner Spinner;
        readonly protected AppCompatTextView TopTextView;
        readonly protected AppCompatEditText BottomEditText;

        protected AbstractMultiSearchView(Android.Content.Context context,
                                         int topTextResId,
                                         int bottomEditResId,
                                         int textArrayResId) : base(context)
        {
            Orientation = Horizontal;
            SetBackgroundColor(BackgroundColorNormalState);

            var searchIconSize = ConversionUtils.ConvertDpToPixels(16f);
            var searchIconView = new AppCompatImageView(context)
            {
                LayoutParameters = new LayoutParams(searchIconSize, searchIconSize)
                {
                    Gravity = (int)GravityFlags.Bottom,
                    RightMargin = DistanceLarge,
                }
            };
            searchIconView.SetImageResource(Resource.Drawable.draft); //TODO put the right one
            AddView(searchIconView);

            var rightLayout = new LinearLayoutCompat(context)
            {
                Orientation = Vertical,
                LayoutParameters = new LayoutParams(0, ViewGroup.LayoutParams.WrapContent)
                {
                    Weight = 1.0f
                }
            };
            AddView(rightLayout);

            var topRightLayout = new LinearLayoutCompat(context)
            {
                Orientation = Horizontal,
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                {
                    Gravity = (int)GravityFlags.Start,
                }
            };
            topRightLayout.DescendantFocusability = DescendantFocusability.BeforeDescendants;
            topRightLayout.FocusableInTouchMode = true;

            rightLayout.AddView(topRightLayout);

            TopTextView = new AppCompatTextView(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent)
                {
                    Gravity = (int)GravityFlags.Start,
                }
            };
            TopTextView.Text = context.GetString(topTextResId);
            TopTextView.Gravity = GravityFlags.CenterVertical;
            TopTextView.SetTextAppearanceCompat(context, TextStyleTopLineResourceId);
            topRightLayout.AddView(TopTextView);

            Spinner = new AppCompatSpinner(context)
            {
                LayoutParameters = new LayoutParams(0, ViewGroup.LayoutParams.WrapContent)
                {
                    Gravity = (int)GravityFlags.End,
                    Weight = 1.0f,
                    LeftMargin = ConversionUtils.ConvertDpToPixels(2),
                }
            };
            Spinner.Adapter = CustomArrayAdapter.Create(context, textArrayResId,
                                                        Resource.Layout.search_spinner_item_multi,
                                                        Resource.Layout.support_simple_spinner_dropdown_item);

            topRightLayout.AddView(Spinner);

            //Cannot define cursor color programmatically otherwise
            BottomEditText = LayoutInflater.From(context).Inflate(Resource.Layout.search_edit_text_layout, null).FindViewById<AppCompatEditText>(Resource.Id.search_edit_text);
            BottomEditText.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            {
                Gravity = (int)GravityFlags.End,
            };
            BottomEditText.SetPadding(0, 0, 0, 0);
            BottomEditText.SetBackgroundColor(Color.Transparent);
            BottomEditText.SetTextAppearanceCompat(context, TextStyleBottomLineResourceId);
            BottomEditText.Hint = context.GetString(bottomEditResId);
            BottomEditText.EditorAction += (sender, e) =>
            {
                if (e.ActionId == ImeAction.Done)
                {
                    BottomEditText.ClearFocus();
                }
            };
            rightLayout.AddView(BottomEditText);
        }
    }
}