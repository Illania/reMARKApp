using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews.Subviews
{
    public class RecurrenceViewController : AbstractViewController
    {
        PatternView patternView;
        RangeView rangeView;

        EditAppointmentViewModel ap = new EditAppointmentViewModel();

        public override void LoadView()
        {
            base.LoadView();

            InitializeView();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            patternView.SetViewModel(ap);
            patternView.Refresh();
        }

        void InitializeView()
        {
            ap.RecurrenceInfo = new RecurrenceInfo();
            ap.RecurrenceInfo.Type = RecurrenceType.Weekly;
            ap.RecurrenceInfo.WeekDays = WeekDays.WorkDays;

            NavigationItem.Title = "Custom recurrence"; //TODO remove support for iOS 10
            View.BackgroundColor = UIColor.GroupTableViewBackgroundColor;

            UIScrollView scrollView = new UIScrollView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = UIColor.White,
                ShowsVerticalScrollIndicator = false,
                ShowsHorizontalScrollIndicator = false,
            };

            View.AddSubview(scrollView);

            View.AddConstraints(new[]
            {
                scrollView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                scrollView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                scrollView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                scrollView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });

            patternView = new PatternView();
            patternView.BackgroundColor = UIColor.Blue;
            rangeView = new RangeView();

            scrollView.AddSubview(patternView);
            //scrollView.AddSubview(rangeView);

            var paddingValue = 20f;

            scrollView.AddConstraints(new[]
            {
                    patternView.LeadingAnchor.ConstraintEqualTo(scrollView.ReadableContentGuide.LeadingAnchor, paddingValue),
                    patternView.TopAnchor.ConstraintEqualTo(scrollView.ReadableContentGuide.TopAnchor, paddingValue),
                    patternView.RightAnchor.ConstraintEqualTo(scrollView.ReadableContentGuide.RightAnchor, -paddingValue),
                    //rangeView.TopAnchor.ConstraintEqualTo(patternView.BottomAnchor, 10f),
                    //rangeView.LeftAnchor.ConstraintEqualTo(patternView.LeftAnchor),
                    //rangeView.RightAnchor.ConstraintEqualTo(patternView.RightAnchor),
                    //rangeView.BottomAnchor.ConstraintEqualTo(scrollView.ReadableContentGuide.BottomAnchor),
            });

            var gestureRecognizer = new UITapGestureRecognizer(() => View.EndEditing(true));
            //View.AddGestureRecognizer(gestureRecognizer);  //TODO testing
        }

        class PatternView : UIStackView, IEditable
        {
            PatternHeaderView patternHeaderView;
            DailyView dailyView;
            WeeklyView weeklyView;
            MonthlyView monthlyView;

            const float radioButtonSpacing = 5f;
            const float interviewHorizontalSpacing = 5f;
            const float interviewVerticalSpacing = 3f;
            const float topSpacing = 4f;
            const float bottomSpacing = -2f;

            const float animationDuration = 0f;

            public PatternView()
            {
                InitializeView();
            }

            public void Refresh()
            {
                Subviews.OfType<IEditable>().ToList().ForEach(a => a.Refresh());
            }

            public void SetViewModel(EditAppointmentViewModel ca)
            {
                Subviews.OfType<IEditable>().ToList().ForEach(a => a.SetViewModel(ca));
            }

            void InitializeView()
            {
                Axis = UILayoutConstraintAxis.Vertical;
                Alignment = UIStackViewAlignment.Fill;
                Distribution = UIStackViewDistribution.Fill;
                Spacing = 10f;
                TranslatesAutoresizingMaskIntoConstraints = false;

                patternHeaderView = new PatternHeaderView();
                patternHeaderView.Updated += PatternHeaderView_Updated;
                dailyView = new DailyView();
                weeklyView = new WeeklyView();
                monthlyView = new MonthlyView();

                AddArrangedSubview(patternHeaderView);
                AddArrangedSubview(new SeparatorSubView());
                AddArrangedSubview(dailyView);
                AddArrangedSubview(weeklyView);
                AddArrangedSubview(monthlyView);
            }

            private void PatternHeaderView_Updated(object sender, EventArgs e)
            {
                dailyView.Refresh();
                weeklyView.Refresh();
                monthlyView.Refresh();
            }

            class PatternHeaderView : UIView, IEditable
            {
                EditAppointmentViewModel viewModel;
                PickerTextField typeField;

                List<RecurrenceType> recurrenceTypes = new List<RecurrenceType> { RecurrenceType.Daily, RecurrenceType.Weekly, RecurrenceType.Monthly, RecurrenceType.Yearly };

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

                    var recurrenceStrings = recurrenceTypes.Select(r => r.ToFriendlyString()).ToList();
                    typeField = new PickerTextField();
                    typeField.InputView = new UIPickerView
                    {
                        Model = new PickerViewModel(recurrenceStrings, UpdateModel)
                    };

                    AddSubview(label);
                    AddSubview(typeField);

                    AddConstraints(new[]
                    {
                        label.LeadingAnchor.ConstraintEqualTo(LeadingAnchor),
                        label.TopAnchor.ConstraintEqualTo(TopAnchor),
                        label.CenterYAnchor.ConstraintEqualTo(CenterYAnchor),
                        label.BottomAnchor.ConstraintEqualTo(BottomAnchor),

                        typeField.LeadingAnchor.ConstraintEqualTo(label.TrailingAnchor, 10f),
                        typeField.TopAnchor.ConstraintEqualTo(TopAnchor),
                        typeField.CenterYAnchor.ConstraintEqualTo(CenterYAnchor),
                    });
                }

                public void UpdateModel(int i)
                {
                    viewModel.RecurrenceInfo.Type = recurrenceTypes[i];
                    Updated(this, EventArgs.Empty);
                    Refresh();
                }

                public void Refresh()
                {
                    typeField.Text = RecurrenceToString(viewModel.RecurrenceInfo.Type);
                    var index = recurrenceTypes.FindIndex(i => i == viewModel.RecurrenceInfo.Type);
                    (typeField.InputView as UIPickerView).Select(index, 0, false);
                }

                public void SetViewModel(EditAppointmentViewModel ca)
                {
                    viewModel = ca;
                }

                string RecurrenceToString(RecurrenceType t)
                {
                    switch (t)
                    {
                        case RecurrenceType.Daily:
                            return "Daily";
                        case RecurrenceType.Weekly:
                            return "Weekly";
                        case RecurrenceType.Monthly:
                            return "Monthly";
                        case RecurrenceType.Yearly:
                            return "Yearly";
                        default:
                            throw new Exception("ERRROR!");
                    }
                }
            }

            class DailyView : UIStackView, IEditable
            {
                EditAppointmentViewModel viewModel;

                RadioButton radioButton1;
                RadioButton radioButton2;
                UITextField daysTextField;

                public DailyView()
                {
                    Axis = UILayoutConstraintAxis.Vertical;
                    Alignment = UIStackViewAlignment.Fill;
                    Distribution = UIStackViewDistribution.Fill;
                    Spacing = 20f;
                    TranslatesAutoresizingMaskIntoConstraints = false;

                    var firstLine = new UIView
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        UserInteractionEnabled = true,
                    };

                    var everyLabel = new UILabel
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        Font = Theme.DefaultFont,
                        Text = "Every",
                    };

                    radioButton1 = new RadioButton();

                    var daysLabel = new UILabel
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        Font = Theme.DefaultFont,
                        Text = "days",
                    };

                    daysTextField = new PickerTextField(true)
                    {
                        Text = "1",
                        KeyboardType = UIKeyboardType.NumberPad
                    };
                    daysTextField.EditingChanged += DaysTextField_EditingChanged;
                    daysTextField.EditingDidBegin += DaysTextField_EditingDidBegin;
                    daysTextField.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
                    daysTextField.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);

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
                        everyLabel.LeadingAnchor.ConstraintEqualTo(radioButton1.TrailingAnchor, radioButtonSpacing),

                        daysTextField.CenterYAnchor.ConstraintEqualTo(radioButton1.CenterYAnchor),
                        daysTextField.LeadingAnchor.ConstraintEqualTo(everyLabel.TrailingAnchor, interviewHorizontalSpacing),
                        daysTextField.BottomAnchor.ConstraintEqualTo(firstLine.BottomAnchor, bottomSpacing),
                        daysTextField.TopAnchor.ConstraintEqualTo(firstLine.TopAnchor, topSpacing),

                        daysLabel.CenterYAnchor.ConstraintEqualTo(radioButton1.CenterYAnchor),
                        daysLabel.LeadingAnchor.ConstraintEqualTo(daysTextField.TrailingAnchor, interviewHorizontalSpacing),
                        //daysLabel.TrailingAnchor.ConstraintEqualTo(firstLine.TrailingAnchor),
                    });

                    var secondLine = new UIView
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        UserInteractionEnabled = true,
                    };
                    secondLine.AddGestureRecognizer(new UITapGestureRecognizer(SecondLine_Tapped));

                    radioButton2 = new RadioButton();

                    var weekedaysLabel = new UILabel
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        Font = Theme.DefaultFont,
                        Text = "Every weekday",
                    };

                    secondLine.AddSubview(radioButton2);
                    secondLine.AddSubview(weekedaysLabel);

                    secondLine.AddConstraints(new[]
                    {
                        radioButton2.LeadingAnchor.ConstraintEqualTo(secondLine.LeadingAnchor),
                        radioButton2.CenterYAnchor.ConstraintEqualTo(secondLine.CenterYAnchor),
                        radioButton2.TopAnchor.ConstraintEqualTo(secondLine.TopAnchor,topSpacing),

                        weekedaysLabel.CenterYAnchor.ConstraintEqualTo(radioButton2.CenterYAnchor),
                        weekedaysLabel.LeadingAnchor.ConstraintEqualTo(radioButton2.TrailingAnchor, radioButtonSpacing),
                        weekedaysLabel.TrailingAnchor.ConstraintEqualTo(secondLine.TrailingAnchor),
                        weekedaysLabel.BottomAnchor.ConstraintEqualTo(secondLine.BottomAnchor, bottomSpacing),
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

                void DaysTextField_EditingDidBegin(object sender, EventArgs e)
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

                public void SetViewModel(EditAppointmentViewModel ca)
                {
                    viewModel = ca;
                }
            }

            class WeeklyView : UIView, IEditable
            {
                EditAppointmentViewModel viewModel;

                const float cellheight = 44f;

                UITextField weeksTextField;
                UITableView weekdaysTableView;
                List<WeekDays> weekDays = new List<WeekDays> { WeekDays.Monday, WeekDays.Tuesday, WeekDays.Wednesday,
                    WeekDays.Thursday, WeekDays.Friday, WeekDays.Saturday, WeekDays.Sunday };

                public WeeklyView()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false;

                    var firstLine = new UIView
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        UserInteractionEnabled = true,
                    };

                    var recurLabel = new UILabel
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        Font = Theme.DefaultFont,
                        Text = "Recur every",
                    };

                    var weeksLabel = new UILabel
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        Font = Theme.DefaultFont,
                        Text = "week(s) on:",
                    };

                    weeksTextField = new PickerTextField(true)
                    {
                        Text = "1",
                        KeyboardType = UIKeyboardType.NumberPad
                    };
                    weeksTextField.EditingChanged += WeeksTextField_EditingChanged;
                    weeksTextField.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
                    weeksTextField.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);

                    firstLine.AddSubview(recurLabel);
                    firstLine.AddSubview(weeksLabel);
                    firstLine.AddSubview(weeksTextField);

                    firstLine.AddConstraints(new[]
                    {
                        recurLabel.LeadingAnchor.ConstraintEqualTo(firstLine.LeadingAnchor),
                        recurLabel.CenterYAnchor.ConstraintEqualTo(firstLine.CenterYAnchor),

                        weeksTextField.LeadingAnchor.ConstraintEqualTo(recurLabel.TrailingAnchor, interviewHorizontalSpacing),
                        weeksTextField.BottomAnchor.ConstraintEqualTo(firstLine.BottomAnchor, bottomSpacing),
                        weeksTextField.TopAnchor.ConstraintEqualTo(firstLine.TopAnchor, topSpacing),
                        weeksTextField.CenterYAnchor.ConstraintEqualTo(recurLabel.CenterYAnchor),

                        weeksLabel.CenterYAnchor.ConstraintEqualTo(recurLabel.CenterYAnchor),
                        weeksLabel.LeadingAnchor.ConstraintEqualTo(weeksTextField.TrailingAnchor, interviewHorizontalSpacing),
                    });

                    weekdaysTableView = new UITableView
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        AllowsSelection = true,
                        AllowsMultipleSelection = true,
                        RowHeight = cellheight,
                        Source = new WeekdaysSource(this),
                        ScrollEnabled = false,
                        CellLayoutMarginsFollowReadableWidth = false,
                        SeparatorInset = UIEdgeInsets.Zero,
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
                    var selected = weekDays.Where(w => viewModel.RecurrenceInfo.WeekDays.HasFlag(w)).ToList();
                    weekdaysTableView.ReloadData();
                    (weekdaysTableView.Source as WeekdaysSource).SetSelected(weekdaysTableView, selected);
                }

                public void SetViewModel(EditAppointmentViewModel ca)
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
                    List<WeekDays> data = new List<WeekDays> { WeekDays.Monday, WeekDays.Tuesday, WeekDays.Wednesday,
                    WeekDays.Thursday, WeekDays.Friday, WeekDays.Saturday, WeekDays.Sunday };
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
                EditAppointmentViewModel viewModel;

                List<WeekDays> weekDays = new List<WeekDays>
                { WeekDays.EveryDay, WeekDays.WorkDays, WeekDays.WeekendDays, WeekDays.Monday, WeekDays.Tuesday,WeekDays.Wednesday,
                    WeekDays.Thursday, WeekDays.Friday, WeekDays.Saturday, WeekDays.Sunday };

                List<WeekOfMonth> weekOfMonth = new List<WeekOfMonth> { WeekOfMonth.First, WeekOfMonth.Second, WeekOfMonth.Third, WeekOfMonth.Fourth, WeekOfMonth.Last };

                RadioButton radioButton1;
                RadioButton radioButton2;
                UITextField dayTextField;
                UITextField monthsField1;
                UITextField monthsField2;
                UITextField weekDayField;
                UITextField weekOfMonthField;

                public MonthlyView()
                {
                    Axis = UILayoutConstraintAxis.Vertical;
                    Alignment = UIStackViewAlignment.Fill;
                    Distribution = UIStackViewDistribution.Fill;
                    Spacing = 20f;
                    TranslatesAutoresizingMaskIntoConstraints = false;

                    var firstLine = new UIView
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        UserInteractionEnabled = true,
                    };

                    radioButton1 = new RadioButton();

                    var dayLabel = new UILabel
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        Font = Theme.DefaultFont,
                        Text = "Day",
                    };

                    var ofEveryLabel1 = new UILabel
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        Font = Theme.DefaultFont,
                        Text = "of every",
                    };

                    var monthsLabel1 = new UILabel
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        Font = Theme.DefaultFont,
                        Text = "month(s)",
                    };

                    dayTextField = new PickerTextField(true)
                    {
                        Text = "1",
                        KeyboardType = UIKeyboardType.NumberPad
                    };
                    dayTextField.EditingChanged += DaysTextField_EditingChanged;
                    dayTextField.EditingDidBegin += DaysTextField_EditingDidBegin;
                    dayTextField.EditingDidEnd += DayTextField_EditingDidEnd;
                    dayTextField.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
                    dayTextField.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);

                    monthsField1 = new PickerTextField(true)
                    {
                        Text = "1",
                        KeyboardType = UIKeyboardType.NumberPad
                    };
                    monthsField1.EditingChanged += MonthsField1_EditingChanged;
                    monthsField1.EditingDidBegin += MonthsField1_EditingDidBegin;
                    monthsField1.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
                    monthsField1.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);

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
                        dayLabel.LeadingAnchor.ConstraintEqualTo(radioButton1.TrailingAnchor, radioButtonSpacing),

                        dayTextField.CenterYAnchor.ConstraintEqualTo(radioButton1.CenterYAnchor),
                        dayTextField.LeadingAnchor.ConstraintEqualTo(dayLabel.TrailingAnchor, interviewHorizontalSpacing),
                        dayTextField.BottomAnchor.ConstraintEqualTo(firstLine.BottomAnchor, bottomSpacing),
                        dayTextField.TopAnchor.ConstraintEqualTo(firstLine.TopAnchor, topSpacing),

                        ofEveryLabel1.CenterYAnchor.ConstraintEqualTo(radioButton1.CenterYAnchor),
                        ofEveryLabel1.LeadingAnchor.ConstraintEqualTo(dayTextField.TrailingAnchor, interviewHorizontalSpacing),

                        monthsField1.CenterYAnchor.ConstraintEqualTo(radioButton1.CenterYAnchor),
                        monthsField1.LeadingAnchor.ConstraintEqualTo(ofEveryLabel1.TrailingAnchor, interviewHorizontalSpacing),
                        monthsField1.BottomAnchor.ConstraintEqualTo(firstLine.BottomAnchor, bottomSpacing),
                        monthsField1.TopAnchor.ConstraintEqualTo(firstLine.TopAnchor, topSpacing),

                        monthsLabel1.CenterYAnchor.ConstraintEqualTo(radioButton1.CenterYAnchor),
                        monthsLabel1.LeadingAnchor.ConstraintEqualTo(monthsField1.TrailingAnchor, interviewHorizontalSpacing),
                    });

                    var secondLine = new UIView
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        UserInteractionEnabled = true,
                    };
                    secondLine.AddGestureRecognizer(new UITapGestureRecognizer(SecondLine_Tapped));

                    radioButton2 = new RadioButton();

                    var theLabel = new UILabel
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        Font = Theme.DefaultFont,
                        Text = "The",
                    };

                    var ofEveryLabel2 = new UILabel
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        Font = Theme.DefaultFont,
                        Text = "of every",
                    };

                    var monthsLabel2 = new UILabel
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        Font = Theme.DefaultFont,
                        Text = "month(s)",
                    };

                    monthsField2 = new PickerTextField(true)
                    {
                        Text = "1",
                        KeyboardType = UIKeyboardType.NumberPad
                    };
                    monthsField2.EditingChanged += MonthsField2_EditingChanged;
                    monthsField2.EditingDidBegin += MonthsField2_EditingDidBegin;
                    monthsField2.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
                    monthsField2.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);

                    var weekDaysStrings = weekDays.Select(w => w.ToFriendlyString()).ToList();

                    weekDayField = new PickerTextField();
                    weekDayField.InputView = new UIPickerView
                    {
                        Model = new PickerViewModel(weekDaysStrings, UpdateWeekDays)
                    };

                    var weekOfMonthsStrings = weekOfMonth.Select(w => w.ToFriendlyString()).ToList();

                    weekOfMonthField = new PickerTextField();
                    weekOfMonthField.InputView = new UIPickerView
                    {
                        Model = new PickerViewModel(weekOfMonthsStrings, UpdateWeekMonth)
                    };

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
                        theLabel.LeadingAnchor.ConstraintEqualTo(radioButton2.TrailingAnchor, radioButtonSpacing),

                        weekOfMonthField.CenterYAnchor.ConstraintEqualTo(radioButton2.CenterYAnchor),
                        weekOfMonthField.LeadingAnchor.ConstraintEqualTo(theLabel.TrailingAnchor, interviewHorizontalSpacing),
                        weekOfMonthField.TopAnchor.ConstraintEqualTo(secondLine.TopAnchor, topSpacing),

                        weekDayField.CenterYAnchor.ConstraintEqualTo(radioButton2.CenterYAnchor),
                        weekDayField.LeadingAnchor.ConstraintEqualTo(weekOfMonthField.TrailingAnchor, interviewHorizontalSpacing),
                        weekDayField.TopAnchor.ConstraintEqualTo(secondLine.TopAnchor, topSpacing),

                        ofEveryLabel2.LeadingAnchor.ConstraintEqualTo(theLabel.LeadingAnchor),
                        ofEveryLabel2.CenterYAnchor.ConstraintEqualTo(monthsField2.CenterYAnchor),

                        monthsField2.LeadingAnchor.ConstraintEqualTo(ofEveryLabel2.TrailingAnchor, interviewHorizontalSpacing),
                        monthsField2.BottomAnchor.ConstraintEqualTo(secondLine.BottomAnchor, - bottomSpacing),
                        monthsField2.TopAnchor.ConstraintEqualTo(weekDayField.BottomAnchor, interviewVerticalSpacing),

                        monthsLabel2.CenterYAnchor.ConstraintEqualTo(monthsField2.CenterYAnchor),
                        monthsLabel2.LeadingAnchor.ConstraintEqualTo(monthsField2.TrailingAnchor, interviewHorizontalSpacing),
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

                private void MonthsField1_EditingDidBegin(object sender, EventArgs e)
                {
                    FirstLine_Tapped();
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

                void DaysTextField_EditingDidBegin(object sender, EventArgs e)
                {
                    FirstLine_Tapped();
                }

                void DayTextField_EditingDidEnd(object sender, EventArgs e)
                {
                    var tryParse = int.TryParse(dayTextField.Text, out var par);
                    if (!tryParse)
                        dayTextField.Text = "1";  //TODO this works, neeed to do the same with the others

                    FirstLine_Tapped();
                }

                private void MonthsField2_EditingDidBegin(object sender, EventArgs e)
                {
                    SecondLine_Tapped();
                }

                private void MonthsField2_EditingChanged(object sender, EventArgs e)
                {
                    SecondLine_Tapped();
                }

                void UpdateWeekMonth(int i)
                {
                    viewModel.RecurrenceInfo.WeekOfMonth = weekOfMonth[i];
                    weekOfMonthField.Text = weekOfMonth[i].ToFriendlyString();
                    SecondLine_Tapped();
                }

                void UpdateWeekDays(int i)
                {
                    viewModel.RecurrenceInfo.WeekDays = weekDays[i];
                    weekDayField.Text = weekDays[i].ToFriendlyString();
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

                        weekDayField.Text = viewModel.RecurrenceInfo.WeekDays.ToFriendlyString();
                        var weekdayIndex = weekDays.FindIndex(a => a == viewModel.RecurrenceInfo.WeekDays);
                        (weekDayField.InputView as UIPickerView).Select(weekdayIndex, 0, false);

                        weekOfMonthField.Text = viewModel.RecurrenceInfo.WeekOfMonth.ToFriendlyString();
                        var weekOfMonthIndex = weekOfMonth.FindIndex(a => a == viewModel.RecurrenceInfo.WeekOfMonth);
                        (weekOfMonthField.InputView as UIPickerView).Select(weekOfMonthIndex, 0, false);

                        monthsField2.Text = viewModel.RecurrenceInfo.Periodicity.ToString();
                    }
                }

                public void SetViewModel(EditAppointmentViewModel ca)
                {
                    viewModel = ca;
                }
            }

        }

        class RangeView : UIStackView, IEditable
        {
            public void Refresh()
            {
                throw new NotImplementedException();
            }

            public void SetViewModel(EditAppointmentViewModel ca)
            {
                throw new NotImplementedException();
            }
        }

        interface IEditable
        {
            void Refresh();
            void SetViewModel(EditAppointmentViewModel vm);
        }

        class PickerTextField : UITextField
        {
            const float insetVal = 5f;

            public PickerTextField(bool editable = false)
            {
                Font = Theme.DefaultFont;
                TranslatesAutoresizingMaskIntoConstraints = false;
                Layer.BorderColor = Theme.Blue.CGColor;
                Layer.BorderWidth = 2f;
                Layer.CornerRadius = 8f;
                TextColor = Theme.Blue;
                if (!editable)
                    TintColor = Theme.Clear;

                AddConstraints(new[] {
                    WidthAnchor.ConstraintGreaterThanOrEqualTo(35f),
                    HeightAnchor.ConstraintGreaterThanOrEqualTo(30f),
                });
            }

            public override CGRect TextRect(CGRect forBounds)
            {
                return forBounds.Inset(insetVal, insetVal);
            }

            public override CGRect EditingRect(CGRect forBounds)
            {
                return forBounds.Inset(insetVal, insetVal);
            }
        }

        class PickerViewModel : UIPickerViewModel
        {
            List<string> elements;
            Action<int> selectedDelegate;

            public PickerViewModel(List<string> elements, Action<int> selectedDelegate)
            {
                this.elements = elements;
                this.selectedDelegate = selectedDelegate;
            }

            public override nint GetComponentCount(UIPickerView pickerView) => 1;

            public override nint GetRowsInComponent(UIPickerView pickerView, nint component) => elements.Count;

            public override string GetTitle(UIPickerView pickerView, nint row, nint component)
            {
                return elements[(int)row];
            }

            public override void Selected(UIPickerView pickerView, nint row, nint component)
            {
                selectedDelegate?.Invoke((int)row);
            }
        }

        public class SeparatorSubView : UIView
        {
            static readonly UIColor backgroundColor = Theme.Blue;

            public SeparatorSubView()
            {
                BackgroundColor = backgroundColor;
                HeightAnchor.ConstraintEqualTo(1.5f).Active = true;
                TranslatesAutoresizingMaskIntoConstraints = false;
            }
        }

        class RadioButton : UIView
        {
            readonly UIView innerDisk;

            private bool enabled;

            public bool Enabled
            {
                get => enabled;
                set { enabled = value; UpdateState(); }
            }

            public RadioButton()
            {
                TranslatesAutoresizingMaskIntoConstraints = false;
                Layer.CornerRadius = 5f;
                Layer.BorderColor = Theme.Blue.CGColor;
                Layer.BorderWidth = 2f;

                innerDisk = new UIView()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                };

                innerDisk.Layer.CornerRadius = 3f;
                innerDisk.BackgroundColor = Theme.Blue;

                AddSubview(innerDisk);

                AddConstraints(new[]
                    {
                    innerDisk.CenterXAnchor.ConstraintEqualTo(CenterXAnchor),
                    innerDisk.CenterYAnchor.ConstraintEqualTo(CenterYAnchor),
                    innerDisk.WidthAnchor.ConstraintEqualTo(innerDisk.HeightAnchor),
                    innerDisk.WidthAnchor.ConstraintEqualTo(6),
                    WidthAnchor.ConstraintEqualTo(HeightAnchor),
                    WidthAnchor.ConstraintEqualTo(15),
                    });

                innerDisk.Alpha = 0;
            }

            void UpdateState()
            {
                innerDisk.Alpha = enabled ? 1 : 0;
            }
        }


    }
}
