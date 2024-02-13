using Android.Content;
using reMark.Mobile.Common.Model;

namespace reMark.Mobile.Droid.Ui.Views.AddEditContactViews
{
    public class MobileView : AbstractPhoneNumberView
    {
        public MobileView(Context context)
            : base(context, CommunicationAddressType.Mobile)
        {
        }
    }
}
