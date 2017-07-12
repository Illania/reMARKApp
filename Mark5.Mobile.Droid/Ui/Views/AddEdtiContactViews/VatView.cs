using Android.Content;
using Android.Text;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class VatView : AbstractSimpleFieldView
    {
        public VatView(Context context)
            : base(context, Resource.String.edit_contact_vat, true)
        {
        }

        public override bool ContainsValidContent() => true;

        public override void RefreshView()
        {
            Content = Contact.Vat;
        }

        protected override void ContentChanged(object sender, TextChangedEventArgs e)
        {
            Contact.Vat = Content;
        }
    }
}