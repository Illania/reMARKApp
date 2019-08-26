using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews.RecurrenceView
{
    class PatternView : UIStackView, IEditable
    {
        PatternHeaderView patternHeaderView;
        DailyView dailyView;
        WeeklyView weeklyView;
        MonthlyView monthlyView;
        YearlyView yearlyView;

        public PatternView()
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

            patternHeaderView = new PatternHeaderView();
            patternHeaderView.Updated += PatternHeaderView_Updated;
            dailyView = new DailyView();
            weeklyView = new WeeklyView();
            monthlyView = new MonthlyView();
            yearlyView = new YearlyView();

            AddArrangedSubview(patternHeaderView);
            AddArrangedSubview(new SeparatorSubView());
            AddArrangedSubview(dailyView);
            AddArrangedSubview(weeklyView);
            AddArrangedSubview(monthlyView);
            AddArrangedSubview(yearlyView);
        }

        public void Refresh()
        {
            Subviews.OfType<IEditable>().ToList().ForEach(a => a.Refresh());
        }

        public void SetViewModel(AddEditAppointmentViewModel ca)
        {
            Subviews.OfType<IEditable>().ToList().ForEach(a => a.SetViewModel(ca));
        }

        private void PatternHeaderView_Updated(object sender, EventArgs e)
        {
            dailyView.Refresh();
            weeklyView.Refresh();
            monthlyView.Refresh();
            yearlyView.Refresh();

        }

        class PatternHeaderView : UIView, IEditable
        {
            AddEditAppointmentViewModel viewModel;
            TypePicker typeField;

            public event EventHandler Updated = delegate { };

            public PatternHeaderView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false;

                var label = new UILabel
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Font = Theme.DefaultFont,
                    TextColor = Theme.DarkGray,
                    Text = "Repeats",
                };

                typeField = new TypePicker(UpdateModel);

                AddSubview(label);
                AddSubview(typeField);

                AddConstraints(new[]
                {
                        label.LeadingAnchor.ConstraintEqualTo(LeadingAnchor),
                        label.TopAnchor.ConstraintEqualTo(TopAnchor),
                        label.CenterYAnchor.ConstraintEqualTo(CenterYAnchor),
                        label.BottomAnchor.ConstraintEqualTo(BottomAnchor),

                        typeField.LeadingAnchor.ConstraintEqualTo(label.TrailingAnchor, Common.interviewHorizontalSpacing),
                        typeField.TopAnchor.ConstraintEqualTo(TopAnchor),
                        typeField.CenterYAnchor.ConstraintEqualTo(CenterYAnchor),
                    });
            }

            void UpdateModel(RecurrenceType rec)
            {
                viewModel.RecurrenceInfo.Type = rec;
                Updated(this, EventArgs.Empty);
                Refresh();
            }

            public void Refresh()
            {
                typeField.SetSelected(viewModel.RecurrenceInfo.Type);
            }

            public void SetViewModel(AddEditAppointmentViewModel ca)
            {
                viewModel = ca;
            }
        }

        class DailyView : UIStackView, IEditable
        {
            AddEditAppointmentViewModel viewModel;

            RadioButton radioButton1;
            RadioButton radioButton2;
            NumberField daysTextField;

            public DailyView()
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

                var everyLabel = new TextLabel { Text = "Every" };
                var daysLabel = new TextLabel { Text = "day(s)", };
                radioButton1 = new RadioButton();

                daysTextField = new NumberField();
                daysTextField.EditingChanged += DaysTextField_EditingChanged;

                firstLine.AddGestureRecognizer(new UITapGestureRecognizer(FirstLine_Tapped));
                firstLine.AddSubview(radioButton1);
                firstLine.AddSubview(everyLabel);
                firstLine.AddSubview(daysLabel);
                firstLine.AddSubview(daysTextField);

                firstLine.AddConstraints(new[]
                {
                        radioButton1.LeadingAnchor.ConstraintEqualTo(firstLine.LeadingAnchor),
                        radioButton1.CenterYAnchor.ConstraintEqualTo(firstLine.CenterYAnchor),

                        everyLabel.CenterYAnchor.ConstraintEqualTo(radioButton1.CenterYAnchor),
                        everyLabel.LeadingAnchor.ConstraintEqualTo(radioButton1.TrailingAnchor, Common.radioButtonSpacing),

                        daysTextField.CenterYAnchor.ConstraintEqualTo(radioButton1.CenterYAnchor),
                        daysTextField.LeadingAnchor.ConstraintEqualTo(everyLabel.TrailingAnchor, Common.interviewHorizontalSpacing),
                        daysTextField.BottomAnchor.ConstraintEqualTo(firstLine.BottomAnchor, Common.bottomSpacing),
                        daysTextField.TopAnchor.ConstraintEqualTo(firstLine.TopAnchor, Common.topSpacing),

                        daysLabel.CenterYAnchor.ConstraintEqualTo(radioButton1.CenterYAnchor),
                        daysLabel.LeadingAnchor.ConstraintEqualTo(daysTextField.TrailingAnchor, Common.interviewHorizontalSpacing),
                    });

                var secondLine = new UIView
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = true,
                };
                secondLine.AddGestureRecognizer(new UITapGestureRecognizer(SecondLine_Tapped));

                radioButton2 = new RadioButton();

                var weekedaysLabel = new TextLabel { Text = "Every weekday" };

                secondLine.AddSubview(radioButton2);
                secondLine.AddSubview(weekedaysLabel);

                secondLine.AddConstraints(new[]
                {
                        radioButton2.LeadingAnchor.ConstraintEqualTo(secondLine.LeadingAnchor),
                        radioButton2.CenterYAnchor.ConstraintEqualTo(secondLine.CenterYAnchor),

                        weekedaysLabel.CenterYAnchor.ConstraintEqualTo(radioButton2.CenterYAnchor),
                        weekedaysLabel.LeadingAnchor.ConstraintEqualTo(radioButton2.TrailingAnchor, Common.radioButtonSpacing),
                        weekedaysLabel.TrailingAnchor.ConstraintEqualTo(secondLine.TrailingAnchor),
                        weekedaysLabel.BottomAnchor.ConstraintEqualTo(secondLine.BottomAnchor, Common.bottomSpacing),
                    });

                AddArrangedSubview(firstLine);
                AddArrangedSubview(secondLine);
            }

            private void FirstLine_Tapped()
            {
                radioButton1.Enabled = true;
                radioButton2.Enabled = false;
                UpdateModel();
            }

            private void SecondLine_Tapped()
            {
                radioButton1.Enabled = false;
                radioButton2.Enabled = true;
                UpdateModel();
            }

            void UpdateModel()
            {
                if (radioButton1.Enabled)
                {
                    viewModel.RecurrenceInfo.WeekDays = WeekDays.EveryDay;
                    viewModel.RecurrenceInfo.Periodicity = int.TryParse(daysTextField.Text, out var s) ? s : 1;
                }
                else if (radioButton2.Enabled)
                {
                    viewModel.RecurrenceInfo.WeekDays = WeekDays.WorkDays;
                }
            }

            void DaysTextField_EditingChanged(object sender, EventArgs e)
            {
                FirstLine_Tapped();
            }

            public void Refresh()
            {
                if (viewModel.RecurrenceInfo.Type != RecurrenceType.Daily)
                {
                    if (!Hidden)
                        Hidden = true;
                    return;
                }

                if (Hidden)
                    Hidden = false;

                if (viewModel.RecurrenceInfo.WeekDays == WeekDays.EveryDay)
                {
                    radioButton1.Enabled = true;
                    radioButton2.Enabled = false;
                    daysTextField.Text = viewModel.RecurrenceInfo.Periodicity.ToString();
                }
                else if (viewModel.RecurrenceInfo.WeekDays == WeekDays.WorkDays)
                {
                    radioButton1.Enabled = false;
                    radioButton2.Enabled = true;
                }
            }

            public void SetViewModel(AddEditAppointmentViewModel ca)
            {
                viewModel = ca;
            }
        }

        class WeeklyView : UIView, IEditable
        {
            AddEditAppointmentViewModel viewModel;

            const float cellheight = 44f;

            NumberField weeksTextField;
            UITableView weekdaysTableView;

            public WeeklyView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false;

                var firstLine = new UIView
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = true,
                };

                var recurLabel = new TextLabel
                {
                    Text = "Recur every",
                };

                var weeksLabel = new TextLabel
                {
                    Text = "week(s) on:",
                };

                weeksTextField = new NumberField();
                weeksTextField.EditingChanged += WeeksTextField_EditingChanged;

                firstLine.AddSubview(recurLabel);
                firstLine.AddSubview(weeksLabel);
                firstLine.AddSubview(weeksTextField);

                firstLine.AddConstraints(new[]
                {
                        recurLabel.LeadingAnchor.ConstraintEqualTo(firstLine.LeadingAnchor),
                        recurLabel.CenterYAnchor.ConstraintEqualTo(firstLine.CenterYAnchor),

                        weeksTextField.LeadingAnchor.ConstraintEqualTo(recurLabel.TrailingAnchor, Common.interviewHorizontalSpacing),
                        weeksTextField.BottomAnchor.ConstraintEqualTo(firstLine.BottomAnchor, Common.bottomSpacing),
                        weeksTextField.TopAnchor.ConstraintEqualTo(firstLine.TopAnchor, Common.topSpacing),
                        weeksTextField.CenterYAnchor.ConstraintEqualTo(recurLabel.CenterYAnchor),

                        weeksLabel.CenterYAnchor.ConstraintEqualTo(recurLabel.CenterYAnchor),
                        weeksLabel.LeadingAnchor.ConstraintEqualTo(weeksTextField.TrailingAnchor, Common.interviewHorizontalSpacing),
                    });

                weekdaysTableView = new UITableView
                {
                    BackgroundColor = UIColor.Clear,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    AllowsSelection = true,
                    AllowsMultipleSelection = true,
                    RowHeight = cellheight,
                    Source = new WeekdaysSource(this),
                    ScrollEnabled = false,
                    CellLayoutMarginsFollowReadableWidth = false,
                    SeparatorInset = UIEdgeInsets.Zero,
                    SeparatorStyle = UITableViewCellSeparatorStyle.None
                };

                AddSubview(firstLine);
                AddSubview(weekdaysTableView);

                AddConstraints(new[]
                {
                        firstLine.LeadingAnchor.ConstraintEqualTo(LeadingAnchor),
                        firstLine.TopAnchor.ConstraintEqualTo(TopAnchor),
                        firstLine.TrailingAnchor.ConstraintEqualTo(weekdaysTableView.TrailingAnchor),

                        weekdaysTableView.TopAnchor.ConstraintEqualTo(firstLine.BottomAnchor,10f ),
                        weekdaysTableView.LeadingAnchor.ConstraintEqualTo(firstLine.LeadingAnchor),
                        weekdaysTableView.BottomAnchor.ConstraintEqualTo(BottomAnchor),
                        weekdaysTableView.TrailingAnchor.ConstraintEqualTo(TrailingAnchor),
                        weekdaysTableView.HeightAnchor.ConstraintEqualTo(cellheight * 7),
                    });
            }

            private void WeeksTextField_EditingChanged(object sender, EventArgs e)
            {
                UpdateModel();
            }

            void UpdateModel()
            {
                viewModel.RecurrenceInfo.Periodicity = int.TryParse(weeksTextField.Text, out var i) ? i : 1;
            }

            public void Refresh()
            {
                if (viewModel.RecurrenceInfo.Type != RecurrenceType.Weekly)
                {
                    if (!Hidden)
                        Hidden = true;
                    return;
                }

                if (Hidden)
                    Hidden = false;

                weeksTextField.Text = viewModel.RecurrenceInfo.Periodicity.ToString();
                var selected = Common.weekDays.Where(w => viewModel.RecurrenceInfo.WeekDays.HasFlag(w)).ToList();
                weekdaysTableView.ReloadData();
                (weekdaysTableView.Source as WeekdaysSource).SetSelected(weekdaysTableView, selected);
            }

            public void SetViewModel(AddEditAppointmentViewModel ca)
            {
                viewModel = ca;
            }

            public void ChangedSelection(WeekDays weekday, bool selected)
            {
                if (selected)
                    viewModel.RecurrenceInfo.WeekDays |= weekday;
                else
                    viewModel.RecurrenceInfo.WeekDays &= ~weekday;

            }

            class WeekdaysSource : UITableViewSource
            {
                WeeklyView parentView;
                List<WeekDays> data = Common.weekDays;

                readonly HashSet<WeekDays> selectedItems = new HashSet<WeekDays>();

                public WeekdaysSource(WeeklyView parentView)
                {
                    this.parentView = parentView;
                }

                public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
                {
                    var weekDay = data[indexPath.Row];
                    var cell = tableView.DequeueReusableCell("cell") ?? UITableViewCellUtilities.CreateDefault("cell", UITableViewCellSelectionStyle.None);
                    cell.TextLabel.Text = weekDay.ToFriendlyString();
                    cell.Accessory = selectedItems.Contains(weekDay) ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
                    cell.BackgroundColor = UIColor.Clear;
                    return cell;
                }

                public override nint RowsInSection(UITableView tableview, nint section) => data.Count;

                public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
                {
                    var cell = tableView.CellAt(indexPath);
                    if (cell == null)
                        return;

                    tableView.CellAt(indexPath).Accessory = UITableViewCellAccessory.Checkmark;
                    selectedItems.Add(data[indexPath.Row]);

                    parentView.ChangedSelection(data[indexPath.Row], true);
                }

                public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
                {
                    var cell = tableView.CellAt(indexPath);
                    if (cell == null)
                        return;

                    if (selectedItems.Count == 1)
                        return;

                    tableView.CellAt(indexPath).Accessory = UITableViewCellAccessory.None;
                    selectedItems.Remove(data[indexPath.Row]);

                    parentView.ChangedSelection(data[indexPath.Row], false);
                }

                public void SetSelected(UITableView tableView, IEnumerable<WeekDays> wdays)
                {
                    foreach (var w in wdays)
                    {
                        selectedItems.Add(w);
                        var index = data.FindIndex(s => s == w);
                        tableView.SelectRow(NSIndexPath.FromRowSection(index, 0), false, UITableViewScrollPosition.None);
                    }

                }
            }

        }

        class MonthlyView : UIStackView, IEditable
        {
            AddEditAppointmentViewModel viewModel;

            RadioButton radioButton1;
            RadioButton radioButton2;
            NumberField dayTextField;
            NumberField monthsField1;
            NumberField monthsField2;
            ExtendedWeekDayPicker weekDayField;
            WeekOfMonthPicker weekOfMonthField;

            public MonthlyView()
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

                radioButton1 = new RadioButton();

                var dayLabel = new TextLabel
                {
                    Text = "Day",
                };

                var ofEveryLabel1 = new TextLabel
                {
                    Text = "of every",
                };

                var monthsLabel1 = new TextLabel
                {
                    Text = "month(s)",
                };

                dayTextField = new NumberField();
                dayTextField.EditingChanged += DaysTextField_EditingChanged;

                monthsField1 = new NumberField();
                monthsField1.EditingChanged += MonthsField1_EditingChanged;

                firstLine.AddGestureRecognizer(new UITapGestureRecognizer(FirstLine_Tapped));
                firstLine.AddSubview(radioButton1);
                firstLine.AddSubview(dayLabel);
                firstLine.AddSubview(ofEveryLabel1);
                firstLine.AddSubview(monthsLabel1);
                firstLine.AddSubview(monthsField1);
                firstLine.AddSubview(dayTextField);

                firstLine.AddConstraints(new[]
                {
                        radioButton1.LeadingAnchor.ConstraintEqualTo(firstLine.LeadingAnchor),
                        radioButton1.CenterYAnchor.ConstraintEqualTo(firstLine.CenterYAnchor),

                        dayLabel.CenterYAnchor.ConstraintEqualTo(radioButton1.CenterYAnchor),
                        dayLabel.LeadingAnchor.ConstraintEqualTo(radioButton1.TrailingAnchor, Common.radioButtonSpacing),

                        dayTextField.CenterYAnchor.ConstraintEqualTo(radioButton1.CenterYAnchor),
                        dayTextField.LeadingAnchor.ConstraintEqualTo(dayLabel.TrailingAnchor, Common.interviewHorizontalSpacing),
                        dayTextField.BottomAnchor.ConstraintEqualTo(firstLine.BottomAnchor, Common.bottomSpacing),
                        dayTextField.TopAnchor.ConstraintEqualTo(firstLine.TopAnchor, Common.topSpacing),

                        ofEveryLabel1.CenterYAnchor.ConstraintEqualTo(radioButton1.CenterYAnchor),
                        ofEveryLabel1.LeadingAnchor.ConstraintEqualTo(dayTextField.TrailingAnchor, Common.interviewHorizontalSpacing),

                        monthsField1.CenterYAnchor.ConstraintEqualTo(radioButton1.CenterYAnchor),
                        monthsField1.LeadingAnchor.ConstraintEqualTo(ofEveryLabel1.TrailingAnchor, Common.interviewHorizontalSpacing),
                        monthsField1.BottomAnchor.ConstraintEqualTo(firstLine.BottomAnchor, Common.bottomSpacing),
                        monthsField1.TopAnchor.ConstraintEqualTo(firstLine.TopAnchor, Common.topSpacing),

                        monthsLabel1.CenterYAnchor.ConstraintEqualTo(radioButton1.CenterYAnchor),
                        monthsLabel1.LeadingAnchor.ConstraintEqualTo(monthsField1.TrailingAnchor, Common.interviewHorizontalSpacing),
                    });

                var secondLine = new UIView
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = true,
                };
                secondLine.AddGestureRecognizer(new UITapGestureRecognizer(SecondLine_Tapped));

                radioButton2 = new RadioButton();

                var theLabel = new TextLabel
                {
                    Text = "The",
                };

                var ofEveryLabel2 = new TextLabel
                {
                    Text = "of every",
                };

                var monthsLabel2 = new TextLabel
                {
                    Text = "month(s)",
                };

                monthsField2 = new NumberField();
                monthsField2.EditingChanged += MonthsField2_EditingChanged;

                weekDayField = new ExtendedWeekDayPicker(UpdateWeekDays);
                weekOfMonthField = new WeekOfMonthPicker(UpdateWeekMonth);

                secondLine.AddSubview(radioButton2);
                secondLine.AddSubview(theLabel);
                secondLine.AddSubview(weekOfMonthField);
                secondLine.AddSubview(weekDayField);
                secondLine.AddSubview(ofEveryLabel2);
                secondLine.AddSubview(monthsField2);
                secondLine.AddSubview(monthsLabel2);

                secondLine.AddConstraints(new[]
                {
                        radioButton2.LeadingAnchor.ConstraintEqualTo(secondLine.LeadingAnchor),

                        theLabel.CenterYAnchor.ConstraintEqualTo(radioButton2.CenterYAnchor),
                        theLabel.LeadingAnchor.ConstraintEqualTo(radioButton2.TrailingAnchor, Common.radioButtonSpacing),

                        weekOfMonthField.CenterYAnchor.ConstraintEqualTo(radioButton2.CenterYAnchor),
                        weekOfMonthField.LeadingAnchor.ConstraintEqualTo(theLabel.TrailingAnchor, Common.interviewHorizontalSpacing),
                        weekOfMonthField.TopAnchor.ConstraintEqualTo(secondLine.TopAnchor, Common.topSpacing),

                        weekDayField.CenterYAnchor.ConstraintEqualTo(radioButton2.CenterYAnchor),
                        weekDayField.LeadingAnchor.ConstraintEqualTo(weekOfMonthField.TrailingAnchor, Common.interviewHorizontalSpacing),
                        weekDayField.TopAnchor.ConstraintEqualTo(secondLine.TopAnchor, Common.topSpacing),

                        ofEveryLabel2.LeadingAnchor.ConstraintEqualTo(theLabel.LeadingAnchor),
                        ofEveryLabel2.CenterYAnchor.ConstraintEqualTo(monthsField2.CenterYAnchor),

                        monthsField2.LeadingAnchor.ConstraintEqualTo(ofEveryLabel2.TrailingAnchor, Common.interviewHorizontalSpacing),
                        monthsField2.BottomAnchor.ConstraintEqualTo(secondLine.BottomAnchor, - Common.bottomSpacing),
                        monthsField2.TopAnchor.ConstraintEqualTo(weekDayField.BottomAnchor, Common.interviewVerticalSpacing),

                        monthsLabel2.CenterYAnchor.ConstraintEqualTo(monthsField2.CenterYAnchor),
                        monthsLabel2.LeadingAnchor.ConstraintEqualTo(monthsField2.TrailingAnchor, Common.interviewHorizontalSpacing),
                    });

                AddArrangedSubview(firstLine);
                AddArrangedSubview(secondLine);
            }

            private void FirstLine_Tapped()
            {
                radioButton1.Enabled = true;
                radioButton2.Enabled = false;
                UpdateModel();
            }

            private void SecondLine_Tapped()
            {
                radioButton1.Enabled = false;
                radioButton2.Enabled = true;
                UpdateModel();
            }

            private void MonthsField1_EditingChanged(object sender, EventArgs e)
            {
                FirstLine_Tapped();
            }

            void DaysTextField_EditingChanged(object sender, EventArgs e)
            {
                var tryParse = int.TryParse(dayTextField.Text, out var par);
                if (tryParse && par > 31)
                    dayTextField.Text = "31";

                FirstLine_Tapped();
            }

            private void MonthsField2_EditingChanged(object sender, EventArgs e)
            {
                SecondLine_Tapped();
            }

            void UpdateWeekMonth(WeekOfMonth wm)
            {
                viewModel.RecurrenceInfo.WeekOfMonth = wm;
                SecondLine_Tapped();
            }

            void UpdateWeekDays(WeekDays wd)
            {
                viewModel.RecurrenceInfo.WeekDays = wd;
                SecondLine_Tapped();
            }

            void UpdateModel()
            {
                if (radioButton1.Enabled)
                {
                    viewModel.RecurrenceInfo.WeekOfMonth = WeekOfMonth.None;
                    viewModel.RecurrenceInfo.Periodicity = int.TryParse(monthsField1.Text, out var p) ? p : 1;
                    viewModel.RecurrenceInfo.DayNumber = int.TryParse(dayTextField.Text, out var s) ? s : 1;
                }
                else if (radioButton2.Enabled)
                {
                    viewModel.RecurrenceInfo.Periodicity = int.TryParse(monthsField2.Text, out var s) ? s : 1;
                }
            }

            public void Refresh()
            {
                if (viewModel.RecurrenceInfo.Type != RecurrenceType.Monthly)
                {
                    if (!Hidden)
                        Hidden = true;
                    return;
                }

                if (Hidden)
                    Hidden = false;

                if (viewModel.RecurrenceInfo.WeekOfMonth == WeekOfMonth.None)
                {
                    radioButton1.Enabled = true;
                    radioButton2.Enabled = false;
                    dayTextField.Text = viewModel.RecurrenceInfo.DayNumber.ToString();
                    monthsField1.Text = viewModel.RecurrenceInfo.Periodicity.ToString();
                }
                else
                {
                    radioButton1.Enabled = false;
                    radioButton2.Enabled = true;

                    weekDayField.SetSelected(viewModel.RecurrenceInfo.WeekDays);
                    weekOfMonthField.SetSelected(viewModel.RecurrenceInfo.WeekOfMonth);

                    monthsField2.Text = viewModel.RecurrenceInfo.Periodicity.ToString();
                }
            }

            public void SetViewModel(AddEditAppointmentViewModel ca)
            {
                viewModel = ca;
            }
        }

        class YearlyView : UIStackView, IEditable
        {
            AddEditAppointmentViewModel viewModel;

            RadioButton radioButton1;
            RadioButton radioButton2;
            UITextField dayTextField;

            ExtendedWeekDayPicker weekDayField;
            WeekOfMonthPicker weekOfMonthField;
            MonthPicker monthField1;
            MonthPicker monthField2;

            public YearlyView()
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

                radioButton1 = new RadioButton();

                var everyLabel = new TextLabel { Text = "Every " };

                dayTextField = new NumberField();
                dayTextField.EditingChanged += DaysTextField_EditingChanged;

                monthField1 = new MonthPicker(UpdateMonth);

                firstLine.AddGestureRecognizer(new UITapGestureRecognizer(FirstLine_Tapped));
                firstLine.AddSubview(radioButton1);
                firstLine.AddSubview(everyLabel);
                firstLine.AddSubview(monthField1);
                firstLine.AddSubview(dayTextField);

                firstLine.AddConstraints(new[]
                {
                        radioButton1.LeadingAnchor.ConstraintEqualTo(firstLine.LeadingAnchor),
                        radioButton1.CenterYAnchor.ConstraintEqualTo(firstLine.CenterYAnchor),

                        everyLabel.CenterYAnchor.ConstraintEqualTo(radioButton1.CenterYAnchor),
                        everyLabel.LeadingAnchor.ConstraintEqualTo(radioButton1.TrailingAnchor, Common.radioButtonSpacing),

                        monthField1.CenterYAnchor.ConstraintEqualTo(radioButton1.CenterYAnchor),
                        monthField1.LeadingAnchor.ConstraintEqualTo(everyLabel.TrailingAnchor, Common.interviewHorizontalSpacing),
                        monthField1.BottomAnchor.ConstraintEqualTo(firstLine.BottomAnchor, Common.bottomSpacing),
                        monthField1.TopAnchor.ConstraintEqualTo(firstLine.TopAnchor, Common.topSpacing),

                        dayTextField.CenterYAnchor.ConstraintEqualTo(radioButton1.CenterYAnchor),
                        dayTextField.LeadingAnchor.ConstraintEqualTo(monthField1.TrailingAnchor, Common.interviewHorizontalSpacing),
                        dayTextField.BottomAnchor.ConstraintEqualTo(firstLine.BottomAnchor, Common.bottomSpacing),
                        dayTextField.TopAnchor.ConstraintEqualTo(firstLine.TopAnchor, Common.topSpacing),
                    });

                var secondLine = new UIView
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = true,
                };
                secondLine.AddGestureRecognizer(new UITapGestureRecognizer(SecondLine_Tapped));

                radioButton2 = new RadioButton();

                var theLabel = new TextLabel { Text = "The" };
                var ofLabel = new TextLabel { Text = "of" };

                weekDayField = new ExtendedWeekDayPicker(UpdateWeekDays);
                weekOfMonthField = new WeekOfMonthPicker(UpdateWeekOfMonth);
                monthField2 = new MonthPicker(UpdateMonth);

                secondLine.AddSubview(radioButton2);
                secondLine.AddSubview(theLabel);
                secondLine.AddSubview(weekOfMonthField);
                secondLine.AddSubview(weekDayField);
                secondLine.AddSubview(ofLabel);
                secondLine.AddSubview(monthField2);

                secondLine.AddConstraints(new[]
                {
                        radioButton2.LeadingAnchor.ConstraintEqualTo(secondLine.LeadingAnchor),

                        theLabel.CenterYAnchor.ConstraintEqualTo(radioButton2.CenterYAnchor),
                        theLabel.LeadingAnchor.ConstraintEqualTo(radioButton2.TrailingAnchor, Common.radioButtonSpacing),

                        weekOfMonthField.CenterYAnchor.ConstraintEqualTo(radioButton2.CenterYAnchor),
                        weekOfMonthField.LeadingAnchor.ConstraintEqualTo(theLabel.TrailingAnchor, Common.interviewHorizontalSpacing),
                        weekOfMonthField.TopAnchor.ConstraintEqualTo(secondLine.TopAnchor, Common.topSpacing),

                        weekDayField.CenterYAnchor.ConstraintEqualTo(radioButton2.CenterYAnchor),
                        weekDayField.LeadingAnchor.ConstraintEqualTo(weekOfMonthField.TrailingAnchor, Common.interviewHorizontalSpacing),
                        weekDayField.TopAnchor.ConstraintEqualTo(secondLine.TopAnchor, Common.topSpacing),

                        ofLabel.LeadingAnchor.ConstraintEqualTo(theLabel.LeadingAnchor),
                        ofLabel.CenterYAnchor.ConstraintEqualTo(monthField2.CenterYAnchor),

                        monthField2.LeadingAnchor.ConstraintEqualTo(ofLabel.TrailingAnchor, Common.interviewHorizontalSpacing),
                        monthField2.BottomAnchor.ConstraintEqualTo(secondLine.BottomAnchor, - Common.bottomSpacing),
                        monthField2.TopAnchor.ConstraintEqualTo(weekDayField.BottomAnchor, Common.interviewVerticalSpacing),
                    });

                AddArrangedSubview(firstLine);
                AddArrangedSubview(secondLine);
            }

            private void FirstLine_Tapped()
            {
                radioButton1.Enabled = true;
                radioButton2.Enabled = false;
                UpdateModel();
            }

            private void SecondLine_Tapped()
            {
                radioButton1.Enabled = false;
                radioButton2.Enabled = true;
                UpdateModel();
            }

            private void MonthsField1_EditingChanged(object sender, EventArgs e)
            {
                FirstLine_Tapped();
            }

            void DaysTextField_EditingChanged(object sender, EventArgs e)
            {
                var tryParse = int.TryParse(dayTextField.Text, out var par);
                if (tryParse && par > 31)
                    dayTextField.Text = "31";

                FirstLine_Tapped();
            }

            private void MonthsField2_EditingChanged(object sender, EventArgs e)
            {
                SecondLine_Tapped();
            }

            private void UpdateWeekOfMonth(WeekOfMonth wm)
            {
                viewModel.RecurrenceInfo.WeekOfMonth = wm;
                SecondLine_Tapped();
            }

            void UpdateMonth(int i)
            {
                viewModel.RecurrenceInfo.Month = i;
                SecondLine_Tapped();
            }

            void UpdateWeekDays(WeekDays wd)
            {
                viewModel.RecurrenceInfo.WeekDays = wd;
                SecondLine_Tapped();
            }

            void UpdateModel()
            {
                if (radioButton1.Enabled)
                {
                    viewModel.RecurrenceInfo.WeekOfMonth = WeekOfMonth.None;
                    viewModel.RecurrenceInfo.DayNumber = int.TryParse(dayTextField.Text, out var s) ? s : 1;
                }
            }

            public void Refresh()
            {
                if (viewModel.RecurrenceInfo.Type != RecurrenceType.Yearly)
                {
                    if (!Hidden)
                        Hidden = true;
                    return;
                }

                if (Hidden)
                    Hidden = false;

                if (viewModel.RecurrenceInfo.WeekOfMonth == WeekOfMonth.None)
                {
                    radioButton1.Enabled = true;
                    radioButton2.Enabled = false;
                    dayTextField.Text = viewModel.RecurrenceInfo.DayNumber.ToString();
                    monthField1.SetSelected(viewModel.RecurrenceInfo.Month);
                }
                else
                {
                    radioButton1.Enabled = false;
                    radioButton2.Enabled = true;

                    weekDayField.SetSelected(viewModel.RecurrenceInfo.WeekDays);
                    weekOfMonthField.SetSelected(viewModel.RecurrenceInfo.WeekOfMonth);
                    monthField2.SetSelected(viewModel.RecurrenceInfo.Month);
                }
            }

            public void SetViewModel(AddEditAppointmentViewModel ca)
            {
                viewModel = ca;
            }
        }
    }

}
