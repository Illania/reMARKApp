//
// Project: Mark5.Mobile.Droid
// File: DocumentDirectionsSearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class DocumentDirectionsSearchView : AbstractButtonsSearchView<SearchDocumentsCriteria>
    {
        CustomButton allButton;
        CustomButton inboxButton;
        CustomButton outboxButton;
        CustomButton draftButton;

        public DocumentDirectionsSearchView(Android.Content.Context context) : base(context)
        {
            allButton = new CustomButton(context, Resource.String.search_document_direction_all, AllButtonAction);
            inboxButton = new CustomButton(context, Resource.String.search_document_direction_inbox, OtherButtonsAction);
            outboxButton = new CustomButton(context, Resource.String.search_document_direction_outbox, OtherButtonsAction);
            draftButton = new CustomButton(context, Resource.String.search_document_direction_draft, OtherButtonsAction);

            allButton.UpdateSelectedState(true);

            AddButtons(allButton, inboxButton, outboxButton, draftButton);
        }


        bool AllButtonAction(CustomButton button)
        {
            if (allButton.Selected)
                return false;

            ResetOtherButtons();

            return true;
        }

        bool OtherButtonsAction(CustomButton button)
        {
            if (allButton.Selected)
            {
                allButton.UpdateSelectedState(false);
                return true;
            }

            var remainingButtonsList = new List<CustomButton> { inboxButton, outboxButton, draftButton };
            remainingButtonsList.Remove(button);

            if ((remainingButtonsList.All(b => b.Selected == true) && !button.Selected) || (remainingButtonsList.All(b => b.Selected == false) && button.Selected))
            {
                ResetOtherButtons();
                allButton.UpdateSelectedState(true);
                return false;
            }

            return true;
        }

        void ResetOtherButtons()
        {
            var otherButtonsList = new List<CustomButton> { inboxButton, outboxButton, draftButton };
            foreach (var button in otherButtonsList)
            {
                button.UpdateSelectedState(false);
            }
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
