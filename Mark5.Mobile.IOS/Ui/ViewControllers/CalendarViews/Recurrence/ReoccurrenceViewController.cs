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
            ap.RecurrenceInfo.Type = RecurrenceType.Yearly;

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
                    patternView.LeftAnchor.ConstraintEqualTo(scrollView.ReadableContentGuide.LeftAnchor, paddingValue),
                    patternView.TopAnchor.ConstraintEqualTo(scrollView.ReadableContentGuide.TopAnchor, paddingValue),

                    //rangeView.TopAnchor.ConstraintEqualTo(patternView.BottomAnchor, 10f),
                    //rangeView.LeftAnchor.ConstraintEqualTo(patternView.LeftAnchor),
                    //rangeView.RightAnchor.ConstraintEqualTo(patternView.RightAnchor),
                    //rangeView.BottomAnchor.ConstraintEqualTo(scrollView.ReadableContentGuide.BottomAnchor),
            });

            var gestureRecognizer = new UITapGestureRecognizer(() => View.EndEditing(true));
            View.AddGestureRecognizer(gestureRecognizer);  //TODO testing
        }

        class PatternView : UIStackView, IEditable
        {
            PatternHeaderView patternHeaderView;
            DailyView dailyView;
            WeeklyView weeklyView;

            const float radioButtonSpacing = 5f;
            const float interViewSpacing = 5f;
            const float topSpacing = 2f;
            const float bottomSpacing = -2f;

            public PatternView()
            {
                InitializeView();
            }

            public void Refresh()
            {
                patternHeaderView.Refresh();
                dailyView.Refresh();
                weeklyView.Refresh();

            }

            public void SetViewModel(EditAppointmentViewModel ca)
            {
                patternHeaderView.SetViewModel(ca);
                dailyView.SetViewModel(ca);
                weeklyView.SetViewModel(ca);
            }

            void InitializeView()
            {
                Axis = UILayoutConstraintAxis.Vertical;
                Alignment = UIStackViewAlignment.Fill;
                Distribution = UIStackViewDistribution.Fill;
                Spacing = 10f;
                TranslatesAutoresizingMaskIntoConstraints = false;

                patternHeaderView = new PatternHeaderView();
                dailyView = new DailyView();
                weeklyView = new WeeklyView();

                AddArrangedSubview(patternHeaderView);
                AddArrangedSubview(new SeparatorSubView());
                AddArrangedSubview(weeklyView);
            }

            class PatternHeaderView : UIStackView, IEditable
            {
                EditAppointmentViewModel viewModel;
                PickerTextField typeField;

                List<RecurrenceType> recurrenceTypes = new List<RecurrenceType> { RecurrenceType.Daily, RecurrenceType.Monthly, RecurrenceType.Weekly, RecurrenceType.Yearly };
                List<string> recurrenceStrings = new List<string> { "Daily", "Monthly", "Weekly", "Yearly" };

                public PatternHeaderView()
                {
                    Axis = UILayoutConstraintAxis.Horizontal;
                    Alignment = UIStackViewAlignment.Fill;
                    Distribution = UIStackViewDistribution.Fill;
                    Spacing = 10f;
                    TranslatesAutoresizingMaskIntoConstraints = false;

                    var label = new UILabel
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        Font = Theme.DefaultFont,
                        TextColor = Theme.DarkGray,
                        Text = "Repeats",
                    };

                    typeField = new PickerTextField();
                    typeField.InputView = new UIPickerView
                    {
                        Model = new PickerViewModel(recurrenceStrings, UpdateModel)
                    };

                    AddArrangedSubview(label);
                    AddArrangedSubview(typeField);
                }

                public void UpdateModel(int i)
                {
                    viewModel.RecurrenceInfo.Type = recurrenceTypes[i];
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
                        daysTextField.LeadingAnchor.ConstraintEqualTo(everyLabel.TrailingAnchor, interViewSpacing),
                        daysTextField.BottomAnchor.ConstraintEqualTo(firstLine.BottomAnchor, bottomSpacing),
                        daysTextField.TopAnchor.ConstraintEqualTo(firstLine.TopAnchor, topSpacing),

                        daysLabel.CenterYAnchor.ConstraintEqualTo(radioButton1.CenterYAnchor),
                        daysLabel.LeadingAnchor.ConstraintEqualTo(daysTextField.TrailingAnchor, interViewSpacing),
                        daysLabel.TrailingAnchor.ConstraintEqualTo(firstLine.TrailingAnchor),
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
                        viewModel.RecurrenceInfo.Periodicity = int.Parse(daysTextField.Text);
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

                        weeksTextField.LeadingAnchor.ConstraintEqualTo(recurLabel.TrailingAnchor, interViewSpacing),
                        weeksTextField.BottomAnchor.ConstraintEqualTo(firstLine.BottomAnchor, bottomSpacing),
                        weeksTextField.TopAnchor.ConstraintEqualTo(firstLine.TopAnchor, topSpacing),
                        weeksTextField.CenterYAnchor.ConstraintEqualTo(recurLabel.CenterYAnchor),

                        weeksLabel.CenterYAnchor.ConstraintEqualTo(recurLabel.CenterYAnchor),
                        weeksLabel.LeadingAnchor.ConstraintEqualTo(weeksTextField.TrailingAnchor, interViewSpacing),
                        weeksLabel.TrailingAnchor.ConstraintEqualTo(firstLine.TrailingAnchor),
                    });

                    weekdaysTableView = new UITableView
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        UserInteractionEnabled = true,
                    };
                    weekdaysTableView.AllowsSelection = true;
                    weekdaysTableView.AllowsMultipleSelection = true;
                    weekdaysTableView.Source = new WeekdaysSource(this);

                    AddSubview(firstLine);
                    AddSubview(weekdaysTableView);

                    AddConstraints(new[]
                    {
                        firstLine.LeadingAnchor.ConstraintEqualTo(LeadingAnchor),
                        firstLine.TopAnchor.ConstraintEqualTo(TopAnchor),

                        weekdaysTableView.LeadingAnchor.ConstraintEqualTo(firstLine.LeadingAnchor),
                        weekdaysTableView.TopAnchor.ConstraintEqualTo(firstLine.BottomAnchor,10f ),
                        weekdaysTableView.BottomAnchor.ConstraintEqualTo(BottomAnchor),
                        weekdaysTableView.TrailingAnchor.ConstraintEqualTo(TrailingAnchor),
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
                    weeksTextField.Text = viewModel.RecurrenceInfo.Periodicity.ToString();
                    var selected = weekDays.Where(w => viewModel.RecurrenceInfo.WeekDays.HasFlag(w)).ToList();
                    (weekdaysTableView.Source as WeekdaysSource).SetSelected(selected);
                    weekdaysTableView.ReloadData();
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
                    readonly HashSet<WeekDays> selectedItems;

                    public WeekdaysSource(WeeklyView parentView)
                    {
                        this.parentView = parentView;
                    }

                    public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
                    {
                        var weekDay = data[indexPath.Row];
                        var cell = tableView.DequeueReusableCell("cell") ?? UITableViewCellUtilities.CreateDefault("cell", UITableViewCellSelectionStyle.None);
                        cell.TextLabel.Text = weekDay.GetDayName();
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

                    public void SetSelected(IEnumerable<WeekDays> wdays)
                    {
                        foreach (var w in wdays)
                            selectedItems.Add(w);
                    }
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
