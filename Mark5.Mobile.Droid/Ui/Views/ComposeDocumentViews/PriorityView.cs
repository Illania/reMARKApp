using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public class PriorityView : ComposeDocumentView
    {
        readonly AppCompatSpinner prioritySpinner;

        readonly List<Priority> priorities = new List<Priority>
        {
            Priority.Low,
            Priority.Normal,
            Priority.Urgent
        };

        public PriorityView(Context context)
            : base(context)
        {
            SetPadding(DistanceNormal + DistanceSmall, DistanceNormal + DistanceSmall, DistanceNormal + DistanceSmall, DistanceNormal + DistanceSmall);

            var titleTextView = new AppCompatTextView(context)
            {
                LayoutParameters = new LayoutParams(DistanceVeryLarge, ViewGroup.LayoutParams.WrapContent),
            };
            titleTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimaryLight);
            titleTextView.SetText(Resource.String.priority);
            AddView(titleTextView);

            prioritySpinner = new AppCompatSpinner(context);
            var spinnerLayoutParams = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            spinnerLayoutParams.Weight = 1;
            prioritySpinner.LayoutParameters = spinnerLayoutParams;
            var adapter = new ArrayAdapter(context, Android.Resource.Layout.SimpleSpinnerItem, priorities);
            adapter.SetDropDownViewResource(Resource.Layout.support_simple_spinner_dropdown_item);
            prioritySpinner.Adapter = adapter;
            SetPriority(Priority.Normal);
            AddView(prioritySpinner);
        }

        #region Public methods

        public override Task RefreshView()
        {
            if (State != null)
            {
                RestoreState();
                State = null;
                return Task.CompletedTask;
            }

            if (CreationModeFlag == DocumentCreationModeFlag.Edit)
            {
                var possiblePriorities = new[]
                {
                    Priority.Urgent,
                    Priority.Normal,
                    Priority.Low
                };
                var previousDocumentPriority = PreviousDocumentPreview.Priority;

                if (!possiblePriorities.Contains(previousDocumentPriority))
                    previousDocumentPriority = Priority.Normal;

                SetPriority(previousDocumentPriority);
            }

            return Task.CompletedTask;
        }

        public override Task UpdateDocument()
        {
            DocumentPreview.Priority = GetPriority();
            return Task.CompletedTask;
        }

        #endregion

        #region Utilities

        void SetPriority(Priority priority)
        {
            prioritySpinner.SetSelection(priorities.IndexOf(priority));
        }

        Priority GetPriority()
        {
            return priorities[prioritySpinner.SelectedItemPosition];
        }

        #endregion

        #region State related

        void RestoreState()
        {
            var priorityViewState = State as PriorityViewState;
            SetPriority(priorityViewState.SelectedPriority);
        }

        public override IComposeDocumentViewState ReturnState()
        {
            return new PriorityViewState
            {
                SelectedPriority = GetPriority()
            };
        }

        class PriorityViewState : IComposeDocumentViewState
        {
            public Priority SelectedPriority { get; set; }
        }

        #endregion
    }
}