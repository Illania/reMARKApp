//
// Project: Mark5.Mobile.Droid
// File: AbstractMultiChoiceSearchView.cs
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
using System.Linq;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public abstract class AbstractMultiChoiceSearchView<T, Y> : AbstractSearchView<T>
    {

        protected readonly AppCompatTextView TitleTextView;
        protected readonly AppCompatTextView SubtitleTextView;

        protected int NoSelectionText;

        protected int DialogTitle;
        protected List<Y> Values;
        protected List<Y> SelectedValues = new List<Y>();
        protected IEqualityComparer<Y> EqualityComparer;
        protected Func<Y, string> DisplayText;

        protected AbstractMultiChoiceSearchView(Context context)
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
                SelectedValues = await Dialogs.ShowMultiSelectDialogAsync(context, DialogTitle, Values, SelectedValues, EqualityComparer, DisplayText);
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
            if (SelectedValues.Any())
            {
                SubtitleTextView.Text = string.Join(", ", DisplayText == null ? SelectedValues.Select(v => v.ToString()) : SelectedValues.Select(DisplayText));
            }
            else
            {
                SubtitleTextView.SetText(NoSelectionText);
            }
        }
    }
}
