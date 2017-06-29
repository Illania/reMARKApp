using System;
using System.Linq;
using Android.Content;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class LastNameView : StringSingleRowView
    {
        public LastNameView(Context context)
            : base(context, Resource.String.edit_contact_last_name)
        {
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(Contact.LastName))
                AddRow(Contact.LastName);
        }

        public override void UpdateContact()
        {
            string name;
            if (Rows.Any() && !string.IsNullOrEmpty(name = Rows[0].GetContent()))
                Contact.LastName = name;
        }
    }
}
