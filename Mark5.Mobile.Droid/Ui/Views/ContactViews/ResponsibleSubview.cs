using System;
using System.Linq;
using Android.Content;
using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public class ResponsibleSubview : DescriptionSubview
    {
        public ResponsibleSubview(Context context)
            : base(context)
        {
            Title = context.GetString(Resource.String.responsible_users);
        }

        public override void RefreshView()
        {
            if (Contact?.ResponsibleUsers?.Count > 0)
            {
                Visibility = ViewStates.Visible;
                Content = string.Join(", ", Contact?.ResponsibleUsers.Values);
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }
    }
}