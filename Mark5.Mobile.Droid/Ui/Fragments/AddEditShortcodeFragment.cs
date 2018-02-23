using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Model.HubMessages;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.AddEditShortcodeViews;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class AddEditShortcodeFragment : BaseFragment
    {
        const string ShortcodeBundleKey = "Shortcode_abf63445-9b80-4ddf-8af6-f1d0d8ea2044";
        const string ShortcodePreviewBundleKey = "ShortcodePreview_880203fa-26a5-4111-a85a-d907fa3fdbd1";
        const string ShortcodeCreationModeFlagBundleKey = "ShortcodeCreationModeFlag_0e4124b4-943c-4078-b88b-e4d15e9c0fde";

        static class RequestCodes
        {
            public const int ContactAddressRequestCode = 111;
        }

        Shortcode shortcode;
        ShortcodePreview shortcodePreview;
        ShortcodeCreationModeFlag creationModeFlag;

        LinearLayoutCompat linearLayout;
        ProgressBar progressBar;
        ScrollView scrollView;
        FloatingActionButton fab;

        DocumentAddressType requestAddressType;

        NameView nameView;
        DescriptionView descriptionView;
        EntryView toView;
        EntryView ccView;
        EntryView bccView;

        List<AddEditShortcodeView> subviews = new List<AddEditShortcodeView>();

        public static (AddEditShortcodeFragment fragment, string tag) NewInstance(ShortcodeCreationModeFlag? flag, Shortcode shortcode, ShortcodePreview shortcodePreview)
        {
            if (flag == ShortcodeCreationModeFlag.Edit)
                CommonConfig.UsageAnalytics.LogEvent(new OpenEditShortcodeEvent());
            if (flag == ShortcodeCreationModeFlag.New)
                CommonConfig.UsageAnalytics.LogEvent(new OpenAddShortcodeEvent());

            Bundle args = new Bundle();

            if (shortcode != null)
                args.PutString(ShortcodeBundleKey, Serializer.Serialize(shortcode));

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

            if (Arguments.ContainsKey(ShortcodeCreationModeFlagBundleKey))
                creationModeFlag = (ShortcodeCreationModeFlag)Arguments.GetInt(ShortcodeCreationModeFlagBundleKey);

            if (savedInstanceState?.ContainsKey(ShortcodeBundleKey) == true)
                shortcode = Serializer.Deserialize<Shortcode>(savedInstanceState.GetString(ShortcodeBundleKey));
            else if (Arguments.ContainsKey(ShortcodeBundleKey))
                shortcode = Serializer.Deserialize<Shortcode>(Arguments.GetString(ShortcodeBundleKey));

            if (savedInstanceState?.ContainsKey(ShortcodePreviewBundleKey) == true)
                shortcodePreview = Serializer.Deserialize<ShortcodePreview>(savedInstanceState.GetString(ShortcodePreviewBundleKey));
            else if (Arguments.ContainsKey(ShortcodePreviewBundleKey))
                shortcodePreview = Serializer.Deserialize<ShortcodePreview>(Arguments.GetString(ShortcodePreviewBundleKey));
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(AddEditShortcodeFragment)} [shortcode.id={shortcodePreview?.Id}, " +
                                     $" mode={creationModeFlag}]...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_with_progress, container, false);

            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            linearLayout.DescendantFocusability = DescendantFocusability.BeforeDescendants;
            linearLayout.FocusableInTouchMode = true;

            scrollView = rootView.FindViewById<ScrollView>(Resource.Id.scroll_view);
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

            if (shortcode != null)
                outState.PutString(ShortcodeBundleKey, Serializer.Serialize(shortcode));

            if (shortcodePreview != null)
                outState.PutString(ShortcodePreviewBundleKey, Serializer.Serialize(shortcodePreview));
        }

        void SetTitle()
        {
            var resId = 0;
            if (creationModeFlag == ShortcodeCreationModeFlag.New)
            {
                resId = Resource.String.edit_shortcode_create;
            }
            else if (creationModeFlag == ShortcodeCreationModeFlag.Edit)
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

            CommonConfig.Logger.Info($"Created {nameof(AddEditShortcodeFragment)} [shortcode.id={shortcodePreview?.Id}, " +
                                     $" mode={creationModeFlag}]...");
        }

        public override void OnResume()
        {
            base.OnResume();

            fab.Enabled = true;
            fab.Visibility = ViewStates.Visible;

            RefreshData();
        }

        public override void OnStop()
        {
            base.OnStop();
            fab.Visibility = ViewStates.Gone;
        }

        #region Refreh methods

        void RefreshData()
        {
            if (creationModeFlag == ShortcodeCreationModeFlag.New && shortcode == null && shortcodePreview == null)
            {
                shortcode = new Shortcode();
                shortcodePreview = new ShortcodePreview();
            }

            RefreshView();
        }

        void RefreshView()
        {
            progressBar.Visibility = ViewStates.Gone;
            scrollView.Visibility = ViewStates.Visible;

            foreach (var subview in subviews)
            {
                subview.Shortcode = shortcode;
                subview.ShortcodePreview = shortcodePreview;
                subview.CreationModeFlag = creationModeFlag;
                subview.RefreshView();
            }
        }

        #endregion

        async void HandleSend()
        {
            if (nameView != null && !nameView.ContainsValidContent())
            {
                nameView.ShowError();
                return;
            }

            var titleResource = creationModeFlag == ShortcodeCreationModeFlag.Edit ? Resource.String.edit_shortcode_edit_loading : Resource.String.edit_shortcode_add_loading;
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, titleResource, Resource.String.please_wait);

            try
            {
                await Managers.ShortcodesManager.CreateOrUpdateShortcodeAsync(shortcode, shortcodePreview);

                dismissAction();

                if (creationModeFlag == ShortcodeCreationModeFlag.Edit)
                    CommonConfig.MessengerHub.Publish(new ShortcodePreviewChangedMessage(this, shortcodePreview));

                Activity?.OnBackPressed();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Error while adding/editing shortcode  [shortcode.id={shortcodePreview?.Id}, " +
                                     $" mode={creationModeFlag}]...", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        #region ContactAddress request

        void OnContactAddressRequest(DocumentAddressType type)
        {
            requestAddressType = type;

            var i = new Intent(Activity, typeof(PickerContactFolderListActivity));
            StartActivityForResult(i, RequestCodes.ContactAddressRequestCode);
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if (requestCode == RequestCodes.ContactAddressRequestCode && resultCode == (int)Result.Ok)
            {
                var recipient = Serializer.Deserialize<Recipient>(data.GetStringExtra(PickerContactFolderListActivity.RecipientResultKey));
                GetEntryViewPerType(requestAddressType).AddEntry(recipient);
            }
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
