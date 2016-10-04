//
// Project: Mark5.Mobile.Droid
// File: ShortcodeFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.Common;
using Mark5.Mobile.Droid.Ui.Views.ShortcodeViews;

namespace Mark5.Mobile.Droid.Ui.Fragments
{

    public class ShortcodeFragment : RetainableStateFragment
    {

        public int? FolderId { get; set; }

        public Folder Folder { get; set; }

        public int? ShortcodeId { get; set; }

        public ShortcodePreview ShortcodePreview { get; set; }

        public Shortcode Shortcode { get; set; }

        public Action CloseRequest { get; set; }

        ProgressBar progress;
        ScrollView scrollView;
        LinearLayoutCompat linearLayout;

        CancellationTokenSource setReadStatusCancellationTokenSource;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(ShortcodeFragment)} [folder.id={FolderId ?? Folder?.Id}, shortcode.id={ShortcodeId ?? ShortcodePreview?.Id ?? Shortcode?.Id}...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout, container, false);

            progress = rootView.FindViewById<ProgressBar>(Resource.Id.progress);
            scrollView = rootView.FindViewById<ScrollView>(Resource.Id.scroll_view);
            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);

            linearLayout.AddView(new DescriptionView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new AddressesView(Context));

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = string.Empty;

            CommonConfig.Logger.Info($"Created {nameof(ShortcodeFragment)} [folder.id={FolderId ?? Folder?.Id}, shortcode.id={ShortcodeId ?? ShortcodePreview?.Id ?? Shortcode?.Id}]");
        }

        public override async void OnResume()
        {
            base.OnResume();

            await RefreshData();
        }

        public override void OnDestroyedByUser()
        {
            base.OnDestroyedByUser();

            setReadStatusCancellationTokenSource.Cancel();
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
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

        async Task RefreshData()
        {
            try
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

                RefreshView();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Downloading shortcode failed [folder.name={Folder?.Name}, folder.id={FolderId ?? Folder?.Id}, shortcodeId={ShortcodeId ?? ShortcodePreview?.Id}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);

                if (CloseRequest != null) CloseRequest();
            }
        }

        void RefreshView()
        {
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

                    var d = linearLayout.GetChildAt(i + 1) as Divider;
                    if (d != null)
                    {
                        d.Visibility = dv.Visibility;
                        i++;
                    }
                }
            }

            linearLayout.Invalidate();
            linearLayout.RequestLayout();
        }

        public override IRetainableState OnRetainInstanceState()
        {
            return new ShortcodeFragmentState
            {
                Folder = Folder,
                ShortcodeId = ShortcodeId,
                ShortcodePreview = ShortcodePreview,
                Shortcode = Shortcode
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var dfs = restoredState as ShortcodeFragmentState;
            if (dfs != null)
            {
                Folder = dfs.Folder;
                ShortcodeId = dfs.ShortcodeId;
                ShortcodePreview = dfs.ShortcodePreview;
                Shortcode = dfs.Shortcode;
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(ShortcodeFragment)} [ShortcodeId={ShortcodePreview?.Id ?? Shortcode.Id}]";
        }

        class ShortcodeFragmentState : IRetainableState
        {

            public Folder Folder { get; set; }

            public int? ShortcodeId { get; set; }

            public ShortcodePreview ShortcodePreview { get; set; }

            public Shortcode Shortcode { get; set; }
        }
    }
}
