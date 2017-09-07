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
using Mark5.Mobile.Droid.Ui.Common.HubMessages;
using Mark5.Mobile.Droid.Ui.Views.AddEditShortcodeViews;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class AddEditShortcodeFragment : RetainableStateFragment
    {
        static class RequestCodes
        {
            public const int ContactAddressRequestCode = 111;
        }

        public Shortcode Shortcode { get; set; }
        public ShortcodePreview ShortcodePreview { get; set; }
        public ShortcodeCreationModeFlag CreationModeFlag { get; set; }
        public Action CloseRequest { get; set; }

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

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(AddEditShortcodeFragment)} [shortcode.id={ShortcodePreview?.Id}, " +
                                     $" mode={CreationModeFlag}]...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_with_progress, container, false);

            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            linearLayout.DescendantFocusability = DescendantFocusability.BeforeDescendants;
            linearLayout.FocusableInTouchMode = true;

            scrollView = rootView.FindViewById<ScrollView>(Resource.Id.scroll_view);
            progressBar = rootView.FindViewById<ProgressBar>(Resource.Id.progress);

            fab = ((View)container.Parent.Parent).FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.SetImageResource(Resource.Drawable.action_save_contact); //TODO need to change the name for the icon
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

        void SetTitle()
        {
            var resId = 0;
            if (CreationModeFlag == ShortcodeCreationModeFlag.New)
            {
                resId = Resource.String.edit_shortcode_create;
            }
            else if (CreationModeFlag == ShortcodeCreationModeFlag.Edit)
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

            CommonConfig.Logger.Info($"Created {nameof(AddEditShortcodeFragment)} [shortcode.id={ShortcodePreview?.Id}, " +
                                     $" mode={CreationModeFlag}]...");
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
            if (CreationModeFlag == ShortcodeCreationModeFlag.New)
            {
                Shortcode = new Shortcode();
                ShortcodePreview = new ShortcodePreview();
            }

            RefreshView();
        }

        void RefreshView()
        {
            progressBar.Visibility = ViewStates.Gone;
            scrollView.Visibility = ViewStates.Visible;

            foreach (var subview in subviews)
            {
                subview.Shortcode = Shortcode;
                subview.ShortcodePreview = ShortcodePreview;
                subview.CreationModeFlag = CreationModeFlag;
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

            var titleResource = CreationModeFlag == ShortcodeCreationModeFlag.Edit ? Resource.String.edit_shortcode_edit_loading : Resource.String.edit_shortcode_add_loading;
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, titleResource, Resource.String.please_wait);

            try
            {
                await Managers.ShortcodesManager.CreateOrUpdateShortcodeAsync(Shortcode, ShortcodePreview);

                dismissAction();

                if (CreationModeFlag == ShortcodeCreationModeFlag.Edit)
                    CommonConfig.MessengerHub.Publish(new ShortcodePreviewChangedMessage(this, ShortcodePreview));

                CloseRequest?.Invoke();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Error while adding/editing contact  [shortcode.id={ShortcodePreview?.Id}, " +
                                     $" mode={CreationModeFlag}]...", ex);

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

        #region Retainable State

        public override string GenerateTag()
        {
            return $"{nameof(AddEditShortcodeFragment)}";
        }

        public override IRetainableState OnRetainInstanceState()
        {
            return new AddEditShortcodeFragmentState
            {
                Shortcode = Shortcode,
                ShortcodePreview = ShortcodePreview,
                CreationModeFlag = CreationModeFlag,
                RequestAddressType = requestAddressType,
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            if (restoredState is AddEditShortcodeFragmentState state)
            {
                Shortcode = state.Shortcode;
                ShortcodePreview = state.ShortcodePreview;
                CreationModeFlag = state.CreationModeFlag;
                requestAddressType = state.RequestAddressType;
            }
        }

        class AddEditShortcodeFragmentState : IRetainableState
        {
            public Shortcode Shortcode { get; set; }
            public ShortcodePreview ShortcodePreview { get; set; }
            public DocumentAddressType RequestAddressType { get; set; }
            public ShortcodeCreationModeFlag CreationModeFlag { get; set; }
        }

        #endregion

    }
}
