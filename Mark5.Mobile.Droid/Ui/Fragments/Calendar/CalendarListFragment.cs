using Android.OS;
using Android.Views;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments.Calendar
{
	public class CalendarListFragment : BaseFragment
	{
		public static (CalendarListFragment fragment, string tag) NewInstance()
		{
			var args = new Bundle();

			var fragment = new CalendarListFragment
			{
				Arguments = args
			};

			var tag = $"{nameof(CalendarListFragment)}";

			return (fragment, tag);
		}

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			HasOptionsMenu = true;
			var rootView = inflater.Inflate(Resource.Layout.list, container, false);
			return rootView;
		}
	}
}