using System;
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

        readonly Priority[] priorities = { Priority.Low, Priority.Normal, Priority.Urgent };

        public PriorityView(Context context)
            : base(context)
        {
            SetPadding(DistanceNormal + DistanceSmall, DistanceNormal + DistanceSmall, DistanceSmall, DistanceNormal + DistanceSmall);

            var titleTextView = new AppCompatTextView(context)
            {
                LayoutParameters = new LayoutParams(DistanceVeryLarge, ViewGroup.LayoutParams.WrapContent),
            };
            titleTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimaryLight);
            titleTextView.SetText(Resource.String.priority);
            AddView(titleTextView);

            prioritySpinner = new AppCompatSpinner(context);
            var spinnerLayoutParams = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent) { Weight = 1 };
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
            if (RestoreWorkingCopy)
            {
                SetPriority(DocumentPreview.Priority);
                return Task.CompletedTask;
            }

            if (DocumentCreationModeFlag == DocumentCreationModeFlag.Edit)
            {
                var previousDocumentPriority = PreviousDocumentPreview.Priority;

                if (previousDocumentPriority != Priority.Low && previousDocumentPriority != Priority.Normal && previousDocumentPriority != Priority.Urgent)
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
            prioritySpinner.SetSelection(Array.IndexOf(priorities, priority));
        }

        Priority GetPriority()
        {
            return priorities[prioritySpinner.SelectedItemPosition];
        }

        #endregion

        #region State related

        void RestoreState()
        {
            var priorityViewState = (PriorityViewState)State;
            SetPriority(priorityViewState.SelectedPriority);
        }

        public override IComposeDocumentViewState GetState()
        {
            return new PriorityViewState { SelectedPriority = GetPriority() };
        }

        class PriorityViewState : IComposeDocumentViewState
        {
            public Priority SelectedPriority { get; set; }
        }

        #endregion
    }
}