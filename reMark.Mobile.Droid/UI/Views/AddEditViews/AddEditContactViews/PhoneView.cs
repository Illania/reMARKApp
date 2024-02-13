using Android.Content;
using reMark.Mobile.Common.Model;

namespace reMark.Mobile.Droid.Ui.Views.AddEditContactViews
{
    public class PhoneView : AbstractPhoneNumberView
    {
        public PhoneView(Context context)
            : base(context, CommunicationAddressType.Phone)
        {
        }
    }
}
