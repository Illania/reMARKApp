//
// Project: Mark5.Mobile.Droid
// File: AbstractMultiView.cs
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
    public class AbstractMultiView : AbstractSearchView
    {
        readonly protected AppCompatSpinner Spinner;
        readonly protected AppCompatTextView TopTextView;
        readonly protected AppCompatEditText BottomEditText;

        public AbstractMultiView(Android.Content.Context context) : base(context)
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
            TopTextView.Text = "Where:";
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

            var priorities = new List<string> { "A", "B", "C" }; //TODO for testing 
            var adapter = new ArrayAdapter(context, Resource.Layout.search_spinner_item_multi, priorities);
            adapter.SetDropDownViewResource(Resource.Layout.support_simple_spinner_dropdown_item);
            Spinner.Adapter = adapter;

            topRightLayout.AddView(Spinner);

            var inflater = LayoutInflater.From(context);
            //Cannot define cursor color programmatically otherwise
            BottomEditText = inflater.Inflate(Resource.Layout.search_edit_text_layout, null).FindViewById<AppCompatEditText>(Resource.Id.search_edit_text);
            BottomEditText.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            {
                Gravity = (int)GravityFlags.End,
            };
            BottomEditText.Hint = "Enter search text";
            BottomEditText.SetPadding(0, 0, 0, 0); //EditText has a default padding
            BottomEditText.SetBackgroundColor(Color.Transparent);
            BottomEditText.SetTextAppearanceCompat(context, TextStyleBottomLineResourceId); //TODO eventually we need to change the color of the hint ?
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