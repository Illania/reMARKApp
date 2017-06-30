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

        public override void UpdateContact()
        {
            throw new NotImplementedException();
        }

        protected override Row GetNewRow(long content)
        {
            return new DateRow(Context, content);
        }

        protected class DateRow : Row
        {
            readonly AppCompatEditText dateEditText;
            long currentTimestamp;
            Context context;

            public DateRow(Context context, long timestamp)
                : base(context, timestamp)
            {
                this.context = context;

                currentTimestamp = timestamp == -1 || timestamp == 0 ? -1 : timestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime().ConvertDateTimeToTimestampMilliseconds();

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

                UpdateText();

            }

            void UpdateText()
            {
                if (currentTimestamp != -1)
                {
                    dateEditText.Text = currentTimestamp.FormatUserTimestampAsLongDateString(context);
                }
                else
                {
                    dateEditText.Text = string.Empty;
                }
            }

            async void EditText_Click(object sender, EventArgs e)
            {
                currentTimestamp = await Dialogs.ShowDatePicker(context, currentTimestamp);

                UpdateText();
            }

            public override long GetContent()
            {
                return 0; //TODO correct
            }

            public override bool ContainsValidContent()
            {
                throw new NotImplementedException();
            }
        }

    }
}
