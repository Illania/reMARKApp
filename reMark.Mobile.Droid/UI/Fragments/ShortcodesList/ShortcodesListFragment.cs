using Android.Content;
using Android.OS;
using Android.Views;
using Google.Android.Material.FloatingActionButton;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.Droid.Ui.Activities;
using reMark.Mobile.Droid.Ui.Common;
using View = Android.Views.View;

namespace reMark.Mobile.Droid.Ui.Fragments
{
    public class ShortcodesListFragment : AbstractShortcodesListFragment
    {
        public static (ShortcodesListFragment fragment, string tag) NewInstance(Folder folder)
        {
            var args = new Bundle();

            if (folder != null)
                args.PutString(FolderBundleKey, Serializer.Serialize(folder));

            var fragment = new ShortcodesListFragment();
            fragment.Arguments = args;

            var tag = $"{nameof(AbstractShortcodesListFragment)} [folder.id={folder.Id}, folder.name={folder.Name}]";

            return (fragment, tag);
        }

        FloatingActionButton fab;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Android.OS.Bundle savedInstanceState)
        {
            if (ServerConfig.SystemSettings.ShortcodesModuleInfo.Permissions.CreateAllowed)
            {
                fab = ((View)container.Parent.Parent).FindViewById<FloatingActionButton>(Resource.Id.fab);
                fab.SetImageResource(Resource.Drawable.action_add);
                fab.SetOnClickListener(new ActionOnClickListener(CreateShortcode));
                fab.Visibility = ViewStates.Visible;
            }

            return base.OnCreateView(inflater, container, savedInstanceState);
        }

        void CreateShortcode()
        {
            StartActivity(AddEditShortcodeActivity.CreateIntent(Context, ShortcodeCreationModeFlag.New));
        }

        #region Adapter callbacks

        protected override void Adapter_ItemClicked(object sender, ShortcodePreview shortcodePreview)
        {
            if (ActionMode == null)
            {
                StartActivity(ShortcodeActivity.CreateIntent(Context, folder: Folder, shortcodePreview: shortcodePreview));
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
