//
// Project: Mark5.Mobile.Droid
// File: LinesSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Linq;
using Android.Content;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public class DocumentLinesSearchView : AbstractMultiChoiceSearchView<SearchDocumentsCriteria, Line>
    {

        public DocumentLinesSearchView(Context context)
            : base(context)
        {
            TitleTextView.SetText(Resource.String.search_document_lines);

            NoSelectionText = Resource.String.search_document_lines_none_selected;

            DialogTitle = Resource.String.search_document_lines;
            Values = ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines;
            EqualityComparer = LambdaEqualityComparer<Line>.Create(l => l.Guid);
            DisplayText = l => l.Name;

            UpdateSubtitle();
        }

        public override void FromCriteria(SearchDocumentsCriteria criteria)
        {
            SelectedValues = ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines.Where(l => criteria.LineGuids.Contains(l.Guid)).ToList();
            UpdateSubtitle();
        }

        public override void ToCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.LineGuids = SelectedValues.Select(l => l.Guid).ToList();
        }
    }
}
