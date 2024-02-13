using Android.Content;
using Android.Text;
using TextChangedEventArgs = Android.Text.TextChangedEventArgs;

namespace reMark.Mobile.Droid.Ui.Views.AddEditContactViews
{
    public class PositionView : AbstractSimpleFieldView
    {
        public PositionView(Context context)
            : base(context, Resource.String.edit_contact_position, true)
        {
        }

        public override void RefreshView()
        {
            Content = Contact.Position;
        }

        protected override void ContentChanged(object sender, TextChangedEventArgs e)
        {
            Contact.Position = Content;
        }
    }
}