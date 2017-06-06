using System;
using System.Collections.Generic;
using System.Linq;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class DocumentDirectionsSearchView : AbstractButtonsSearchView<SearchDocumentsCriteria>
    {
        readonly CustomButton allButton;
        readonly CustomButton inboxButton;
        readonly CustomButton outboxButton;
        readonly CustomButton draftButton;

        public DocumentDirectionsSearchView(Android.Content.Context context)
            : base(context)
        {
            allButton = new CustomButton(context, Resource.String.search_document_direction_all, AllButtonAction);
            inboxButton = new CustomButton(context, Resource.String.search_document_direction_inbox, OtherButtonsAction);
            outboxButton = new CustomButton(context, Resource.String.search_document_direction_outbox, OtherButtonsAction);
            draftButton = new CustomButton(context, Resource.String.search_document_direction_draft, OtherButtonsAction);

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

            var remainingButtonsList = new List<CustomButton>
            {
                inboxButton,
                outboxButton,
                draftButton
            };
            remainingButtonsList.Remove(button);

            if (remainingButtonsList.All(b => b.Selected == true) && !button.Selected || remainingButtonsList.All(b => b.Selected == false) && button.Selected)
            {
                ResetOtherButtons();
                allButton.UpdateSelectedState(true);
                return false;
            }

            return true;
        }

        void ResetOtherButtons()
        {
            var otherButtonsList = new List<CustomButton>
            {
                inboxButton,
                outboxButton,
                draftButton
            };
            otherButtonsList.ForEach(b => b.UpdateSelectedState(false));
        }

        void ResetButtons()
        {
            allButton.UpdateSelectedState(false);
        }

        public override void Refresh()
        {
            allButton.UpdateSelectedState(false);
            inboxButton.UpdateSelectedState(false);
            outboxButton.UpdateSelectedState(false);
            draftButton.UpdateSelectedState(false);

            var directions = new List<DocumentDirection>
            {
                DocumentDirection.Incoming,
                DocumentDirection.Outgoing,
                DocumentDirection.Draft
            };

            if (Criteria.Directions == null || !Criteria.Directions.Any() || directions.Intersect(Criteria.Directions).Count() == directions.Count)
            {
                allButton.UpdateSelectedState(true);
                return;
            }

            if (Criteria.Directions.Contains(DocumentDirection.Draft))
                draftButton.UpdateSelectedState(true);

            if (Criteria.Directions.Contains(DocumentDirection.Incoming))
                inboxButton.UpdateSelectedState(true);

            if (Criteria.Directions.Contains(DocumentDirection.Outgoing))
                outboxButton.UpdateSelectedState(true);
        }

        public override void UpdateCriteria()
        {
            var selectedDirections = new List<DocumentDirection>();

            if (draftButton.Selected || allButton.Selected)
                selectedDirections.Add(DocumentDirection.Draft);

            if (inboxButton.Selected || allButton.Selected)
                selectedDirections.Add(DocumentDirection.Incoming);

            if (outboxButton.Selected || allButton.Selected)
                selectedDirections.Add(DocumentDirection.Outgoing);

            Criteria.Directions = selectedDirections;
        }
    }
}