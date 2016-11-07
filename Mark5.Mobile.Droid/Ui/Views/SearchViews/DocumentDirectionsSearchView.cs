//
// Project: Mark5.Mobile.Droid
// File: DirectionsSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public class DocumentDirectionsSearchView : AbstractMultiChoiceSearchView<SearchDocumentsCriteria, DocumentDirection>
    {

        public DocumentDirectionsSearchView(Context context)
            : base(context)
        {
            TitleTextView.SetText(Resource.String.search_document_directions);

            NoSelectionText = Resource.String.search_document_directions_none_selected;

            DialogTitle = Resource.String.search_document_directions;
            Values = new List<DocumentDirection> { DocumentDirection.Incoming, DocumentDirection.Outgoing, DocumentDirection.Draft };

            UpdateSubtitle();
        }

        public override void FromCriteria(SearchDocumentsCriteria criteria)
        {
            SelectedValues = criteria.Directions;
            UpdateSubtitle();
        }

        public override void ToCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.Directions = SelectedValues;
        }
    }
}
