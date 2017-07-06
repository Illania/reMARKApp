using System;
using Android.Content;
using Android.Text;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    //To be used with deparment and companies
    public class NameView : AbstractSimpleFieldView
    {
        readonly Action<string> onNameChanged;

        public NameView(Context context, Action<string> onNameChanged)
            : base(context, Resource.String.edit_contact_name, false)
        {
            this.onNameChanged = onNameChanged;
        }

        public override void RefreshView()
        {
            Content = ContactPreview.Name;
            onNameChanged?.Invoke(Content);
        }

        protected override void ContentChanged(object sender, TextChangedEventArgs e)
        {
            ContactPreview.Name = Content;
            onNameChanged?.Invoke(Content);
        }
    }
}
