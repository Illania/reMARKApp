using System;
using Android.Content;
using Android.Text;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    //To be used with deparment and companies
    public class NameView : AbstractSimpleFieldView
    {
        public NameView(Context context)
            : base(context, Resource.String.edit_contact_name, false)
        {
        }

        public override void RefreshView()
        {
            ContactPreview.Name = Content;
        }

        protected override void ContentChanged(object sender, TextChangedEventArgs e)
        {
            ContactPreview.Name = Content;
        }
    }
}
