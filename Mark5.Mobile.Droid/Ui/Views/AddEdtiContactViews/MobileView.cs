using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class MobileView : AbstractPhoneNumberView
    {
        public MobileView(Context context)
            : base(context, CommunicationAddressType.Mobile)
        {
        }
    }
}
