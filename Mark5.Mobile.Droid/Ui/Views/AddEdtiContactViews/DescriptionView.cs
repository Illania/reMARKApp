using System.Linq;
using Android.Content;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class DescriptionView : AbstractStringSingleRowView
    {
        public DescriptionView(Context context)
            : base(context, Resource.String.edit_contact_description)
        {
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(ContactPreview.Description))
                AddRow(ContactPreview.Description);
        }

        public override void UpdateContact()
        {
            string shortId;
            if (Rows.Any() && !string.IsNullOrEmpty(shortId = Rows[0].GetContent()))
                ContactPreview.Description = shortId;
        }
    }
}