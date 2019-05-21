using System;
using Android.OS;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments.Calendar
{
    public class CreateAppointmentFragment : BaseFragment
    {
        public static (CreateAppointmentFragment fragment, string tag) NewInstance()
        {
            var args = new Bundle();

            var fragment = new CreateAppointmentFragment
            {
                Arguments = args
            };

            var tag = $"{nameof(CreateAppointmentFragment)}";

            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return new FrameLayout(Context);
        }
    }
}
