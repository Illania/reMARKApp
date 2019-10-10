using System;
using Android.App;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.RecurrenceViews
{
    public class PatternView : RecurrenceParentView
    {
        readonly PatternHeaderView patternHeaderView;
        readonly DailyView dailyView;
        readonly WeeklyView weeklyView;
        readonly MonthlyView monthlyView;
        readonly YearlyView yearlyView;

        public PatternView(Context context) : base(context)
        {
            Orientation = Vertical;
            patternHeaderView = new PatternHeaderView(context);
            patternHeaderView.Updated += PatternHeaderView_Updated;
            dailyView = new DailyView(context);
            weeklyView = new WeeklyView(context);
            monthlyView = new MonthlyView(context);
            yearlyView = new YearlyView(context);

            AddView(patternHeaderView);
            AddView(new SeparatorView(context));
            AddView(dailyView);
            AddView(weeklyView);
            AddView(monthlyView);
            AddView(yearlyView);
        }

        private void PatternHeaderView_Updated(object sender, EventArgs e)
        {
            ((Activity)Context).RunOnUiThread(() =>
           {
               dailyView.Refresh();
               weeklyView.Refresh();
               monthlyView.Refresh();
               yearlyView.Refresh();
           });
        }

        public override void Refresh()
        {
            for (int i = 0; i < ChildCount; i++)
            {
                if (GetChildAt(i) is IEditable editable)
                    editable.Refresh();
            }
        }

        public override void SetViewModel(RecurrenceInfo ri)
        {
            for (int i = 0; i < ChildCount; i++)
            {
                if (GetChildAt(i) is IEditable editable)
                    editable.SetViewModel(ri);
            }
        }

        class PatternHeaderView : RecurrenceSubView
        {
            readonly TypePicker typePicker;
            public event EventHandler Updated = delegate { };

            public PatternHeaderView(Context context) : base(context)
            {
                Orientation = Horizontal;
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                Gravity = (int)GravityFlags.CenterVertical;

                var repeatsLabel = new SpecialLabelTextView(Context) { Text = "Repeats" };

                typePicker = new TypePicker(Context, TypeSelected);

                AddView(repeatsLabel);
                AddView(typePicker);
            }

            public override void Refresh()
            {
                typePicker.SetSelected(ri.Type);
            }

            void TypeSelected(RecurrenceType rt)
            {
                ri.Type = rt;
                Updated(this, EventArgs.Empty);
                Refresh();
            }
        }

        class DailyView : RecurrenceSubView
        {
            AppCompatRadioButton radioButton1;
            AppCompatRadioButton radioButton2;
            NumberField daysTextField;

            public DailyView(Context context) : base(context)
            {
                Orientation = Vertical;
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

                var firstLine = new LinearLayoutCompat(Context)
                {
                    Orientation = Horizontal,
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                    {
                        BottomMargin = Common.verticalSpacing,
                    },
                };
                firstLine.Click += (a, b) => FirstLine_Click();

                radioButton1 = new AppCompatRadioButton(context);
                radioButton1.Click += (a, b) => FirstLine_Click();
                daysTextField = new NumberField(context);
                daysTextField.TextModified += DaysTextField_TextChanged;
                var everyLabel = new LabelTextView(context) { Text = "Every" };
                var daysLabel = new LabelTextView(context) { Text = "day(s)" };

                firstLine.AddView(radioButton1);
                firstLine.AddView(everyLabel);
                firstLine.AddView(daysTextField);
                firstLine.AddView(daysLabel);

                var secondLine = new LinearLayoutCompat(Context)
                {
                    Orientation = Horizontal,
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                };
                secondLine.Click += (a, b) => SecondLine_Click();

                radioButton2 = new AppCompatRadioButton(context);
                radioButton2.Click += (a, b) => SecondLine_Click();

                var everyWeekdayLabel = new LabelTextView(context) { Text = "Every weekday" };

                secondLine.AddView(radioButton2);
                secondLine.AddView(everyWeekdayLabel);

                AddView(firstLine);
                AddView(secondLine);
            }

            private void FirstLine_Click()
            {
                radioButton1.Checked = true;
                radioButton2.Checked = false;
                UpdateModel();
            }

            private void SecondLine_Click()
            {
                radioButton1.Checked = false;
                radioButton2.Checked = true;
                UpdateModel();
            }

            private void DaysTextField_TextChanged(object sender, string e)
            {
                FirstLine_Click();
            }

            void UpdateModel()
            {
                if (radioButton1.Checked)
                {
                    ri.WeekDays = WeekDays.EveryDay;
                    ri.Periodicity = int.TryParse(daysTextField.Text, out var s) ? s : 1;
                }
                else if (radioButton2.Checked)
                {
                    ri.WeekDays = WeekDays.WorkDays;
                }
            }

            public override void Refresh()
            {
                if (ri.Type != RecurrenceType.Daily)
                {
                    Visibility = ViewStates.Gone;
                    return;
                }

                Visibility = ViewStates.Visible;
                daysTextField.SetText(ri.Periodicity.ToString());

                if (ri.WeekDays == WeekDays.EveryDay)
                {
                    radioButton1.Checked = true;
                    radioButton2.Checked = false;
                }
                else if (ri.WeekDays == WeekDays.WorkDays)
                {
                    radioButton1.Checked = false;
                    radioButton2.Checked = true;
                }
            }
        }

        class WeeklyView : RecurrenceSubView
        {
            NumberField weeksTextField;
            WeekDaysSelectionView weekDaysSelectionView;

            public WeeklyView(Context context) : base(context)
            {
                Orientation = Vertical;
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

                var firstLine = new LinearLayoutCompat(Context)
                {
                    Orientation = Horizontal,
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                    {
                        BottomMargin = Common.verticalSpacing,
                    },
                };

                weeksTextField = new NumberField(context);
                weeksTextField.TextModified += WeeksTextField_TextChanged;
                var recurLabel = new LabelTextView(context) { Text = "Recur every" };
                var weeksLabel = new LabelTextView(context) { Text = "week(s) on:" };

                firstLine.AddView(recurLabel);
                firstLine.AddView(weeksTextField);
                firstLine.AddView(weeksLabel);

                weekDaysSelectionView = new WeekDaysSelectionView(context);
                weekDaysSelectionView.SelectionChanged += WeekDaysSelectionView_SelectionChanged;

                AddView(firstLine);
                AddView(weekDaysSelectionView);
            }

            private void WeekDaysSelectionView_SelectionChanged(object sender, (WeekDays wd, bool selected) e)
            {
                if (e.selected)
                    ri.WeekDays |= e.wd;
                else
                    ri.WeekDays &= ~e.wd;
            }

            private void WeeksTextField_TextChanged(object sender, string e)
            {
                ri.Periodicity = int.TryParse(weeksTextField.Text, out var i) ? i : 1;
            }

            public override void Refresh()
            {
                if (ri.Type != RecurrenceType.Weekly)
                {
                    Visibility = ViewStates.Gone;
                    return;
                }

                Visibility = ViewStates.Visible;
                weeksTextField.SetText(ri.Periodicity.ToString());
                weekDaysSelectionView.Refresh(ri.WeekDays);
            }
        }

        class MonthlyView : RecurrenceSubView
        {
            AppCompatRadioButton radioButton1;
            AppCompatRadioButton radioButton2;
            NumberField dayTextField;
            NumberField monthsField1;
            NumberField monthsField2;
            ExtendedWeekDayPicker weekDayField;
            WeekOfMonthPicker weekOfMonthField;

            public MonthlyView(Context context) : base(context)
            {
                Orientation = Vertical;
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

                var firstLine = new LinearLayoutCompat(Context)
                {
                    Orientation = Horizontal,
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                    {
                        BottomMargin = Common.verticalSpacing,
                    },
                    Gravity = (int)GravityFlags.CenterVertical,
                };
                firstLine.Click += (a, b) => FirstLine_Click();

                radioButton1 = new AppCompatRadioButton(context);
                radioButton1.Click += (a, b) => FirstLine_Click();

                dayTextField = new NumberField(context);
                dayTextField.TextModified += DaysTextField_TextChanged;
                var dayLabel = new LabelTextView(context) { Text = "Day" };
                var everyLabel = new LabelTextView(context) { Text = "of every" };
                var monthsLabel = new LabelTextView(context) { Text = "month(s)" };
                monthsField1 = new NumberField(context);
                monthsField1.TextModified += MonthsField1_TextChanged;

                firstLine.AddView(radioButton1);
                firstLine.AddView(dayLabel);
                firstLine.AddView(dayTextField);
                firstLine.AddView(everyLabel);
                firstLine.AddView(monthsField1);
                firstLine.AddView(monthsLabel);

                var secondLine = new LinearLayoutCompat(Context)
                {
                    Orientation = Horizontal,
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                };
                secondLine.Click += (a, b) => SecondLine_Click();

                var container = new LinearLayoutCompat(Context)
                {
                    Orientation = Vertical,
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                };

                var topLine = new LinearLayoutCompat(Context)
                {
                    Orientation = Horizontal,
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                    {
                        BottomMargin = Common.verticalSpacing,
                    },
                    Gravity = (int)GravityFlags.CenterVertical,
                };

                var bottomLine = new LinearLayoutCompat(Context)
                {
                    Orientation = Horizontal,
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent),
                    Gravity = (int)GravityFlags.CenterVertical,
                };

                radioButton2 = new AppCompatRadioButton(context);
                radioButton2.Click += (a, b) => SecondLine_Click();

                var theLabel = new LabelTextView(context) { Text = "The" };
                var everyLabel2 = new LabelTextView(context) { Text = "of every" };
                var monthsLabel2 = new LabelTextView(context) { Text = "month(s)" };
                monthsField2 = new NumberField(context);
                monthsField2.TextModified += MonthsField2_TextChanged;
                weekDayField = new ExtendedWeekDayPicker(context, UpdateWeekDays);
                weekOfMonthField = new WeekOfMonthPicker(context, UpdateWeekOfMonth);


                secondLine.AddView(radioButton2);
                secondLine.AddView(container);

                container.AddView(topLine);
                container.AddView(bottomLine);

                topLine.AddView(theLabel);
                topLine.AddView(weekOfMonthField);
                topLine.AddView(weekDayField);

                bottomLine.AddView(everyLabel2);
                bottomLine.AddView(monthsField2);
                bottomLine.AddView(monthsLabel2);

                AddView(firstLine);
                AddView(secondLine);
            }

            private void FirstLine_Click()
            {
                radioButton1.Checked = true;
                radioButton2.Checked = false;
                UpdateModel();
            }

            private void SecondLine_Click()
            {
                radioButton1.Checked = false;
                radioButton2.Checked = true;
                UpdateModel();
            }

            private void DaysTextField_TextChanged(object sender, string e)
            {
                FirstLine_Click();
            }

            private void MonthsField1_TextChanged(object sender, string e)
            {
                FirstLine_Click();
            }

            private void MonthsField2_TextChanged(object sender, string e)
            {
                SecondLine_Click();
            }

            private void UpdateWeekOfMonth(WeekOfMonth wm)
            {
                ri.WeekOfMonth = wm;
                SecondLine_Click();
            }

            private void UpdateWeekDays(WeekDays wd)
            {
                ri.WeekDays = wd;
                SecondLine_Click();
            }

            void UpdateModel()
            {
                if (radioButton1.Checked)
                {
                    ri.WeekOfMonth = WeekOfMonth.None;
                    ri.Periodicity = int.TryParse(monthsField1.Text, out var p) ? p : 1;
                    ri.DayNumber = int.TryParse(dayTextField.Text, out var s) ? s : 1;
                }
                else if (radioButton2.Checked)
                {
                    ri.Periodicity = int.TryParse(monthsField2.Text, out var s) ? s : 1;
                }
            }

            public override void Refresh()
            {
                if (ri.Type != RecurrenceType.Monthly)
                {
                    Visibility = ViewStates.Gone;
                    return;
                }

                Visibility = ViewStates.Visible;

                dayTextField.SetText(ri.DayNumber.ToString());
                monthsField1.SetText(ri.Periodicity.ToString());
                monthsField2.SetText(ri.Periodicity.ToString());
                weekDayField.SetSelected(ri.WeekDays);

                if (ri.WeekOfMonth == WeekOfMonth.None)
                {
                    radioButton1.Checked = true;
                    radioButton2.Checked = false;
                }
                else
                {
                    radioButton1.Checked = false;
                    radioButton2.Checked = true;

                    weekOfMonthField.SetSelected(ri.WeekOfMonth);
                }
            }
        }

        class YearlyView : RecurrenceSubView
        {
            AppCompatRadioButton radioButton1;
            AppCompatRadioButton radioButton2;
            NumberField dayTextField;

            ExtendedWeekDayPicker weekDayField;
            WeekOfMonthPicker weekOfMonthField;
            MonthPicker monthField1;
            MonthPicker monthField2;

            public YearlyView(Context context) : base(context)
            {
                Orientation = Vertical;
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

                var firstLine = new LinearLayoutCompat(Context)
                {
                    Orientation = Horizontal,
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                    {
                        BottomMargin = Common.verticalSpacing,
                    },
                    Gravity = (int)GravityFlags.CenterVertical,
                };
                firstLine.Click += (a, b) => FirstLine_Click();

                radioButton1 = new AppCompatRadioButton(context);
                radioButton1.Click += (a, b) => FirstLine_Click();

                dayTextField = new NumberField(context);
                dayTextField.TextModified += DaysTextField_TextChanged;
                var everyLabel = new LabelTextView(context) { Text = "Every" };
                monthField1 = new MonthPicker(context, UpdateMonth1);

                firstLine.AddView(radioButton1);
                firstLine.AddView(everyLabel);
                firstLine.AddView(monthField1);
                firstLine.AddView(dayTextField);

                var secondLine = new LinearLayoutCompat(Context)
                {
                    Orientation = Horizontal,
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                };
                secondLine.Click += (a, b) => SecondLine_Click();

                var container = new LinearLayoutCompat(Context)
                {
                    Orientation = Vertical,
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                };

                var topLine = new LinearLayoutCompat(Context)
                {
                    Orientation = Horizontal,
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                    {
                        BottomMargin = Common.verticalSpacing,
                    },
                    Gravity = (int)GravityFlags.CenterVertical,
                };

                var bottomLine = new LinearLayoutCompat(Context)
                {
                    Orientation = Horizontal,
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent),
                    Gravity = (int)GravityFlags.CenterVertical,
                };

                radioButton2 = new AppCompatRadioButton(context);
                radioButton2.Click += (a, b) => SecondLine_Click();

                var theLabel = new LabelTextView(context) { Text = "The" };
                var ofLabel = new LabelTextView(context) { Text = "of" };
                monthField2 = new MonthPicker(context, UpdateMonth2);
                weekDayField = new ExtendedWeekDayPicker(context, UpdateWeekDays);
                weekOfMonthField = new WeekOfMonthPicker(context, UpdateWeekOfMonth);

                secondLine.AddView(radioButton2);
                secondLine.AddView(container);

                container.AddView(topLine);
                container.AddView(bottomLine);

                topLine.AddView(theLabel);
                topLine.AddView(weekOfMonthField);
                topLine.AddView(weekDayField);

                bottomLine.AddView(ofLabel);
                bottomLine.AddView(monthField2);

                AddView(firstLine);
                AddView(secondLine);
            }

            private void FirstLine_Click()
            {
                radioButton1.Checked = true;
                radioButton2.Checked = false;
                UpdateModel();
            }

            private void SecondLine_Click()
            {
                radioButton1.Checked = false;
                radioButton2.Checked = true;
                UpdateModel();
            }

            private void DaysTextField_TextChanged(object sender, string e)
            {
                var tryParse = int.TryParse(dayTextField.Text, out var par);
                if (tryParse && par > 31)
                    dayTextField.SetText("31");

                FirstLine_Click();
            }

            private void UpdateWeekOfMonth(WeekOfMonth wm)
            {
                ri.WeekOfMonth = wm;
                SecondLine_Click();
            }

            private void UpdateMonth1(int m)
            {
                ri.Month = m;
                FirstLine_Click();
            }

            private void UpdateMonth2(int m)
            {
                ri.Month = m;
                SecondLine_Click();
            }

            private void UpdateWeekDays(WeekDays wd)
            {
                ri.WeekDays = wd;
                SecondLine_Click();
            }

            void UpdateModel()
            {
                if (radioButton1.Checked)
                {
                    ri.WeekOfMonth = WeekOfMonth.None;
                    ri.DayNumber = int.TryParse(dayTextField.Text, out var s) ? s : 1;
                }
            }

            public override void Refresh()
            {
                if (ri.Type != RecurrenceType.Yearly)
                {
                    Visibility = ViewStates.Gone;
                    return;
                }

                Visibility = ViewStates.Visible;

                dayTextField.SetText(ri.DayNumber.ToString());
                monthField1.SetSelected(ri.Month);

                monthField2.SetSelected(ri.Month);
                weekDayField.SetSelected(ri.WeekDays);

                if (ri.WeekOfMonth == WeekOfMonth.None)
                {
                    radioButton1.Checked = true;
                    radioButton2.Checked = false;
                }
                else
                {
                    radioButton1.Checked = false;
                    radioButton2.Checked = true;

                    weekOfMonthField.SetSelected(ri.WeekOfMonth);
                }

                Invalidate();
                RequestLayout();
            }
        }

    }
}
