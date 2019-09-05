using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.Droid.Utilities;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Android.Support.V4.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Text.Method;

namespace Mark5.Mobile.Droid.Ui.Views.RecurrenceViews
{

    public static class Common
    {
        public static int interviewHorizontalSpacing = Conversion.ConvertDpToPixels(8f);
        public static int commonPadding = Conversion.ConvertDpToPixels(10f);
        public static int pickerPadding = Conversion.ConvertDpToPixels(6.5f);
        public static int verticalSpacing = Conversion.ConvertDpToPixels(7f);

        public static List<RecurrenceType> recurrenceTypes = new List<RecurrenceType> { RecurrenceType.Daily, RecurrenceType.Weekly, RecurrenceType.Monthly, RecurrenceType.Yearly };

        public static List<WeekDays> weekDaysExtended = new List<WeekDays>
                { WeekDays.EveryDay, WeekDays.WorkDays, WeekDays.WeekendDays, WeekDays.Monday, WeekDays.Tuesday,WeekDays.Wednesday,
                    WeekDays.Thursday, WeekDays.Friday, WeekDays.Saturday, WeekDays.Sunday };

        public static List<WeekOfMonth> weekOfMonth = new List<WeekOfMonth> { WeekOfMonth.First, WeekOfMonth.Second, WeekOfMonth.Third, WeekOfMonth.Fourth, WeekOfMonth.Last };

        public static List<WeekDays> weekDays = new List<WeekDays> { WeekDays.Monday, WeekDays.Tuesday, WeekDays.Wednesday,
                    WeekDays.Thursday, WeekDays.Friday, WeekDays.Saturday, WeekDays.Sunday };

        public static List<string> months = new List<string> { "January", "February", "March", "April", "May", "June", "July", "August", "September",
        "October", "November", "Dicember"};

        public static Drawable GetBackground(Context context)
        {
            var shape = new GradientDrawable();
            shape.SetCornerRadius(Conversion.ConvertDpToPixels(4f));
            shape.SetColor(ContextCompat.GetColor(context, Resource.Color.lightgray));

            return shape;
        }
    }

    interface IEditable
    {
        void Refresh();
        void SetViewModel(RecurrenceInfo ri);
    }

    public abstract class RecurrenceParentView : LinearLayoutCompat, IEditable
    {
        protected RecurrenceInfo ri;
        public RecurrenceParentView(Context context) : base(context)
        {
            LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        }

        public abstract void Refresh();

        public abstract void SetViewModel(RecurrenceInfo ri);
    }

    public abstract class RecurrenceSubView : LinearLayoutCompat, IEditable
    {
        protected RecurrenceInfo ri;
        public RecurrenceSubView(Context context) : base(context)
        {
            SetPadding(Common.commonPadding, Common.commonPadding, Common.commonPadding, Common.commonPadding);
        }

        public abstract void Refresh();

        public void SetViewModel(RecurrenceInfo ri)
        {
            this.ri = ri;
        }
    }

    public class SeparatorView : LinearLayoutCompat
    {
        public SeparatorView(Context context) : base(context)
        {
            LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, Conversion.ConvertDpToPixels(1));
            SetBackgroundColor(new Color(ContextCompat.GetColor(context, Resource.Color.lightgray)));
        }
    }

    public class DateField : TextView
    {
        Action<DateTime> selectedAction;
        DateTime currentDate;

        public DateField(Context context, Action<DateTime> selectedAction) : base(context)
        {
            this.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);

            SetPadding(Common.pickerPadding, Common.pickerPadding, Common.pickerPadding, Common.pickerPadding);

            Background = Common.GetBackground(context);

            this.selectedAction = selectedAction;
            Click += DateField_Click;
        }

        async void DateField_Click(object sender, EventArgs e)
        {
            long userTimestamp = -1;

            if (currentDate != default)
                userTimestamp = currentDate.ConvertDateTimeToTimestampMilliseconds();

            var newTimestamp = await Dialogs.ShowDatePicker(Context, userTimestamp, addRemoveDateChoice: false);

            var newDate = newTimestamp.ConvertTimestampMillisecondsToDateTime();

            currentDate = newDate;
            UpdateText();
            selectedAction?.Invoke(newDate);
        }

        public void SetDate(DateTime dt)
        {
            if (dt != default)
                currentDate = dt;
            else
                currentDate = DateTime.Now;

            UpdateText();
        }

        public void UpdateText()
        {
            Text = currentDate.ConvertDateTimeToTimestampMilliseconds().FormatUserTimestampAsLongDateString(Context);
        }
    }

    public class WeekDaysSelectionView : LinearLayoutCompat
    {
        List<DayView> dayViews = new List<DayView>();

        public WeekDaysSelectionView(Context context) : base(context)
        {
            LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            Orientation = Horizontal;

            Common.weekDays.ForEach(wd => dayViews.Add(new DayView(context, wd)));
            dayViews.ForEach(AddView);
        }

        public void Refresh(WeekDays wd)
        {
            dayViews.ForEach(dv => dv.Refresh(wd));
        }

        class DayView : AppCompatTextView
        {
            readonly WeekDays weekDay;
            public DayView(Context context, WeekDays wd) : base(context)
            {
                Text = wd.ToFriendlyString().ToUpper()[0].ToString();
                TextAlignment = TextAlignment.Center;
                Gravity = GravityFlags.Center;

                weekDay = wd;

                var dimension = Conversion.ConvertDpToPixels(38f);

                LayoutParameters = new LayoutParams(dimension, dimension)
                {
                    RightMargin = Conversion.ConvertDpToPixels(5f),
                };

                Click += DayView_Click;
            }

            private void DayView_Click(object sender, EventArgs e)
            {
                Selected = !Selected;
                UpdateUI();
            }

            public void Refresh(WeekDays wd)
            {
                if (wd.HasFlag(weekDay))
                    Selected = true;
                else
                    Selected = false;

                UpdateUI();
            }

            void UpdateUI()
            {
                SetBackgroundResource(Selected ? Resource.Drawable.circle_blue : Resource.Drawable.circle_white);
            }
        }
    }

    public class LabelTextView : AppCompatTextView
    {
        public LabelTextView(Context context) : base(context)
        {
            this.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
            {
                RightMargin = Common.interviewHorizontalSpacing,
            };
        }
    }

    public class SpecialLabelTextView : AppCompatTextView
    {
        public SpecialLabelTextView(Context context) : base(context)
        {
            this.SetTextAppearanceCompat(context, Resource.Style.fontPrimaryLight);
            LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
            {
                RightMargin = Common.interviewHorizontalSpacing,
            };
        }
    }

    public class NumberField : AppCompatEditText
    {
        public NumberField(Context context) : base(context)
        {
            this.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);

            Background = Common.GetBackground(context);

            LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
            {
                RightMargin = Common.interviewHorizontalSpacing,
            };

            SetMinimumWidth(Conversion.ConvertDpToPixels(25f));
            SetPadding(Common.pickerPadding, Common.pickerPadding, Common.pickerPadding, Common.pickerPadding);

            InputType = Android.Text.InputTypes.ClassNumber;
            //KeyListener = DigitsKeyListener.GetInstance(null, false, false);
        }
    }

    public abstract class PickerField<T> : LinearLayoutCompat
    {
        protected Action<T> selectedAction;
        AppCompatSpinner spinner;

        public PickerField(Context context, Action<T> selectedAction, List<string> data) : base(context)
        {
            this.selectedAction = selectedAction;

            Background = Common.GetBackground(context);

            LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
            {
                RightMargin = Common.interviewHorizontalSpacing,
            };
            SetPadding(Common.pickerPadding, Common.pickerPadding, Common.pickerPadding, Common.pickerPadding);

            spinner = new AppCompatSpinner(context);
            spinner.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            spinner.Adapter = new CommonAdapter(context, data);
            spinner.ItemSelected += PickerField_ItemSelected;
            spinner.SetPadding(0, 0, 0, 0);
            spinner.Background = null;

            AddView(spinner);
        }

        protected void SetSelection(int index)
        {
            spinner.SetSelection(index);
        }

        private void PickerField_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Update(e.Position);
        }

        abstract public void Update(int i);

        abstract public void SetSelected(T value);
    }

    public class CommonAdapter : ArrayAdapter
    {
        readonly List<string> data;

        public CommonAdapter(Context context, List<string> data)
            : base(context, Android.Resource.Layout.SimpleSpinnerItem)
        {
            this.data = data;
        }

        public override int Count => data.Count;

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = (convertView ?? new AppCompatTextView(Context)) as AppCompatTextView;
            view.LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            view.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);
            view.Text = data[position];
            return view;
        }

        public override View GetDropDownView(int position, View convertView, ViewGroup parent)
        {
            var view = GetView(position, convertView, parent);
            view.SetPadding(Common.pickerPadding, Common.pickerPadding, Common.pickerPadding, Common.pickerPadding);
            return view;
        }
    }

    public class TypePicker : PickerField<RecurrenceType>
    {
        public TypePicker(Context context, Action<RecurrenceType> selectedAction)
            : base(context, selectedAction, Common.recurrenceTypes.Select(w => w.ToFriendlyString()).ToList())
        {
        }

        public override void SetSelected(RecurrenceType value)
        {
            var index = Common.recurrenceTypes.FindIndex(i => i == value);
            SetSelection(index);
        }

        public override void Update(int i)
        {
            selectedAction?.Invoke(Common.recurrenceTypes[i]);
        }
    }

    public class WeekDayPicker : PickerField<WeekDays>
    {
        public WeekDayPicker(Context context, Action<WeekDays> selectedAction)
            : base(context, selectedAction, Common.weekDays.Select(w => w.ToFriendlyString()).ToList())
        {
        }

        public override void SetSelected(WeekDays value)
        {
            var index = Common.weekDays.FindIndex(i => i == value);
            SetSelection(index);
        }

        public override void Update(int i)
        {
            selectedAction?.Invoke(Common.weekDays[i]);
        }
    }

    public class ExtendedWeekDayPicker : PickerField<WeekDays>
    {
        public ExtendedWeekDayPicker(Context context, Action<WeekDays> selectedAction)
            : base(context, selectedAction, Common.weekDaysExtended.Select(w => w.ToFriendlyString()).ToList())
        {
        }

        public override void SetSelected(WeekDays value)
        {
            var index = Common.weekDaysExtended.FindIndex(i => i == value);
            SetSelection(index);
        }

        public override void Update(int i)
        {
            selectedAction?.Invoke(Common.weekDaysExtended[i]);
        }
    }

    public class WeekOfMonthPicker : PickerField<WeekOfMonth>
    {
        public WeekOfMonthPicker(Context context, Action<WeekOfMonth> selectedAction)
            : base(context, selectedAction, Common.weekOfMonth.Select(w => w.ToFriendlyString()).ToList())
        {
        }

        public override void SetSelected(WeekOfMonth value)
        {
            var index = Common.weekOfMonth.FindIndex(i => i == value);
            SetSelection(index);
        }

        public override void Update(int i)
        {
            selectedAction?.Invoke(Common.weekOfMonth[i]);
        }
    }

    public class MonthPicker : PickerField<int>
    {
        public MonthPicker(Context context, Action<int> selectedAction)
            : base(context, selectedAction, Common.months)
        {
        }

        public override void SetSelected(int value)
        {
            SetSelection(value);
        }

        public override void Update(int i)
        {
            selectedAction?.Invoke(i);
        }
    }
}
