using System.Linq;
using Android.Content;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class VatView : AbstractStringSingleRowView
    {
        public VatView(Context context)
            : base(context, Resource.String.edit_contact_vat)
        {
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(Contact.Vat))
                AddRow(Contact.Vat);
        }

        public override void UpdateContact()
        {
            string position;
            if (Rows.Any() && !string.IsNullOrEmpty(position = Rows[0].GetContent()))
                Contact.Vat = position;
        }
    }
}