using Android.Content;
using reMark.Mobile.Common.Model;

namespace reMark.Mobile.Droid.Ui.Views.AddEditContactViews
{
    public class TelexView : AbstractPhoneNumberView
    {
        public TelexView(Context context)
            : base(context, CommunicationAddressType.Telex)
        {
        }
    }
}