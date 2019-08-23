using System;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.RecurrenceViews
{
    public class RangeView : RecurrenceParentView
    {
        HeaderView headerView;
        EndView endView;

        public RangeView(Context context) : base(context)
        {
            Orientation = Vertical;

            headerView = new HeaderView(context);
            endView = new EndView(context);

            AddView(new SeparatorView(context));
            AddView(headerView);
            AddView(endView);
        }

        public override void Refresh()
        {
            headerView.Refresh();
            endView.Refresh();
        }

        public override void SetViewModel(RecurrenceInfo ri)
        {
            headerView.SetViewModel(ri);
            endView.SetViewModel(ri);
        }

        class HeaderView : RecurrenceSubView
        {
            DateField dateField;

            public HeaderView(Context context) : base(context)
            {
                Orientation = Horizontal;
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

                var startLabel = new SpecialLabelTextView(context) { Text = "Starts" };
                dateField = new DateField(context, UpdatedStartDate);

                AddView(startLabel);
                AddView(dateField);
            }

            private void UpdatedStartDate(DateTime dt)
            {
                ri.StartDate = dt;
            }

            public override void Refresh()
            {
                dateField.SetDate(ri.StartDate);
            }
        }

        class EndView : RecurrenceSubView
        {
            AppCompatRadioButton radioButton1;
            AppCompatRadioButton radioButton2;
            AppCompatRadioButton radioButton3;

            NumberField occurrenceField;
            DateField endDateField;

            public EndView(Context context) : base(context)
            {
                Orientation = Vertical;
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

                var firstLine = new LinearLayoutCompat(Context)
                {
                    Orientation = Horizontal,
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                };
                firstLine.Click += (a, b) => FirstLine_Click();

                radioButton1 = new AppCompatRadioButton(context);
                var noEndLabel = new LabelTextView(context) { Text = "No end date" };

                firstLine.AddView(radioButton1);
                firstLine.AddView(noEndLabel);

                var secondLine = new LinearLayoutCompat(Context)
                {
                    Orientation = Horizontal,
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                };
                secondLine.Click += (a, b) => SecondLine_Click();

                radioButton2 = new AppCompatRadioButton(context);
                var endAfterLabel = new LabelTextView(context) { Text = "End after" };
                var occurrencesLabel = new LabelTextView(context) { Text = "occurrences" };
                occurrenceField = new NumberField(context);
                occurrenceField.TextChanged += OccurrenceField_TextChanged;

                secondLine.AddView(radioButton2);
                secondLine.AddView(endAfterLabel);
                secondLine.AddView(occurrenceField);
                secondLine.AddView(occurrencesLabel);

                var thirdLine = new LinearLayoutCompat(Context)
                {
                    Orientation = Horizontal,
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                };
                thirdLine.Click += (a, b) => ThirdLine_Click();

                radioButton3 = new AppCompatRadioButton(context);
                var endByLabel = new LabelTextView(context) { Text = "End by" };
                endDateField = new DateField(context, UpdateEndDate);

                thirdLine.AddView(radioButton3);
                thirdLine.AddView(endByLabel);
                thirdLine.AddView(endDateField);

                AddView(firstLine);
                AddView(secondLine);
                AddView(thirdLine);
            }

            private void FirstLine_Click()
            {
                radioButton1.Checked = true;
                radioButton2.Checked = false;
                radioButton3.Checked = false;

                UpdateModel();
            }

            private void SecondLine_Click()
            {
                radioButton1.Checked = false;
                radioButton2.Checked = true;
                radioButton3.Checked = false;

                UpdateModel();
            }

            private void ThirdLine_Click()
            {
                radioButton1.Checked = false;
                radioButton2.Checked = false;
                radioButton3.Checked = true;

                UpdateModel();
            }

            private void UpdateEndDate(DateTime dt)
            {
                ri.EndDate = dt;
                ThirdLine_Click();
            }

            private void OccurrenceField_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
            {
                SecondLine_Click();
            }

            void UpdateModel()
            {
                ri.OccurrenceCount = int.TryParse(occurrenceField.Text, out var p) ? p : 1;
            }

            public override void Refresh()  //TODO need to give default values
            {
                if (ri.Range == RecurrenceRange.NoEndDate)
                {
                    radioButton1.Checked = true;
                    radioButton2.Checked = false;
                    radioButton3.Checked = false;
                }
                else if (ri.Range == RecurrenceRange.OccurrenceCount)
                {
                    radioButton1.Checked = false;
                    radioButton2.Checked = true;
                    radioButton3.Checked = false;

                    occurrenceField.Text = ri.OccurrenceCount.ToString();
                }
                else if (ri.Range == RecurrenceRange.EndByDate)
                {
                    radioButton1.Checked = false;
                    radioButton2.Checked = false;
                    radioButton3.Checked = true;

                    endDateField.SetDate(ri.EndDate);
                }
            }
        }
    }
}
