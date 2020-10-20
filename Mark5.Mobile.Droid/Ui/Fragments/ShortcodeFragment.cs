using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.ShortcodeViews;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class ShortcodeFragment : BaseFragment
    {
        const string FolderIdBundleKey = "FolderId_20cd4171-75b6-4c2a-b82e-73b4491da5da";
        const string FolderBundleKey = "Folder_3c6faf65-a2e2-498f-8188-c731b373adb3";
        const string ShortcodeIdBundleKey = "ShortcodeId_07a8c9bf-3430-46fa-9f28-c66d68a11df7";
        const string ShortcodePreviewBundleKey = "ShortcodePreview_0e12da2b-686c-48c9-8292-b033fb0ab556";
        const string NotificationGuidBundleKey = "NotificationBundle_d137bb3f-17a4-4a2c-9076-cf0704236d14";

        static class RequestCodes
        {
            public const int EditRequest = 1;
        }


        int? folderId;
        Folder folder;
        int? shortcodeId;
        ShortcodePreview shortcodePreview;
        Shortcode shortcode;
        Guid NotificationGuid;
        bool forceRefresh;

        ProgressBar progress;
        NestedScrollView scrollView;
        LinearLayoutCompat linearLayout;

        Action dismissAction;

        public static (ShortcodeFragment fragment, string tag) NewInstance(int? folderId, Folder folder, int? shortcodeId, ShortcodePreview shortcodePreview, Guid? notificationGuid)
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenShortcodeEvent());

            var args = new Bundle();

            if (folderId != null)
                args.PutInt(FolderIdBundleKey, folderId.Value);

            if (folder != null)
                args.PutString(FolderBundleKey, Serializer.Serialize(folder));

            if (shortcodeId != null)
                args.PutInt(ShortcodeIdBundleKey, shortcodeId.Value);

            if (shortcodePreview != null)
                args.PutString(ShortcodePreviewBundleKey, Serializer.Serialize(shortcodePreview));

            if (notificationGuid != null)
                args.PutString(NotificationGuidBundleKey, Serializer.Serialize(notificationGuid));

            var fragment = new ShortcodeFragment();
            fragment.Arguments = args;

            var tag = $"{nameof(ShortcodeFragment)} [ShortcodeId={shortcodePreview?.Id ?? shortcodeId}]";

            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Arguments.ContainsKey(FolderIdBundleKey))
                folderId = Arguments.GetInt(FolderIdBundleKey);

            if (Arguments.ContainsKey(FolderBundleKey))
                folder = Serializer.Deserialize<Folder>(Arguments.GetString(FolderBundleKey));

            if (Arguments.ContainsKey(ShortcodeIdBundleKey))
                shortcodeId = Arguments.GetInt(ShortcodeIdBundleKey);

            if (Arguments.ContainsKey(ShortcodePreviewBundleKey))
                shortcodePreview = Serializer.Deserialize<ShortcodePreview>(Arguments.GetString(ShortcodePreviewBundleKey));

            if (Arguments.ContainsKey(NotificationGuidBundleKey))
                NotificationGuid = Serializer.Deserialize<Guid>(Arguments.GetString(NotificationGuidBundleKey));
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(ShortcodeFragment)} [folder.name={folder?.Name}, folder.id={folderId ?? folder?.Id}, shortcodeId={shortcodeId ?? shortcodePreview?.Id}...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_with_progress, container, false);
            rootView.SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.lightgray)));

            progress = rootView.FindViewById<ProgressBar>(Resource.Id.progress);
            scrollView = rootView.FindViewById<NestedScrollView>(Resource.Id.scroll_view);
            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            linearLayout.SetClipToPadding(false);
            var padding = Conversion.ConvertDpToPixels(10f);
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

        public override void OnDestroyView()
        {
            dismissAction?.Invoke();
            base.OnDestroyView();
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = null;
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;

            CommonConfig.Logger.Info($"Created {nameof(ShortcodeFragment)} [folder.name={folder?.Name}, folder.id={folderId ?? folder?.Id}, shortcodeId={shortcodeId ?? shortcodePreview?.Id}]");
        }

        public override async void OnResume()
        {
            base.OnResume();

            await RefreshData(forceRefresh);
            forceRefresh = false;
        }

        #region Options menu

        static class MenuItemActions
        {
            public const int CreateNewDocument = 20;
            public const int CopyToWorktray = 30;
            public const int CopyToFolder = 40;
            public const int MoveToFolder = 41;
            public const int Edit = 50;
            public const int Actions = 70;
            public const int Links = 80;
            public const int Delete = 90;
            public const int DeleteFromFolder = 100;
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            if (shortcodePreview == null)
                return;

            menu.Add(Menu.None, MenuItemActions.CreateNewDocument, MenuItemActions.CreateNewDocument, Resource.String.create_new_document);

            if (ServerConfig.SystemSettings.ShortcodesModuleInfo.WorktrayEnabled ?? true)
                menu.Add(Menu.None, MenuItemActions.CopyToWorktray, MenuItemActions.CopyToWorktray, Resource.String.copy_to_worktray);

            menu.Add(Menu.None, MenuItemActions.CopyToFolder, MenuItemActions.CopyToFolder, Resource.String.copy_to_folder);

            if (folder?.InternalType == FolderInternalType.FilterView || folder?.InternalType == FolderInternalType.Static || folder?.InternalType == FolderInternalType.Worktray)
                menu.Add(Menu.None, MenuItemActions.MoveToFolder, MenuItemActions.MoveToFolder, Resource.String.move_to_folder);

            menu.Add(Menu.None, MenuItemActions.Actions, MenuItemActions.Actions, Resource.String.history);
            menu.Add(Menu.None, MenuItemActions.Links, MenuItemActions.Links, Resource.String.overview);

            if (folder?.InternalType == FolderInternalType.FilterView || folder?.InternalType == FolderInternalType.Static || folder?.InternalType == FolderInternalType.Worktray)
                menu.Add(Menu.None, MenuItemActions.DeleteFromFolder, MenuItemActions.DeleteFromFolder, Resource.String.delete_from_folder);

            if (ServerConfig.SystemSettings.ShortcodesModuleInfo.Permissions.DeleteAllowed)
                menu.Add(Menu.None, MenuItemActions.Delete, MenuItemActions.Delete, Resource.String.delete);

            if (ServerConfig.SystemSettings.ShortcodesModuleInfo.Permissions.EditAllowed)
                menu.Add(Menu.None, MenuItemActions.Edit, MenuItemActions.Edit, Resource.String.edit);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == MenuItemActions.CreateNewDocument)
            {
                CommonConfig.UsageAnalytics.LogEvent(new ShortcodeComposeDocumentEvent());

                if (!ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines.Any())
                {
                    Dialogs.ShowConfirmDialog(Activity, Resource.String.no_lines_error_title, Resource.String.no_lines_error_content);
                    return true;
                }
                StartActivity(ComposeDocumentActivity.CreateIntent(Context, DocumentCreationModeFlag.New, CopyToNewOption.None,
                                                                   preconfiguredEmailAddresses: new Dictionary<DocumentAddressType, string[]>
                {
                    { DocumentAddressType.To, shortcode.Addresses.Where(a => a.Type == CommunicationAddressType.Email && a.AddressType == DocumentAddressType.To).Select(a => a.Address).ToArray() },
                    { DocumentAddressType.Cc, shortcode.Addresses.Where(a => a.Type == CommunicationAddressType.Email && a.AddressType == DocumentAddressType.Cc).Select(a => a.Address).ToArray() },
                    { DocumentAddressType.Bcc, shortcode.Addresses.Where(a => a.Type == CommunicationAddressType.Email && a.AddressType == DocumentAddressType.Bcc).Select(a => a.Address).ToArray() },
                }));
            }

            if (item.ItemId == MenuItemActions.CopyToWorktray)
            {
                CopyToWorktrayAction();
                return true;
            }

            if (item.ItemId == MenuItemActions.CopyToFolder)
            {
                StartActivity(CopyMoveToFolderListActivity.CreateIntent(Context, CopyMoveToFolderListActivity.ModeType.Copy, ModuleType.Shortcodes,
                                                                        new List<IBusinessEntity> { shortcodePreview }));
                return true;
            }

            if (item.ItemId == MenuItemActions.MoveToFolder)
            {
                StartActivity(CopyMoveToFolderListActivity.CreateIntent(Context, CopyMoveToFolderListActivity.ModeType.Move, ModuleType.Shortcodes,
                                                                        new List<IBusinessEntity> { shortcodePreview }, folder));
                return true;
            }

            if (item.ItemId == MenuItemActions.Edit)
            {
                StartActivityForResult(AddEditShortcodeActivity.CreateIntent(Context, ShortcodeCreationModeFlag.Edit, shortcodePreview), RequestCodes.EditRequest);
                return true;
            }

            if (item.ItemId == MenuItemActions.Actions)
            {
                StartActivity(ObjectActionsActivity.CreateIntent(Context, shortcodePreview as IBusinessEntity));
                return true;
            }

            if (item.ItemId == MenuItemActions.Links)
            {
                StartActivity(ObjectLinksActivity.CreateIntent(Context, shortcodePreview as IBusinessEntity));
                return true;
            }

            if (item.ItemId == MenuItemActions.DeleteFromFolder)
            {
                DeleteFromFolderAction();
                return true;
            }

            if (item.ItemId == MenuItemActions.Delete)
            {
                DeleteAction();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            forceRefresh |= (resultCode == (int)Result.Ok && requestCode == RequestCodes.EditRequest);
        }

        async void CopyToWorktrayAction()
        {
            var option = await Dialogs.ShowListDialog(Context, Resource.String.copy_to_worktray, Resource.Array.copy_to_worktray_options, true);

            if (option == 0)
            {
                CommonConfig.Logger.Info($"Attempting copy to worktray [shortcodePreview={shortcodePreview}]...");

                dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.copying_to_worktray, Resource.String.please_wait);

                try
                {
                    await Managers.CommonActionsManager.CopyToWorktray(new List<IBusinessEntity>
                    {
                        shortcodePreview
                    });

                    dismissAction();
                }
                catch (Exception ex)
                {
                    dismissAction();

                    CommonConfig.Logger.Error($"Copying to worktray failed [shortcodePreview={shortcodePreview}]", ex);

                    await Dialogs.ShowErrorDialogAsync(Activity, ex);
                }
            }

            if (option == 1)
            {
                StartActivity(CopyToUserWorktrayActivity.CreateIntent(Context, new List<IBusinessEntity> { shortcodePreview }));
            }
        }

        async void DeleteFromFolderAction()
        {
            var yesNo = await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.delete_from_folder, Resource.String.delete_from_folder_are_you_sure);
            if (!yesNo)
                return;

            CommonConfig.Logger.Info($"Attempting to delete from folder [shortcodePreview={shortcodePreview}]...");

            dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.deleting_from_folder, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.RemoveFromFolder(new List<IBusinessEntity>
                    {
                        shortcodePreview
                    },
                    folder);

                dismissAction();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Deleting from folder failed [shortcodePreview={shortcodePreview}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        async void DeleteAction()
        {
            var yesNo = await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.delete, Resource.String.delete_are_you_sure);
            if (!yesNo)
                return;

            CommonConfig.Logger.Info($"Attempting to delete [shortcodePreview={shortcodePreview}]...");

            dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.deleting, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.Delete(new List<IBusinessEntity>
                {
                    shortcodePreview
                });

                dismissAction();
                Activity?.OnBackPressed();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Deleting failed [shortcodePreview={shortcodePreview}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        #endregion

        void AddressesView_DocumentAddressClicked(object sender, DocumentAddress e)
        {
            if (e.Type == CommunicationAddressType.Email)
            {
                CommonConfig.UsageAnalytics.LogEvent(new ShortcodeClickEmailEvent());

                if (!ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines.Any())
                {
                    Dialogs.ShowConfirmDialog(Activity, Resource.String.no_lines_error_title, Resource.String.no_lines_error_content);
                    return;
                }

                StartActivity(ComposeDocumentActivity.CreateIntent(Context, DocumentCreationModeFlag.New, CopyToNewOption.None,
                                                                   preconfiguredEmailAddresses: new Dictionary<DocumentAddressType, string[]>
                {
                    { DocumentAddressType.To, new [] {e.Address} }
                }));
            }
        }

        async Task RefreshData(bool force = false)
        {
            try
            {
                if (NotificationGuid != default(Guid))
                    await Managers.NotificationsManager.MarkAsRead(NotificationGuid);

                if (force || (shortcodeId.HasValue && shortcodePreview == null && shortcode == null))
                {
                    var sourceType = (Restored && !force) ? SourceType.Local : SourceType.Auto;

                    var container = await Managers.ShortcodesManager.GetShortcodeWithPreviewAsync(folderId ?? folder?.Id, shortcodeId ?? shortcodePreview.Id, sourceType);
                    shortcodePreview = container.ShortcodePreview;
                    shortcode = container.Shortcode;
                }

                if (shortcodePreview != null && shortcode == null)
                    shortcode = await Managers.ShortcodesManager.GetShortcodeAsync(folderId ?? folder?.Id, shortcodePreview.Id, Restored ? SourceType.Local : SourceType.Auto);

                RefreshView();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Downloading shortcode failed [folder.name={folder?.Name}, folder.id={folderId ?? folder?.Id}, shortcodeId={shortcodeId ?? shortcodePreview?.Id}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);

                Activity?.OnBackPressed();
            }
        }

        void RefreshView()
        {
            ((AppCompatActivity)Activity).SupportActionBar.Title = shortcodePreview.Name;

            progress.Visibility = ViewStates.Gone;
            scrollView.Visibility = ViewStates.Visible;

            for (var i = 0; i < linearLayout.ChildCount; i++)
            {
                if (linearLayout.GetChildAt(i) is ShortcodeView dv)
                {
                    dv.ShortcodePreview = shortcodePreview;
                    dv.Shortcode = shortcode;
                    dv.RefreshView();
                }
            }

            linearLayout.Invalidate();
            linearLayout.RequestLayout();

            Activity.InvalidateOptionsMenu();
        }
    }
}