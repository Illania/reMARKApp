using Android.Content;
using Android.Text;

namespace Mark5.Mobile.Droid.Ui.Views.AddEditContactViews
{
    public class WebpageView : AbstractSimpleFieldView
    {
        public WebpageView(Context context)
            : base(context, Resource.String.edit_contact_webpage, true,
                   inputType: InputTypes.TextFlagNoSuggestions
                   | InputTypes.TextFlagCapSentences
                   | InputTypes.ClassText
                   | InputTypes.TextVariationUri)
        {
        }

        public override void RefreshView()
        {
            Content = Contact.WebPageAddress;
        }

        protected override void ContentChanged(object sender, TextChangedEventArgs e)
        {
            Contact.WebPageAddress = Content;
        }

    }
}