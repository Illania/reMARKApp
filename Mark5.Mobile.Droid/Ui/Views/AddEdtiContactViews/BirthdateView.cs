using System;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public abstract class BirthdateView : AbstractMultipleRowsView<long>
    {
        public BirthdateView(Context context)
            : base(context, Resource.String.edit_contact_birthdate, true)
        {
        }

        public override void RefreshView()
        {
            if (Contact.BirthDateTimestamp != -6847804800000 && Contact.BirthDateTimestamp != -1)
                AddRow(Contact.BirthDateTimestamp); //TODO need a conversion!
        }

        //var currentTimestamp = timestamp == -1 || timestamp == 0 ? -1 : timestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime().ConvertDateTimeToTimestampMilliseconds();

        protected override Row GetNewRow()
        {
            return new DateRow(Context, this);
        }

        protected override void AddButton_Click(object sender, EventArgs e)
        {
            CreateDialog();
        }

        async void CreateDialog(long timestamp = -1, DateRow row = null)
        {
            var newTimestamp = await Dialogs.ShowDatePicker(Context, timestamp);

            if (newTimestamp != -1)
            {
                if (row == null)
                {
                    AddRow(newTimestamp);
                }
                else
                {
                    row.SetContent(newTimestamp);
                }
            }
        }

        protected class DateRow : Row
        {
            readonly AppCompatEditText dateEditText;
            long currentTimestamp;

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

            protected override void UpdateRow()
            {
                if (currentTimestamp != -1)
                {
                    dateEditText.Text = currentTimestamp.FormatUserTimestampAsLongDateString(Context);
                }
                else
                {
                    dateEditText.Text = string.Empty;
                }
            }

            void EditText_Click(object sender, EventArgs e)
            {
                (ParentView as BirthdateView).CreateDialog(currentTimestamp, this);
            }

            public override bool ContainsValidContent()
            {
                throw new NotImplementedException();
            }
        }

    }
}
