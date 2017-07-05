using System;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class BirthdateView : AbstractMultipleRowsView<long>
    {
        public BirthdateView(Context context)
            : base(context, Resource.String.edit_contact_birthdate, true)
        {
        }

        public override void RefreshView()
        {
            if (Contact.BirthDateTimestamp != -6847804800000 && Contact.BirthDateTimestamp != -1)
                AddRow(Contact.BirthDateTimestamp);
        }

        protected override Row GetNewRow()
        {
            return new DateRow(Context, this);
        }

        protected override void AddButton_Click(object sender, EventArgs e)
        {
            CreateDialog();
        }

        protected override void Row_DeleteClicked(object sender, EventArgs e)
        {
            Contact.BirthDateTimestamp = -1;
            RemoveRow(sender as DateRow);
        }

        async void CreateDialog(DateRow row = null)
        {
            long timestamp = -1;

            if (row != null)
            {
                timestamp = row.GetContent();
            }

            var newTimestamp = await Dialogs.ShowDatePicker(Context, timestamp);

            if (newTimestamp != -1)
            {
                var utcTimestamp = newTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUserTimeToUtc().ConvertDateTimeToTimestampMilliseconds();
                Contact.BirthDateTimestamp = utcTimestamp;

                if (row == null)
                {
                    AddRow(utcTimestamp);
                }
                else
                {
                    row.SetContent(utcTimestamp);
                }
            }
        }

        protected class DateRow : Row
        {
            readonly AppCompatEditText dateEditText;

            public DateRow(Context context, BirthdateView birthdateView)
                : base(context, birthdateView)
            {
                dateEditText = new AppCompatEditText(context);

                var editTextLp = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1.0f)
                {
                    Gravity = (int)GravityFlags.CenterVertical,
                };

                dateEditText.RequestFocus();
                dateEditText.Focusable = false;
                dateEditText.KeyListener = null;
                dateEditText.Click += EditText_Click;
                Layout.AddView(dateEditText, 0, editTextLp);
            }

            public override void UpdateRow()
            {
                if (Content != -1)
                {
                    dateEditText.Text = Content.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime().ConvertDateTimeToTimestampMilliseconds()
                        .FormatUserTimestampAsLongDateString(Context);
                }
                else
                {
                    dateEditText.Text = string.Empty;
                }
            }

            void EditText_Click(object sender, EventArgs e)
            {
                ((BirthdateView)ParentView).CreateDialog(this);
            }

            public override bool ContainsValidContent()
            {
                throw new NotImplementedException();
            }
        }

    }
}
