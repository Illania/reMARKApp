using System.Linq;
using Android.Content;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class WebpageView : StringSingleRowView
    {
        public WebpageView(Context context)
            : base(context, Resource.String.edit_contact_webpage)
        {
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(Contact.WebPageAddress))
                AddRow(Contact.WebPageAddress);
        }

        public override void UpdateContact()
        {
            string position;
            if (Rows.Any() && !string.IsNullOrEmpty(position = Rows[0].GetContent()))
                Contact.WebPageAddress = position;
        }
    }
}