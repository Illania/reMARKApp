//
// Project: Mark5.Mobile.Droid
// File: DocumentDirectionsSearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class DocumentDirectionsSearchView : AbstractButtonsSearchView<SearchDocumentsCriteria>
    {
        StyledButton inboxButton;
        StyledButton outboxButton;
        StyledButton draftButton;

        public DocumentDirectionsSearchView(Android.Content.Context context) : base(context)
        {
            inboxButton = new StyledButton(context, Resource.String.search_document_direction_inbox);
            outboxButton = new StyledButton(context, Resource.String.search_document_direction_outbox);
            draftButton = new StyledButton(context, Resource.String.search_document_direction_draft);

            AddButtons(inboxButton, outboxButton, draftButton);
        }

        public override void FromCriteria(SearchDocumentsCriteria criteria)
        {
            //TODO
        }

        public override void ToCriteria(SearchDocumentsCriteria criteria)
        {
            //TODO
        }
    }
}
