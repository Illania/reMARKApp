using System;
using System.Threading.Tasks;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.RecurrenceViews;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments.Calendar
{
    public class ReoccurrenceFragment : BaseFragment
    {
        const string RecurrenceKey = "RecurrenceKey";

        public Task<RecurrenceInfo> Task => tcs.Task;

        readonly TaskCompletionSource<RecurrenceInfo> tcs = new TaskCompletionSource<RecurrenceInfo>();

        RecurrenceInfo recInfo;

        PatternView patternView;
        RangeView rangeView;

        public static (ReoccurrenceFragment fragment, string tag) NewInstance(RecurrenceInfo rec)
        {
            var fragment = new ReoccurrenceFragment();
            var tag = $"{nameof(ReoccurrenceFragment)}";

            var args = new Bundle();
            if (rec != null)
                args.PutString(RecurrenceKey, Serializer.Serialize(rec));

            fragment.Arguments = args;
            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Arguments.ContainsKey(RecurrenceKey))
                recInfo = Serializer.Deserialize<RecurrenceInfo>(Arguments.GetString(RecurrenceKey));

            recInfo = recInfo ?? new RecurrenceInfo { Type = RecurrenceType.Daily };
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(ReoccurrenceFragment)}");
            var rootView = inflater.Inflate(Resource.Layout.linear_layout_base, container, false);

            var linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            linearLayout.LayoutTransition = new Android.Animation.LayoutTransition();

            var padding = Conversion.ConvertDpToPixels(10f);
            linearLayout.SetPadding(padding, padding, padding, padding);

            patternView = new PatternView(Context);
            rangeView = new RangeView(Context);

            linearLayout.AddView(patternView);
            linearLayout.AddView(rangeView);

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnResume()
        {
            base.OnResume();

            patternView.SetViewModel(recInfo);
            rangeView.SetViewModel(recInfo);

            patternView.Refresh();
            rangeView.Refresh();
        }

        public override void OnStop()
        {
            base.OnStop();

            tcs.TrySetResult(null);
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            menu.Clear();
            var item = menu.Add(Menu.None, 10, 10, Resource.String.done);
            item.SetShowAsAction(ShowAsAction.Always);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == 10)
            {
                CloseFragment();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        void CloseFragment()
        {
            tcs.SetResult(recInfo);
            ((AppCompatActivity)Activity).OnBackPressed();
        }
    }
}
