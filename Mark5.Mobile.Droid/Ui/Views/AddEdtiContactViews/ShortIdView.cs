using System.Linq;
using Android.Content;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class ShortIdView : StringSingleRowView
    {
        public ShortIdView(Context context)
            : base(context, Resource.String.edit_contact_short_id)
        {
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(ContactPreview.ShortId))
                AddRow(ContactPreview.ShortId);
        }

        public override void UpdateContact()
        {
            string shortId;
            if (Rows.Any() && !string.IsNullOrEmpty(shortId = Rows[0].GetContent()))
                ContactPreview.ShortId = shortId;
        }
    }
}