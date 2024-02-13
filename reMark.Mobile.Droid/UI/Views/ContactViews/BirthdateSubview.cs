using System;
using Android.Content;
using Android.Views;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.Droid.Utilities;

namespace reMark.Mobile.Droid.Ui.Views.ContactViews
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