//
// Project: Mark5.Mobile.Droid
// File: LinesSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.DocumentsSearchViews
{

    public class LinesSearchView : DocumentsSearchView
    {

        readonly AppCompatTextView linesTitle;
        readonly AppCompatTextView linesSubtitle;

        List<Line> SelectedLines = new List<Line>();

        public LinesSearchView(Context context)
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
                SelectedLines = await Dialogs.ShowMultiSelectDialogAsync(context, Resource.String.search_lines, ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines, SelectedLines, LambdaEqualityComparer<Line>.Create(l => l.Guid), l => l.Name);
                UpdateSubtitle();
            };

            linesTitle = new AppCompatTextView(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            linesTitle.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            linesTitle.SetText(Resource.String.search_lines);
            AddView(linesTitle);

            linesSubtitle = new AppCompatTextView(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            UpdateSubtitle();
            linesSubtitle.SetTextAppearanceCompat(context, Resource.Style.fontSmallLight);
            AddView(linesSubtitle);
        }

        void UpdateSubtitle()
        {
            if (SelectedLines.Count < 1)
            {
                linesSubtitle.SetText(Resource.String.search_lines_none_selected);
            }
            else
            {
                linesSubtitle.Text = string.Join(", ", SelectedLines.Select(l => l.Name));
            }
        }

        public override void FromCriteria(SearchDocumentsCriteria criteria)
        {
            SelectedLines = ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines.Where(l => criteria.LineGuids.Contains(l.Guid)).ToList();
            UpdateSubtitle();
        }

        public override void ToCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.LineGuids = SelectedLines.Select(l => l.Guid).ToList();
        }
    }
}
