using System;
using Android.Content;
using Android.Text;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    //To be used with deparment and companies
    public class NameView : AbstractSimpleFieldView
    {
        public NameView(Context context)
            : base(context, Resource.String.edit_contact_name, false, true, Resource.String.edit_contact_name_error)
        {
        }

        public bool ContainsValidContent()
        {
            return !string.IsNullOrEmpty(ContactPreview.Name);
        }

        public override void RefreshView()
        {
            Content = ContactPreview.Name;
        }

        protected override void ContentChanged(object sender, TextChangedEventArgs e)
        {
            ContactPreview.Name = Content;
        }
    }
}
