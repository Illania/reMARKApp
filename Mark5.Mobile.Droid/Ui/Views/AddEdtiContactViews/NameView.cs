using System;
using System.Linq;
using Android.Content;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class FirstNameView : StringSingleRow
    {
        public FirstNameView(Context context)
            : base(context, Resource.String.edit_contact_first_name)
        {
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(Contact.FirstName))
                AddRow(Contact.FirstName);
        }

        public override void UpdateContact()
        {
            string name;
            if (Rows.Any() && !string.IsNullOrEmpty(name = Rows[0].GetContent()))
                Contact.FirstName = name;
        }
    }
}
