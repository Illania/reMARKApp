//
// Project: Mark5.Mobile.Droid
// File: AbstractSingleChoiceSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public abstract class AbstractSingleChoiceSearchView<T, Y> : AbstractSearchView<T>
    {

        protected readonly AppCompatTextView TitleTextView;
        protected readonly AppCompatTextView SubtitleTextView;

        protected int DialogTitle;
        protected List<Y> Values;
        protected Y SelectedValue;
        protected IEqualityComparer<Y> EqualityComparer;
        protected Func<Y, string> DisplayText;

        protected AbstractSingleChoiceSearchView(Context context)
            : base(context)
        {
            Orientation = Vertical;
            SetPadding(DistanceLarge, DistanceNormal, DistanceLarge, DistanceNormal);

            var typedArray = Context.ObtainStyledAttributes(new int[] { Resource.Attribute.selectableItemBackground });
            SetBackgroundResource(typedArray.GetResourceId(0, 0));
            typedArray.Recycle();

            Clickable = true;
            Click += async (sender, e) =>
            {
                SelectedValue = await Dialogs.ShowSingleSelectDialogAsync(context, DialogTitle, Values, SelectedValue, EqualityComparer, DisplayText);
                UpdateSubtitle();
            };

            TitleTextView = new AppCompatTextView(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            TitleTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            AddView(TitleTextView);

            SubtitleTextView = new AppCompatTextView(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            SubtitleTextView.SetTextAppearanceCompat(context, Resource.Style.fontSmallLight);
            AddView(SubtitleTextView);
        }

        protected void UpdateSubtitle()
        {
            SubtitleTextView.Text = DisplayText == null ? SelectedValue?.ToString() : DisplayText(SelectedValue);
        }
    }
}
