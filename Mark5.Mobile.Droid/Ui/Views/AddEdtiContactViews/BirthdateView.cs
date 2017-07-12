using System;
using Android.Content;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class BirthdateView : AbstractSimpleFieldView
    {
        public BirthdateView(Context context)
            : base(context, Resource.String.edit_contact_birthdate, true, false,
                   inputType: Android.Text.InputTypes.DatetimeVariationDate)
        {
        }

        public override bool ContainsValidContent() => true;

        public override void RefreshView()
        {
            if (Contact.BirthDateTimestamp != -6847804800000 && Contact.BirthDateTimestamp != -1)
                Content = Contact.BirthDateTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime().ConvertDateTimeToTimestampMilliseconds()
                        .FormatUserTimestampAsLongDateString(Context);
        }

        protected override async void ContentClicked(object sender, EventArgs e)
        {
            long userTimestamp = -1;
            if (Contact.BirthDateTimestamp != -6847804800000 && Contact.BirthDateTimestamp != -1)
                userTimestamp = Contact.BirthDateTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime().ConvertDateTimeToTimestampMilliseconds();

            var newTimestamp = await Dialogs.ShowDatePicker(Context, userTimestamp, addRemoveDateChoice: true);

            if (newTimestamp == 0)
            {
                Clear();
                return;
            }

            if (newTimestamp != -1)
            {
                var utcTimestamp = newTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUserTimeToUtc().ConvertDateTimeToTimestampMilliseconds();
                Contact.BirthDateTimestamp = utcTimestamp;
            }

            RefreshView();
        }

        void Clear()
        {
            Content = string.Empty;
            Contact.BirthDateTimestamp = -1;
        }

    }
}
