using System;
using Android.Content;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public class BirthdateSubview : DescriptionCardSubview
    {
        public BirthdateSubview(Context context)
            : base(context)
        {
            Title = context.GetString(Resource.String.birthdate);
        }

        public override void RefreshView()
        {
            if (ContactPreview?.Type != ContactType.Person || Contact?.BirthDateTimestamp == -1)
            {
                Visibility = ViewStates.Gone;
            }
            else
            {
                Visibility = ViewStates.Visible;
                Content = Contact?.BirthDateTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToServerTime()
                                  .ConvertDateTimeToTimestampMilliseconds().FormatUserTimestampAsLongDateString(Context);
            }
        }
    }
}