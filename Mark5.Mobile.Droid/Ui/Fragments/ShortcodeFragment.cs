//
// Project: Mark5.Mobile.Droid
// File: ShortcodeFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System;
using System.Threading.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.ShortcodeViews;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{

    public class ShortcodeFragment : RetainableStateFragment
    {

        public int? FolderId { get; set; }
        public Folder Folder { get; set; }
        public int SearchId { get; set; }
        public int? ShortcodeId { get; set; }
        public ShortcodePreview ShortcodePreview { get; set; }
        public Shortcode Shortcode { get; set; }
        public Action CloseRequest { get; set; }
        public bool ReadOnlyMode { get; set; }

        ProgressBar progress;
        ScrollView scrollView;
        LinearLayoutCompat linearLayout;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(ShortcodeFragment)} [folder.name={Folder?.Name}, searchId={SearchId}, folder.id={FolderId ?? Folder?.Id}, shortcodeId={ShortcodeId ?? ShortcodePreview?.Id}, readOnlyMode={ReadOnlyMode}...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout, container, false);
            rootView.SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.lightgray)));

            progress = rootView.FindViewById<ProgressBar>(Resource.Id.progress);
            scrollView = rootView.FindViewById<ScrollView>(Resource.Id.scroll_view);
            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            linearLayout.SetClipToPadding(false);
            var padding = ConversionUtils.ConvertDpToPixels(10.0f);
            linearLayout.SetPadding(padding, padding, padding, padding);

            linearLayout.AddView(new DescriptionView(Context));
            var avReplyTo = new AddressesView(Context, DocumentAddressType.ReplyTo);
            avReplyTo.DocumentAddressClicked += AddressesView_DocumentAddressClicked;
            linearLayout.AddView(avReplyTo);
            var avTo = new AddressesView(Context, DocumentAddressType.To);
            avTo.DocumentAddressClicked += AddressesView_DocumentAddressClicked;
            linearLayout.AddView(avTo);
            var avCc = new AddressesView(Context, DocumentAddressType.Cc);
            avCc.DocumentAddressClicked += AddressesView_DocumentAddressClicked;
            linearLayout.AddView(avCc);
            var avBcc = new AddressesView(Context, DocumentAddressType.Bcc);
            avBcc.DocumentAddressClicked += AddressesView_DocumentAddressClicked;
            linearLayout.AddView(avBcc);

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = string.Empty;

            CommonConfig.Logger.Info($"Created {nameof(ShortcodeFragment)} [folder.name={Folder?.Name}, searchId={SearchId}, folder.id={FolderId ?? Folder?.Id}, shortcodeId={ShortcodeId ?? ShortcodePreview?.Id}, readOnlyMode={ReadOnlyMode}]");
        }

        public override async void OnResume()
        {
            base.OnResume();

            await RefreshData();
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            if (ReadOnlyMode) return;

            menu.Add(Menu.None, 10, 10, Resource.String.create_new_document);
            menu.Add(Menu.None, 20, 20, Resource.String.copy_to_worktray);
            menu.Add(Menu.None, 30, 30, Resource.String.copy_to_folder);

            if (Folder.InternalType == FolderInternalType.FilterView
                || Folder.InternalType == FolderInternalType.Static
                || Folder.InternalType == FolderInternalType.Worktray)
            {
                menu.Add(Menu.None, 31, 31, Resource.String.move_to_folder);
            }

            menu.Add(Menu.None, 40, 40, Resource.String.actions);
            menu.Add(Menu.None, 50, 50, Resource.String.links);

            if (Folder.InternalType == FolderInternalType.FilterView
                || Folder.InternalType == FolderInternalType.Static
                || Folder.InternalType == FolderInternalType.Worktray)
            {
                menu.Add(Menu.None, 60, 60, Resource.String.delete_from_folder);
            }

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator
                || ServerConfig.SystemSettings.ShortcodesModuleInfo.Permissions.DeleteAllowed)
            {
                menu.Add(Menu.None, 61, 61, Resource.String.delete);
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            return base.OnOptionsItemSelected(item);
        }

        void AddressesView_DocumentAddressClicked(object sender, DocumentAddress e)
        {
            // TODO
        }

        async Task RefreshData()
        {
            try
            {
                if (Folder != null || FolderId.HasValue)
                {
                    if (ShortcodeId.HasValue && ShortcodePreview == null && Shortcode == null)
                    {
                        var container = await Managers.ShortcodesManager.GetShortcodeWithPreviewAsync(FolderId ?? Folder.Id, ShortcodeId.Value);
                        ShortcodePreview = container.ShortcodePreview;
                        Shortcode = container.Shortcode;
                    }

                    if (ShortcodePreview != null && Shortcode == null)
                    {
                        Shortcode = await Managers.ShortcodesManager.GetShortcodeAsync(FolderId ?? Folder.Id, ShortcodePreview.Id);
                    }
                }

                if (SearchId <= -999)
                {
                    if (ShortcodePreview != null && Shortcode == null)
                    {
                        Shortcode = await Managers.SearchManager.GetShortcodeAsync(SearchId, ShortcodePreview);
                    }
                }

                RefreshView();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Downloading shortcode failed [folder.name={Folder?.Name}, searchId={SearchId}, folder.id={FolderId ?? Folder?.Id}, shortcodeId={ShortcodeId ?? ShortcodePreview?.Id}, readOnlyMode={ReadOnlyMode}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);

                if (CloseRequest != null) CloseRequest();
            }
        }

        void RefreshView()
        {
            ((AppCompatActivity)Activity).SupportActionBar.Title = ShortcodePreview.Name;

            progress.Visibility = ViewStates.Gone;
            scrollView.Visibility = ViewStates.Visible;

            for (var i = 0; i < linearLayout.ChildCount; i++)
            {
                var dv = linearLayout.GetChildAt(i) as ShortcodeView;
                if (dv != null)
                {
                    dv.ShortcodePreview = ShortcodePreview;
                    dv.Shortcode = Shortcode;
                    dv.RefreshView();
                }
            }

            linearLayout.Invalidate();
            linearLayout.RequestLayout();
        }

        public override IRetainableState OnRetainInstanceState()
        {
            return new ShortcodeFragmentState
            {
                FolderId = FolderId,
                Folder = Folder,
                SearchId = SearchId,
                ShortcodeId = ShortcodeId,
                ShortcodePreview = ShortcodePreview,
                Shortcode = Shortcode,
                ReadOnlyMode = ReadOnlyMode
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var sfs = restoredState as ShortcodeFragmentState;
            if (sfs != null)
            {
                FolderId = sfs.FolderId;
                Folder = sfs.Folder;
                SearchId = sfs.SearchId;
                ShortcodeId = sfs.ShortcodeId;
                ShortcodePreview = sfs.ShortcodePreview;
                Shortcode = sfs.Shortcode;
                ReadOnlyMode = sfs.ReadOnlyMode;
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(ShortcodeFragment)} [ShortcodeId={ShortcodePreview?.Id ?? Shortcode.Id}]";
        }

        class ShortcodeFragmentState : IRetainableState
        {

            public int? FolderId { get; set; }

            public Folder Folder { get; set; }

            public int SearchId { get; set; }

            public int? ShortcodeId { get; set; }

            public ShortcodePreview ShortcodePreview { get; set; }

            public Shortcode Shortcode { get; set; }

            public bool ReadOnlyMode { get; set; }
        }
    }
}
