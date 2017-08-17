using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class TelexView : AbstractPhoneNumberView
    {
        public TelexView(Context context)
            : base(context, CommunicationAddressType.Telex)
        {
        }
    }
}