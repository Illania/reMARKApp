using System;
using Android.Content;
using Android.Text;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class ShortIdView : AbstractSimpleFieldView
    {
        public ShortIdView(Context context)
            : base(context, Resource.String.edit_contact_short_id, true
                   , inputType: InputTypes.TextFlagNoSuggestions
                   | InputTypes.ClassText)
        {
        }

        public override bool ContainsValidContent() => true;

        public override void RefreshView()
        {
            Content = ContactPreview.ShortId;
        }

        protected override void ContentChanged(object sender, TextChangedEventArgs e)
        {
            ContactPreview.ShortId = Content;
        }
    }
}