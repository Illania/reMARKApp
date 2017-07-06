using System;
using Android.Content;
using Android.Text;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class DescriptionView : AbstractSimpleFieldView
    {
        public DescriptionView(Context context)
            : base(context, Resource.String.edit_contact_description, true)
        {
        }

        public override void RefreshView()
        {
            Content = ContactPreview.Description;
        }

        protected override void ContentChanged(object sender, TextChangedEventArgs e)
        {
            ContactPreview.Description = Content;
        }
    }
}