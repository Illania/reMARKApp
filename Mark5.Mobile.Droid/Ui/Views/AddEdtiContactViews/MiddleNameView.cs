using System;
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

        protected override void Row_DeleteClicked(object sender, EventArgs e)
        {
            Contact.Patronymic = string.Empty;
            RemoveRow(sender as Row);
        }

        protected override void TextChanged(string newText)
        {
            Contact.Patronymic = newText;
        }

    }
}
