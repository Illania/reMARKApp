using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

namespace Mark5.Mobile.Droid
{
    public class FoldersListInternalFragment : Fragment
    {
        public int Val
        {
            get;
            set;
        }

        public static FoldersListInternalFragment Create(int val)
        {
            var fragment = new FoldersListInternalFragment();
            var args = new Bundle();
            args.PutInt("val", val);
            fragment.Arguments = args;
            return fragment;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Arguments != null)
            {
                Val = Arguments.GetInt("val");
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.fragment_list_folders_internal, container, false);
        }

        public override void OnStart()
        {
            base.OnStart();

            var text = View.FindViewById<TextView>(Resource.Id.textView1);
            text.Text = Val.ToString();
        }
    }
}

