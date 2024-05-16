using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Core.Widget;
using Google.Android.Material.FloatingActionButton;
using reMark.Mobile.Classes.Enum;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Manager;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.Droid.Model.HubMessages;
using reMark.Mobile.Droid.Ui.Activities;
using reMark.Mobile.Droid.Ui.Common;
using reMark.Mobile.Droid.Ui.Views.AddEditShortcodeViews;
using reMark.Mobile.Droid.Utilities;
using ProgressBar = Android.Widget.ProgressBar;
using View = Android.Views.View;

namespace reMark.Mobile.Droid.Ui.Fragments
{
    public class AddEditShortcodeFragment : BaseFragment
    {
        const string ShortcodePreviewBundleKey = "ShortcodePreview_880203fa-26a5-4111-a85a-d907fa3fdbd1";
        const string ShortcodeCreationModeFlagBundleKey = "ShortcodeCreationModeFlag_0e4124b4-943c-4078-b88b-e4d15e9c0fde";

        static class RequestCodes
        {
            public const int ContactAddressRequestCode = 111;
        }

        Shortcode _shortcode;
        ShortcodePreview _shortcodePreview;
        ShortcodeCreationModeFlag _creationModeFlag;

        LinearLayoutCompat linearLayout;
        ProgressBar progressBar;
        NestedScrollView scrollView;
        FloatingActionButton fab;

        DocumentAddressType requestAddressType;

        NameView nameView;
        DescriptionView descriptionView;
        EntryView toView;
        EntryView ccView;
        EntryView bccView;

        List<AddEditShortcodeView> subviews = new();

        Action dismissAction;

        public static (AddEditShortcodeFragment fragment, string tag) NewInstance(ShortcodeCreationModeFlag? flag, 
            ShortcodePreview shortcodePreview)
        {
            if (flag == ShortcodeCreationModeFlag.Edit)
                CommonConfig.UsageAnalytics.LogEvent(new OpenEditShortcodeEvent());
            if (flag == ShortcodeCreationModeFlag.New)
                CommonConfig.UsageAnalytics.LogEvent(new OpenAddShortcodeEvent());

            Bundle args = new Bundle();

            if (shortcodePreview != null)
                args.PutString(ShortcodePreviewBundleKey, Serializer.Serialize(shortcodePreview));

            if (flag != null)
                args.PutInt(ShortcodeCreationModeFlagBundleKey, (int)flag);

            var fragment = new AddEditShortcodeFragment();
            fragment.Arguments = args;

            var tag = $"{nameof(AddEditShortcodeFragment)}";

            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Arguments != null && Arguments.ContainsKey(ShortcodeCreationModeFlagBundleKey))
                _creationModeFlag = (ShortcodeCreationModeFlag)Arguments.GetInt(ShortcodeCreationModeFlagBundleKey);

            if (savedInstanceState?.ContainsKey(ShortcodePreviewBundleKey) == true)
                _shortcodePreview = Serializer.Deserialize<ShortcodePreview>(savedInstanceState.GetString(ShortcodePreviewBundleKey));
            else if (Arguments != null && Arguments.ContainsKey(ShortcodePreviewBundleKey))
                _shortcodePreview = Serializer.Deserialize<ShortcodePreview>(Arguments.GetString(ShortcodePreviewBundleKey));
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(AddEditShortcodeFragment)} [shortcode.id={_shortcodePreview?.Id}, " +
                                     $" mode={_creationModeFlag}]...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_with_progress, container, false);

            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            linearLayout.DescendantFocusability = DescendantFocusability.BeforeDescendants;
            linearLayout.FocusableInTouchMode = true;

            scrollView = rootView.FindViewById<NestedScrollView>(Resource.Id.scroll_view);
            progressBar = rootView.FindViewById<ProgressBar>(Resource.Id.progress);

            fab = ((View)container.Parent.Parent).FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.SetImageResource(Resource.Drawable.action_save);
            fab.SetOnClickListener(new ActionOnClickListener(HandleSend));
            fab.Enabled = true;
            fab.Size = FloatingActionButton.SizeNormal;
            fab.Visibility = ViewStates.Visible;

            subviews.Clear();

            var bottomMargin = ((CoordinatorLayout.LayoutParams)fab.LayoutParameters).BottomMargin;
            var fabHeight = Conversion.ConvertDpToPixels(56);
            linearLayout.SetPadding(linearLayout.PaddingLeft, linearLayout.PaddingTop, linearLayout.PaddingRight, fabHeight + bottomMargin * 2);

            PrepareSubviews();

            SetTitle();

            return rootView;
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            if (_shortcodePreview != null)
                outState.PutString(ShortcodePreviewBundleKey, Serializer.Serialize(_shortcodePreview));
        }

        private void SetTitle()
        {
            var resId = 0;
            if (_creationModeFlag == ShortcodeCreationModeFlag.New)
            {
                resId = Resource.String.edit_shortcode_create;
            }
            else if (_creationModeFlag == ShortcodeCreationModeFlag.Edit)
            {
                resId = Resource.String.edit_shortcode_edit;
            }

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(resId);
        }

        void PrepareSubviews()
        {
            nameView = new NameView(Context);
            descriptionView = new DescriptionView(Context);
            toView = new EntryView(Context, DocumentAddressType.To, OnContactAddressRequest);
            ccView = new EntryView(Context, DocumentAddressType.Cc, OnContactAddressRequest);
            bccView = new EntryView(Context, DocumentAddressType.Bcc, OnContactAddressRequest);

            subviews.Add(nameView);
            subviews.Add(descriptionView);
            subviews.Add(toView);
            subviews.Add(ccView);
            subviews.Add(bccView);

            subviews.ForEach(linearLayout.AddView);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            CommonConfig.Logger.Info($"Created {nameof(AddEditShortcodeFragment)} [shortcode.id={_shortcodePreview?.Id}, " +
                                     $" mode={_creationModeFlag}]...");
        }

        public override async void OnResume()
        {
            base.OnResume();

            fab.Enabled = true;
            fab.Visibility = ViewStates.Visible;

            await RefreshData();
        }

        public override void OnDestroyView()
        {
            dismissAction?.Invoke();
            base.OnDestroyView();
        }

        public override void OnStop()
        {
            base.OnStop();
            fab.Visibility = ViewStates.Gone;
        }

        #region Refreh methods

        private async Task RefreshData()
        {
            if (_creationModeFlag == ShortcodeCreationModeFlag.New && _shortcodePreview == null)
            {
                _shortcode = new Shortcode();
                _shortcodePreview = new ShortcodePreview();
            }
            else
            {
                try
                {
                    if (_shortcodePreview.Id <= 0)
                        return;
                        
                    _shortcode = await Managers.ShortcodesManager.GetShortcodeAsync(-1, _shortcodePreview.Id, SourceType.Local);
                    RefreshView();
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error($"Retrieving shortcode failed [shortcodeId={_shortcodePreview?.Id}]", ex);

                    await Dialogs.ShowErrorDialogAsync(Activity, ex);

                    Activity?.OnBackPressed();
                }
            }

            RefreshView();
        }

        private void RefreshView()
        {
            progressBar.Visibility = ViewStates.Gone;
            scrollView.Visibility = ViewStates.Visible;

            foreach (var subview in subviews)
            {
                subview.Shortcode = _shortcode;
                subview.ShortcodePreview = _shortcodePreview;
                subview.CreationModeFlag = _creationModeFlag;
                subview.RefreshView();
            }
        }

        #endregion

        private async void HandleSend()
        {
            if (nameView != null && !nameView.ContainsValidContent())
            {
                nameView.ShowError();
                return;
            }

            var titleResource = _creationModeFlag == ShortcodeCreationModeFlag.Edit ? Resource.String.edit_shortcode_edit_loading : Resource.String.edit_shortcode_add_loading;
            dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, titleResource, Resource.String.please_wait);

            try
            {
                await Managers.ShortcodesManager.CreateOrUpdateShortcodeAsync(_shortcode, _shortcodePreview);

                dismissAction();

                if (_creationModeFlag == ShortcodeCreationModeFlag.Edit)
                    CommonConfig.MessengerHub.Publish(new ShortcodePreviewChangedMessage(this, _shortcodePreview));

                Activity?.OnBackPressed();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Error while adding/editing shortcode  [shortcode.id={_shortcodePreview?.Id}, " +
                                     $" mode={_creationModeFlag}]...", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        #region ContactAddress request

        private void OnContactAddressRequest(DocumentAddressType type)
        {
            requestAddressType = type;

            var i = new Intent(Activity, typeof(PickerContactFolderListActivity));
            StartActivityForResult(i, RequestCodes.ContactAddressRequestCode);
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if (requestCode != RequestCodes.ContactAddressRequestCode || resultCode != (int)Result.Ok)
                return;

            var recipient = Serializer.Deserialize<Recipient>(data.GetStringExtra(PickerContactFolderListActivity.RecipientResultKey));
            GetEntryViewPerType(requestAddressType).AddEntry(recipient);
        }

        EntryView GetEntryViewPerType(DocumentAddressType type)
        {
            switch (type)
            {
                case DocumentAddressType.To:
                    return toView;
                case DocumentAddressType.Cc:
                    return ccView;
                case DocumentAddressType.Bcc:
                    return bccView;
                default:
                    throw new ArgumentException("Invalid type");
            }
        }

        #endregion
    }
}
