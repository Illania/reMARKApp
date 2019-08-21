using System;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments.Calendar.Reoccurrence
{
    public class ReoccurrenceFragment : BaseFragment
    {
        const string RecurrenceKey = "RecurrenceKey";

        RecurrenceInfo recInfo;

        PatternView patternView;
        RangeView rangeView;

        public static (ReoccurrenceFragment fragment, string tag) NewInstance(RecurrenceInfo rec)
        {
            var fragment = new ReoccurrenceFragment();
            var tag = $"{nameof(ReoccurrenceFragment)}";

            var args = new Bundle();
            args.PutString(RecurrenceKey, Serializer.Serialize(rec));

            fragment.Arguments = args;
            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Arguments.ContainsKey(RecurrenceKey))
                recInfo = Serializer.Deserialize<RecurrenceInfo>(Arguments.GetString(RecurrenceKey));
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(ReoccurrenceFragment)}");
            var rootView = inflater.Inflate(Resource.Layout.linear_layout_base, container, false);

            var linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);

            patternView = new PatternView(Context);
            rangeView = new RangeView(Context);

            linearLayout.AddView(patternView);
            linearLayout.AddView(rangeView);

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


    }
}
