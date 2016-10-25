//
// Project: Mark5.Mobile.Droid
// File: PrioritiesSearchView.cs
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

    public class PrioritiesSearchView : DocumentsSearchView
    {

        readonly AppCompatTextView prioritiesTitle;
        readonly AppCompatTextView prioritiesSubtitle;

        List<Priority> SelectedPriorities = new List<Priority>();

        public PrioritiesSearchView(Context context)
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
                SelectedPriorities = await Dialogs.ShowMultiSelectDialogAsync(context, Resource.String.search_lines, new List<Priority> { Priority.Low, Priority.Normal, Priority.Urgent }, SelectedPriorities);
                UpdateSubtitle();
            };

            prioritiesTitle = new AppCompatTextView(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            prioritiesTitle.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            prioritiesTitle.SetText(Resource.String.search_priorities);
            AddView(prioritiesTitle);

            prioritiesSubtitle = new AppCompatTextView(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            UpdateSubtitle();
            prioritiesSubtitle.SetTextAppearanceCompat(context, Resource.Style.fontSmallLight);
            AddView(prioritiesSubtitle);
        }

        void UpdateSubtitle()
        {
            if (SelectedPriorities.Count < 1)
            {
                prioritiesSubtitle.SetText(Resource.String.search_priorities_none_selected);
            }
            else
            {
                prioritiesSubtitle.Text = string.Join(", ", SelectedPriorities);
            }
        }

        public override void SetFromCriteria(SearchDocumentsCriteria criteria)
        {
            SelectedPriorities = criteria.Priorities;
            UpdateSubtitle();
        }

        public override void UpdateCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.Priorities = SelectedPriorities;
        }
    }
}
