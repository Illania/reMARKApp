using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.OS;
using Android.Support.V7.App;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class ResponsibleSelectionFragment : AbstractUserSelectionFragment
    {
        public Task<Dictionary<int, string>> Task => tcs.Task;

        TaskCompletionSource<Dictionary<int, string>> tcs = new TaskCompletionSource<Dictionary<int, string>>();

        public static (ResponsibleSelectionFragment fragment, string tag) NewInstance(List<int> preselectedUserIds)
        {
            var args = new Bundle();

            if (preselectedUserIds != null)
                args.PutString(PreselectedUserIdsBundleKey, Serializer.Serialize(preselectedUserIds));

            args.PutInt(ActionButtonTextResIdBundleKey, Resource.String.confirm);
            args.PutBoolean(IncludeCurrentUserBundleKey, true);
            args.PutBoolean(AllowNoUserSelectedBundleKey, true);

            ResponsibleSelectionFragment fragment = new ResponsibleSelectionFragment();
            fragment.Arguments = args;

            var tag = $"{nameof(ResponsibleSelectionFragment)}";

            return (fragment, tag);
        }

        protected override void ActionButton_Click(object sender, EventArgs e)
        {
            var responsibleDict = new Dictionary<int, string>();
            foreach (var kvp in SelectedSystemUsers)
            {
                responsibleDict.Add(kvp.Key, kvp.Value.Username);
            }
            tcs.SetResult(responsibleDict);
            ((AppCompatActivity)Activity).OnBackPressed();
        }

        protected override string GetInfo()
        {
            return $"[preselectedEntitites.Count ={preselectedUserIds?.Count}]";
        }
    }
}
