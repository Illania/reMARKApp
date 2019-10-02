using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews.RecurrenceView
{
    public static class Common
    {
        public const float radioButtonSpacing = 5f;
        public const float interviewHorizontalSpacing = 5f;
        public const float interviewVerticalSpacing = 3f;
        public const float topSpacing = 1f;
        public const float bottomSpacing = -1f;
        public const float internalStackViewSpacing = 5f;
        public const float stackViewSpacing = 10f;

        public static List<RecurrenceType> recurrenceTypes = new List<RecurrenceType> { RecurrenceType.Daily, RecurrenceType.Weekly, RecurrenceType.Monthly, RecurrenceType.Yearly };

        public static List<WeekDays> weekDaysExtended = new List<WeekDays>
                { WeekDays.EveryDay, WeekDays.WorkDays, WeekDays.WeekendDays, WeekDays.Monday, WeekDays.Tuesday,WeekDays.Wednesday,
                    WeekDays.Thursday, WeekDays.Friday, WeekDays.Saturday, WeekDays.Sunday };

        public static List<WeekOfMonth> weekOfMonth = new List<WeekOfMonth> { WeekOfMonth.First, WeekOfMonth.Second, WeekOfMonth.Third, WeekOfMonth.Fourth, WeekOfMonth.Last };

        public static List<WeekDays> weekDays = new List<WeekDays> { WeekDays.Monday, WeekDays.Tuesday, WeekDays.Wednesday,
                    WeekDays.Thursday, WeekDays.Friday, WeekDays.Saturday, WeekDays.Sunday };

        public static List<string> months = new List<string> { "January", "February", "March", "April", "May", "June", "July", "August", "September",
        "October", "November", "Dicember"};
    }


    interface IEditable
    {
        void Refresh();
        void SetViewModel(RecurrenceInfo ri);
    }

    abstract class BaseField : UITextField
    {
        const float insetVal = 4f;

        public BaseField(bool editable = false)
        {
            Font = Theme.DefaultFont;
            TranslatesAutoresizingMaskIntoConstraints = false;
            Layer.BorderColor = Theme.DarkerBlue.CGColor;
            Layer.BorderWidth = 1f;
            Layer.CornerRadius = 8f;
            TextColor = Theme.DarkerBlue;
            if (!editable)
                TintColor = Theme.Clear;

            SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);

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

    class NumberField : BaseField
    {
        public NumberField()
            : base(true)
        {
            Text = "1";
            KeyboardType = UIKeyboardType.NumberPad;

            EditingDidEnd += NumberField_EditingDidEnd;
        }

        private void NumberField_EditingDidEnd(object sender, EventArgs e)
        {
            var tryParse = int.TryParse(Text, out _);
            if (!tryParse)
                Text = "1";
        }
    }

    abstract class BasePickerField : BaseField
    {
        public BasePickerField()
            : base(false)
        {

        }
    }

    abstract class PickerField<T> : BasePickerField
    {
        protected Action<T> selectedAction;

        public PickerField(Action<T> selectedAction, List<string> data)
        {
            this.selectedAction = selectedAction;

            InputView = new UIPickerView
            {
                Model = new PickerViewModel(data, Update)
            };
        }

        abstract public void SetSelected(T value);

        abstract protected void Update(int i);

        abstract protected void SetText(T value);

    }

    sealed class TypePicker : PickerField<RecurrenceType>
    {
        public TypePicker(Action<RecurrenceType> selectedAction)
            : base(selectedAction, Common.recurrenceTypes.Select(w => w.ToFriendlyString()).ToList())
        {
        }

        protected override void Update(int i)
        {
            SetText(Common.recurrenceTypes[i]);
            selectedAction?.Invoke(Common.recurrenceTypes[i]);
        }

        public override void SetSelected(RecurrenceType value)
        {
            SetText(value);
            var index = Common.recurrenceTypes.FindIndex(i => i == value);
            (InputView as UIPickerView).Select(index, 0, false);
        }

        protected override void SetText(RecurrenceType value)
        {
            Text = value.ToFriendlyString();
        }
    }

    class WeekDayPicker : PickerField<WeekDays>
    {
        public WeekDayPicker(Action<WeekDays> selectedAction)
            : base(selectedAction, Common.weekDays.Select(w => w.ToFriendlyString()).ToList())
        {
        }

        protected override void Update(int i)
        {
            SetText(Common.weekDays[i]);
            selectedAction?.Invoke(Common.weekDays[i]);
        }

        public override void SetSelected(WeekDays value)
        {
            Text = value.ToFriendlyString();
            var index = Common.weekDays.FindIndex(i => i == value);
            (InputView as UIPickerView).Select(index, 0, false);
        }

        protected override void SetText(WeekDays value)
        {
            Text = value.ToFriendlyString();
        }

    }

    class ExtendedWeekDayPicker : PickerField<WeekDays>
    {
        public ExtendedWeekDayPicker(Action<WeekDays> selectedAction)
            : base(selectedAction, Common.weekDaysExtended.Select(w => w.ToFriendlyString()).ToList())
        {
        }

        protected override void Update(int i)
        {
            SetText(Common.weekDaysExtended[i]);
            selectedAction?.Invoke(Common.weekDaysExtended[i]);
        }

        public override void SetSelected(WeekDays value)
        {
            Text = value.ToFriendlyString();
            var index = Common.weekDaysExtended.FindIndex(i => i == value);
            (InputView as UIPickerView).Select(index, 0, false);
        }

        protected override void SetText(WeekDays value)
        {
            Text = value.ToFriendlyString();
        }
    }

    class WeekOfMonthPicker : PickerField<WeekOfMonth>
    {
        public WeekOfMonthPicker(Action<WeekOfMonth> selectedAction)
            : base(selectedAction, Common.weekOfMonth.Select(w => w.ToFriendlyString()).ToList())
        {
        }

        protected override void Update(int i)
        {
            SetText(Common.weekOfMonth[i]);
            selectedAction?.Invoke(Common.weekOfMonth[i]);
        }

        public override void SetSelected(WeekOfMonth value)
        {
            Text = value.ToFriendlyString();
            var index = Common.weekOfMonth.FindIndex(i => i == value);
            (InputView as UIPickerView).Select(index, 0, false);
        }

        protected override void SetText(WeekOfMonth value)
        {
            Text = value.ToFriendlyString();
        }
    }

    class MonthPicker : PickerField<int>
    {
        public MonthPicker(Action<int> selectedAction)
            : base(selectedAction, Common.months)
        {
        }

        protected override void Update(int i)
        {
            SetText(i);
            selectedAction?.Invoke(i + 1);
        }

        public override void SetSelected(int value)
        {
            if (value <= 0)
                value = 1;
            SetText(value - 1);
            (InputView as UIPickerView).Select(value - 1, 0, false);
        }

        protected override void SetText(int value)
        {
            Text = Common.months[value];
        }
    }

    class DateField : BaseField
    {
        UIDatePicker datePicker;
        Action<DateTime> selectedAction;

        public DateField(Action<DateTime> selectedAction)
            : base(false)
        {
            this.selectedAction = selectedAction;
            datePicker = new UIDatePicker
            {
                Mode = UIDatePickerMode.Date
            };
            datePicker.ValueChanged += DatePicker_ValueChanged;

            InputView = datePicker;
        }

        private void DatePicker_ValueChanged(object sender, EventArgs e)
        {
            var selectedDate = datePicker.Date;

            SetText(selectedDate);

            var selectedDateComponents = NSCalendar.CurrentCalendar.Components(NSCalendarUnit.Day | NSCalendarUnit.Month | NSCalendarUnit.Year, selectedDate);
            var date = new DateTime((int)selectedDateComponents.Year, (int)selectedDateComponents.Month, (int)selectedDateComponents.Day, 0, 0, 0, DateTimeKind.Local);

            selectedAction?.Invoke(date);
        }

        public void SetSelected(DateTime datetime)
        {
            var dateComponents = new NSDateComponents
            {
                Day = datetime.Day,
                Month = datetime.Month,
                Year = datetime.Year,
            };

            var nsDate = NSCalendar.CurrentCalendar.DateFromComponents(dateComponents);
            datePicker.SetDate(nsDate, false);
            SetText(nsDate);
        }

        void SetText(NSDate date)
        {
            var dateFormatter = new NSDateFormatter
            {
                DateStyle = NSDateFormatterStyle.Short,
                TimeStyle = NSDateFormatterStyle.None,
                TimeZone = NSTimeZone.LocalTimeZone,
                Locale = NSLocale.CurrentLocale,
            };

            Text = dateFormatter.StringFor(date);
        }
    }

    class TextLabel : UILabel
    {
        public TextLabel()
        {
            TranslatesAutoresizingMaskIntoConstraints = false;
            Font = Theme.DefaultFont;
            HeightAnchor.ConstraintGreaterThanOrEqualTo(30f).Active = true;
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

    class SeparatorSubView : UIView
    {
        static readonly UIColor backgroundColor = Theme.Blue;

        public SeparatorSubView()
        {
            BackgroundColor = backgroundColor;
            HeightAnchor.ConstraintEqualTo(1f).Active = true;
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
            Layer.CornerRadius = 7.5f;
            Layer.BorderColor = Theme.DarkerBlue.CGColor;
            Layer.BorderWidth = 1f;

            innerDisk = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            innerDisk.Layer.CornerRadius = 4f;
            innerDisk.BackgroundColor = Theme.DarkerBlue;

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
