using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class PhoneView : AbstractPhoneNumberView
    {
        public PhoneView(Context context)
            : base(context, CommunicationAddressType.Phone)
        {
        }
    }
}
