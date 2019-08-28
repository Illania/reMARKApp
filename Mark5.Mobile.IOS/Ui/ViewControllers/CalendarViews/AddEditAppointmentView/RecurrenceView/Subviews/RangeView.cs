using System;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews.RecurrenceView
{
    class RangeView : UIStackView, IEditable
    {
        HeaderView headerView;
        EndView endView;

        public RangeView()
        {
            InitializeView();
        }

        void InitializeView()
        {
            Axis = UILayoutConstraintAxis.Vertical;
            Alignment = UIStackViewAlignment.Fill;
            Distribution = UIStackViewDistribution.Fill;
            Spacing = Common.stackViewSpacing;
            TranslatesAutoresizingMaskIntoConstraints = false;

            headerView = new HeaderView();
            endView = new EndView();

            AddArrangedSubview(new SeparatorSubView());
            AddArrangedSubview(headerView);
            AddArrangedSubview(endView);
        }

        public void Refresh()
        {
            headerView.Refresh();
            endView.Refresh();
        }

        public void SetViewModel(RecurrenceInfo ca)
        {
            headerView.SetViewModel(ca);
            endView.SetViewModel(ca);
        }

        class HeaderView : UIView, IEditable
        {
            RecurrenceInfo recInfo;
            DateField startDateField;

            public event EventHandler Updated = delegate { };

            public HeaderView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false;

                var label = new UILabel
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Font = Theme.DefaultFont,
                    TextColor = Theme.DarkGray,
                    Text = "Starts",
                };

                startDateField = new DateField(UpdateStartDate);

                AddSubview(label);
                AddSubview(startDateField);

                AddConstraints(new[]
                {
                        label.LeadingAnchor.ConstraintEqualTo(LeadingAnchor),
                        label.TopAnchor.ConstraintEqualTo(TopAnchor),
                        label.CenterYAnchor.ConstraintEqualTo(CenterYAnchor),
                        label.BottomAnchor.ConstraintEqualTo(BottomAnchor),

                        startDateField.LeadingAnchor.ConstraintEqualTo(label.TrailingAnchor, Common.interviewHorizontalSpacing),
                        startDateField.TopAnchor.ConstraintEqualTo(TopAnchor),
                        startDateField.CenterYAnchor.ConstraintEqualTo(CenterYAnchor),
                    });
            }

            private void UpdateStartDate(DateTime date)
            {
                recInfo.StartDate = date;
            }

            public void Refresh()
            {
                startDateField.SetSelected(recInfo.StartDate);
            }

            public void SetViewModel(RecurrenceInfo ca)
            {
                recInfo = ca;
            }
        }

        class EndView : UIStackView, IEditable
        {
            RecurrenceInfo recInfo;

            RadioButton radioButton1;
            RadioButton radioButton2;
            RadioButton radioButton3;

            NumberField occurrenceField;
            DateField endDateField;

            public EndView()
            {
                Axis = UILayoutConstraintAxis.Vertical;
                Alignment = UIStackViewAlignment.Fill;
                Distribution = UIStackViewDistribution.Fill;
                Spacing = Common.internalStackViewSpacing;
                TranslatesAutoresizingMaskIntoConstraints = false;

                var firstLine = new UIView
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = true,
                };
                firstLine.AddGestureRecognizer(new UITapGestureRecognizer(FirstLine_Tapped));

                radioButton1 = new RadioButton();
                var noEndLabel = new TextLabel { Text = "No end date" };

                firstLine.AddSubview(radioButton1);
                firstLine.AddSubview(noEndLabel);

                firstLine.AddConstraints(new[]
                {
                        radioButton1.LeadingAnchor.ConstraintEqualTo(firstLine.LeadingAnchor),
                        radioButton1.CenterYAnchor.ConstraintEqualTo(firstLine.CenterYAnchor),

                        noEndLabel.CenterYAnchor.ConstraintEqualTo(radioButton1.CenterYAnchor),
                        noEndLabel.LeadingAnchor.ConstraintEqualTo(radioButton1.TrailingAnchor, Common.radioButtonSpacing),
                        noEndLabel.TrailingAnchor.ConstraintEqualTo(firstLine.TrailingAnchor),
                        noEndLabel.BottomAnchor.ConstraintEqualTo(firstLine.BottomAnchor, Common.bottomSpacing),
                    });

                var secondLine = new UIView
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = true,
                };

                var endAfterLabel = new TextLabel { Text = "End after " };
                var occurrencesLabel = new TextLabel { Text = "occurrences" };
                radioButton2 = new RadioButton();

                occurrenceField = new NumberField();
                occurrenceField.EditingChanged += OccurrenceField_TextChanged;

                secondLine.AddGestureRecognizer(new UITapGestureRecognizer(SecondLine_Tapped));
                secondLine.AddSubview(radioButton2);
                secondLine.AddSubview(endAfterLabel);
                secondLine.AddSubview(occurrencesLabel);
                secondLine.AddSubview(occurrenceField);

                secondLine.AddConstraints(new[]
                {
                        radioButton2.LeadingAnchor.ConstraintEqualTo(secondLine.LeadingAnchor),
                        radioButton2.CenterYAnchor.ConstraintEqualTo(secondLine.CenterYAnchor),

                        endAfterLabel.CenterYAnchor.ConstraintEqualTo(radioButton2.CenterYAnchor),
                        endAfterLabel.LeadingAnchor.ConstraintEqualTo(radioButton2.TrailingAnchor, Common.radioButtonSpacing),

                        occurrenceField.CenterYAnchor.ConstraintEqualTo(radioButton2.CenterYAnchor),
                        occurrenceField.LeadingAnchor.ConstraintEqualTo(endAfterLabel.TrailingAnchor, Common.interviewHorizontalSpacing),
                        occurrenceField.BottomAnchor.ConstraintEqualTo(secondLine.BottomAnchor, Common.bottomSpacing),
                        occurrenceField.TopAnchor.ConstraintEqualTo(secondLine.TopAnchor, Common.topSpacing),

                        occurrencesLabel.CenterYAnchor.ConstraintEqualTo(radioButton2.CenterYAnchor),
                        occurrencesLabel.LeadingAnchor.ConstraintEqualTo(occurrenceField.TrailingAnchor, Common.interviewHorizontalSpacing),
                    });

                var thirdLine = new UIView
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = true,
                };

                var endByLabel = new TextLabel { Text = "End by " };
                radioButton3 = new RadioButton();

                endDateField = new DateField(UpdateEndDate);

                thirdLine.AddGestureRecognizer(new UITapGestureRecognizer(ThirdLine_Tapped));
                thirdLine.AddSubview(radioButton3);
                thirdLine.AddSubview(endByLabel);
                thirdLine.AddSubview(endDateField);

                thirdLine.AddConstraints(new[]
                {
                        radioButton3.LeadingAnchor.ConstraintEqualTo(thirdLine.LeadingAnchor),
                        radioButton3.CenterYAnchor.ConstraintEqualTo(thirdLine.CenterYAnchor),

                        endByLabel.CenterYAnchor.ConstraintEqualTo(radioButton3.CenterYAnchor),
                        endByLabel.LeadingAnchor.ConstraintEqualTo(radioButton3.TrailingAnchor, Common.radioButtonSpacing),

                        endDateField.CenterYAnchor.ConstraintEqualTo(radioButton3.CenterYAnchor),
                        endDateField.LeadingAnchor.ConstraintEqualTo(endByLabel.TrailingAnchor, Common.interviewHorizontalSpacing),
                        endDateField.BottomAnchor.ConstraintEqualTo(thirdLine.BottomAnchor, Common.bottomSpacing),
                        endDateField.TopAnchor.ConstraintEqualTo(thirdLine.TopAnchor, Common.topSpacing),
                    });

                AddArrangedSubview(firstLine);
                AddArrangedSubview(secondLine);
                AddArrangedSubview(thirdLine);
            }

            private void FirstLine_Tapped()
            {
                radioButton1.Enabled = true;
                radioButton2.Enabled = false;
                radioButton3.Enabled = false;

                UpdateModel();
            }

            private void SecondLine_Tapped()
            {
                radioButton1.Enabled = false;
                radioButton2.Enabled = true;
                radioButton3.Enabled = false;

                UpdateModel();
            }

            private void ThirdLine_Tapped()
            {
                radioButton1.Enabled = false;
                radioButton2.Enabled = false;
                radioButton3.Enabled = true;

                UpdateModel();
            }

            private void UpdateEndDate(DateTime date)
            {
                recInfo.EndDate = date;
            }

            private void OccurrenceField_TextChanged(object sender, EventArgs e)
            {
                SecondLine_Tapped();
            }

            void UpdateModel()
            {
                recInfo.OccurrenceCount = int.TryParse(occurrenceField.Text, out var p) ? p : 1;
            }

            public void Refresh()
            {
                if (recInfo.Range == RecurrenceRange.NoEndDate)
                {
                    radioButton1.Enabled = true;
                    radioButton2.Enabled = false;
                    radioButton3.Enabled = false;
                }
                else if (recInfo.Range == RecurrenceRange.OccurrenceCount)
                {
                    radioButton1.Enabled = false;
                    radioButton2.Enabled = true;
                    radioButton3.Enabled = false;

                    occurrenceField.Text = recInfo.OccurrenceCount.ToString();
                }
                else if (recInfo.Range == RecurrenceRange.EndByDate)
                {
                    radioButton1.Enabled = false;
                    radioButton2.Enabled = false;
                    radioButton3.Enabled = true;

                    endDateField.SetSelected(recInfo.EndDate);
                }
            }

            public void SetViewModel(RecurrenceInfo vm)
            {
                recInfo = vm;
            }
        }
    }
}
