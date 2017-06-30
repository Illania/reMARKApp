using System.Linq;
using Android.Content;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class MiddleNameView : AbstractStringSingleRowView
    {
        public MiddleNameView(Context context)
            : base(context, Resource.String.edit_contact_middle_name)
        {
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(Contact.Patronymic))
                AddRow(Contact.Patronymic);
        }

        public override void UpdateContact()
        {
            string name;
            if (Rows.Any() && !string.IsNullOrEmpty(name = Rows[0].GetContent()))
                Contact.Patronymic = name;
        }
    }
}
