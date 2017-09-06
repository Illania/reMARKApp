using System;
using Android.Content;
using Android.Text;

namespace Mark5.Mobile.Droid.Ui.Views.AddEditShortcodeViews
{
    public class NameView : AbstractSimpleFieldView
    {
        public NameView(Context context)
            : base(context, Resource.String.edit_shortcode_name, false, true, -1,
                   InputTypes.ClassText | InputTypes.TextFlagCapSentences | InputTypes.TextFlagNoSuggestions)
        {
        }

        public override void RefreshView()
        {
            Content = ShortcodePreview.Name;
        }

        protected override void ContentChanged(object sender, TextChangedEventArgs e)
        {
            ShortcodePreview.Name = Content;
        }
    }
}
