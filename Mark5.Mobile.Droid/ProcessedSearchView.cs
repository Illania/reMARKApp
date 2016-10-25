//
// Project: Mark5.Mobile.Droid
// File: ProcessedSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.DocumentsSearchViews
{

    public class ProcessedSearchView : DocumentsSearchView
    {

        readonly AppCompatTextView processedTitle;
        readonly AppCompatTextView processedSubtitle;

        bool? SelectedProcessed;

        public ProcessedSearchView(Context context)
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
                SelectedProcessed = await Dialogs.ShowSingleSelectDialogAsync(context, Resource.String.search_processed, new List<bool?> { null, true, false }, SelectedProcessed, displayText: TextForValue);
                processedSubtitle.Text = TextForValue(SelectedProcessed);
            };

            processedTitle = new AppCompatTextView(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            processedTitle.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            processedTitle.SetText(Resource.String.search_processed);
            AddView(processedTitle);

            processedSubtitle = new AppCompatTextView(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            processedSubtitle.Text = TextForValue(SelectedProcessed);
            processedSubtitle.SetTextAppearanceCompat(context, Resource.Style.fontSmallLight);
            AddView(processedSubtitle);
        }

        string TextForValue(bool? value)
        {
            if (value == null)
            {
                return Context.GetString(Resource.String.search_processed_none_selected);
            }
            if (value.Value)
            {
                return Context.GetString(Resource.String.search_processed_true);
            }

            return Context.GetString(Resource.String.search_processed_false);
        }

        public override void SetFromCriteria(SearchDocumentsCriteria criteria)
        {
            SelectedProcessed = criteria.Processed;
            processedSubtitle.Text = TextForValue(SelectedProcessed);
        }

        public override void UpdateCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.Processed = SelectedProcessed;
        }
    }
}
