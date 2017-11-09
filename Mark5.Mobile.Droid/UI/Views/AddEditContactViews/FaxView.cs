using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.AddEditContactViews
{
    public class FaxView : AbstractPhoneNumberView
    {
        public FaxView(Context context)
            : base(context, CommunicationAddressType.Fax)
        {
        }
    }
}
