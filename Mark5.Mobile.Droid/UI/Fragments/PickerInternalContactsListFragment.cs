using System;
using System.Collections.Generic;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class PickerInternalContactsListFragment : AbstractUserSelectionFragment
    {
        public static (PickerInternalContactsListFragment fragment, string tag) NewInstance()
        {
            var args = new Bundle();

            args.PutInt(ActionButtonTextResIdBundleKey, Resource.String.confirm);
            args.PutBoolean(IncludeCurrentUserBundleKey, false);
            args.PutBoolean(AllowNoUserSelectedBundleKey, true);

            PickerInternalContactsListFragment fragment = new PickerInternalContactsListFragment();
            fragment.Arguments = args;

            var tag = $"{nameof(PickerInternalContactsListFragment)}";

            return (fragment, tag);
        }

        protected override void ActionButton_Click(object sender, EventArgs e)
        {
            var userList = new List<SystemUser>();
            foreach (var kvp in SelectedSystemUsers)
            {
                userList.Add(kvp.Value);
            }

            var intent = new Intent();
            intent.PutExtra(PickerInternalContactsListActivity.RecipientResultKey, Serializer.Serialize(userList));
            ((AppCompatActivity)Activity).SetResult(Android.App.Result.Ok, intent);
            ((AppCompatActivity)Activity).Finish();
        }

        protected override string GetInfo()
        {
            return nameof(PickerInternalContactsListFragment);
        }
    }
}