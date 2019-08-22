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

namespace Mark5.Mobile.Droid.Ui.Views.RecurrenceViews
{

    public static class Common
    {
        public static int interviewHorizontalSpacing = Conversion.ConvertDpToPixels(10f);
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

    public abstract class RecurrenceParentView : LinearLayoutCompat, IEditable
    {
        protected RecurrenceInfo ri;
        public RecurrenceParentView(Context context) : base(context)
        {
            LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            {
                LeftMargin = Conversion.ConvertDpToPixels(10f),
            };
        }

        public abstract void Refresh();

        public abstract void SetViewModel(RecurrenceInfo ri);
    }

    public abstract class RecurrenceSubView : LinearLayoutCompat, IEditable
    {
        protected RecurrenceInfo ri;
        public RecurrenceSubView(Context context) : base(context)
        {
        }

        public abstract void Refresh();

        public void SetViewModel(RecurrenceInfo ri)
        {
            this.ri = ri;
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
            LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
            {
                RightMargin = Common.interviewHorizontalSpacing,
            };
        }
    }

    public abstract class PickerField<T> : AppCompatSpinner
    {
        protected Action<T> selectedAction;

        public PickerField(Context context, Action<T> selectedAction, List<string> data) : base(context)
        {
            this.selectedAction = selectedAction;

            Adapter = new CommonAdapter(context, data);

            ItemSelected += PickerField_ItemSelected;
        }

        private void PickerField_ItemSelected(object sender, ItemSelectedEventArgs e)
        {
            Update(e.Position);
        }

        abstract public void Update(int i);

        abstract public void SetSelected(T value);
    }

    public class CommonAdapter : ArrayAdapter
    {
        readonly List<string> data;
        Context context;

        public CommonAdapter(Context context, List<string> data)
            : base(context, Android.Resource.Layout.SimpleSpinnerItem)
        {
            this.context = context;
            this.data = data;
        }

        public override int Count => data.Count;

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView ?? new AppCompatTextView(Context);

            (view as AppCompatTextView).Text = data[position];
            return view;
        }

        public override View GetDropDownView(int position, View convertView, ViewGroup parent)
        {
            return GetView(position, convertView, parent);
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
