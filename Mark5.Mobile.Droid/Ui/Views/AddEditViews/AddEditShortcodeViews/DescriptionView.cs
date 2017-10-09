using Android.Content;
using Android.Text;

namespace Mark5.Mobile.Droid.Ui.Views.AddEditShortcodeViews
{
    public class DescriptionView : AbstractSimpleFieldView
    {
        public DescriptionView(Context context)
            : base(context, Resource.String.edit_shortcode_description, true, true, -1,
                   InputTypes.TextFlagMultiLine
                   | InputTypes.TextFlagCapSentences
                   | InputTypes.TextFlagAutoCorrect | InputTypes.ClassText)
        {
        }

        public override void RefreshView()
        {
            Content = ShortcodePreview.Description;
        }

        protected override void ContentChanged(object sender, TextChangedEventArgs e)
        {
            ShortcodePreview.Description = Content;
        }
    }
}
