using Android.Content;
using Android.Support.Design.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class ShortcodesListFragment : AbstractShortcodesListFragment
    {
        FloatingActionButton fab;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Android.OS.Bundle savedInstanceState)
        {
            if (ServerConfig.SystemSettings.ShortcodesModuleInfo.Permissions.CreateAllowed)
            {
                fab = ((View)container.Parent.Parent).FindViewById<FloatingActionButton>(Resource.Id.fab);
                fab.SetImageResource(Resource.Drawable.action_add_contact);
                fab.SetOnClickListener(new ActionOnClickListener(CreateShortcode));
                fab.Visibility = ViewStates.Visible;
            }

            return base.OnCreateView(inflater, container, savedInstanceState);
        }

        void CreateShortcode()
        {
            var intent = new Intent(Context, typeof(AddEditShortcodeActivity));
            intent.PutExtra(AddEditShortcodeActivity.ShortcodeCreationModeFlagIntentKey, (int)ShortcodeCreationModeFlag.New);
            StartActivity(intent);
        }

        #region Adapter callbacks

        protected override void Adapter_ItemClicked(object sender, ShortcodePreview shortcodePreview)
        {
            if (ActionMode == null)
            {
                var i = new Intent(Activity, typeof(ShortcodeActivity));
                i.PutExtra(ShortcodeActivity.FolderIntentKey, Serializer.Serialize(Folder));
                i.PutExtra(ShortcodeActivity.ShortcodePreviewIntentKey, Serializer.Serialize(shortcodePreview));
                StartActivity(i);
            }
            else
            {
                CurrentAdapter.SetSelected(shortcodePreview, !CurrentAdapter.IsSelected(shortcodePreview));

                if (CurrentAdapter.SelectedItemCount < 1)
                {
                    ActionMode.Finish();
                }
                else
                {
                    ActionMode.Title = CurrentAdapter.SelectedItemCount.ToString();
                    ActionMode.Invalidate();
                }
            }
        }

        protected override void Adapter_ItemLongClicked(object sender, ShortcodePreview shortcodePreview)
        {
            if (ActionMode == null)
                ActionMode = Activity.StartActionMode(this);

            Adapter_ItemClicked(sender, shortcodePreview);
        }

        #endregion
    }
}
