using System.Linq;
using Android.Content;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class PositionView : AbstractStringSingleRowView
    {
        public PositionView(Context context)
            : base(context, Resource.String.edit_contact_position)
        {
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(Contact.Position))
                AddRow(Contact.Position);
        }

        public override void UpdateContact()
        {
            string position;
            if (Rows.Any() && !string.IsNullOrEmpty(position = Rows[0].GetContent()))
                Contact.Position = position;
        }
    }
}