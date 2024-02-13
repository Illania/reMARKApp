using Android.Content;
using reMark.Mobile.Common.Model;

namespace reMark.Mobile.Droid.Ui.Views.AddEditContactViews
{
    public class FaxView : AbstractPhoneNumberView
    {
        public FaxView(Context context)
            : base(context, CommunicationAddressType.Fax)
        {
        }
    }
}
