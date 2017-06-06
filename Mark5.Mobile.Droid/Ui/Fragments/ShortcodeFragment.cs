//
// Project: Mark5.Mobile.Droid
// File: ShortcodeFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Common.HubMessages;
using Mark5.Mobile.Droid.Ui.Views.ShortcodeViews;
using Mark5.Mobile.Droid.Utilities;

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
        public Guid NotificationGuid { get; set; }

        ProgressBar progress;
        ScrollView scrollView;
        LinearLayoutCompat linearLayout;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(ShortcodeFragment)} [folder.name={Folder?.Name}, folder.id={FolderId ?? Folder?.Id}, shortcodeId={ShortcodeId ?? ShortcodePreview?.Id}...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_with_progress, container, false);
            rootView.SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.lightgray)));

            progress = rootView.FindViewById<ProgressBar>(Resource.Id.progress);
            scrollView = rootView.FindViewById<ScrollView>(Resource.Id.scroll_view);
            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            linearLayout.SetClipToPadding(false);
            var padding = ConversionUtils.ConvertDpToPixels(10f);
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

            ((AppCompatActivity) Activity).SupportActionBar.Title = null;
            ((AppCompatActivity) Activity).SupportActionBar.Subtitle = null;

            CommonConfig.Logger.Info($"Created {nameof(ShortcodeFragment)} [folder.name={Folder?.Name}, folder.id={FolderId ?? Folder?.Id}, shortcodeId={ShortcodeId ?? ShortcodePreview?.Id}]");
        }

        public override async void OnResume()
        {
            base.OnResume();

            await RefreshData();
        }

        #region Options menu

        static class MenuItemActions
        {
            public const int CreateNewDocument = 20;
            public const int CopyToWorktray = 30;
            public const int CopyToFolder = 40;
            public const int MoveToFolder = 41;
            public const int Actions = 70;
            public const int Links = 80;
            public const int Delete = 90;
            public const int DeleteFromFolder = 100;
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            if (ShortcodePreview == null)
                return;

            menu.Add(Menu.None, MenuItemActions.CreateNewDocument, MenuItemActions.CreateNewDocument, Resource.String.create_new_document);
            menu.Add(Menu.None, MenuItemActions.CopyToWorktray, MenuItemActions.CopyToWorktray, Resource.String.copy_to_worktray);
            menu.Add(Menu.None, MenuItemActions.CopyToFolder, MenuItemActions.CopyToFolder, Resource.String.copy_to_folder);

            if (Folder?.InternalType == FolderInternalType.FilterView || Folder?.InternalType == FolderInternalType.Static || Folder?.InternalType == FolderInternalType.Worktray)
            {
                menu.Add(Menu.None, MenuItemActions.MoveToFolder, MenuItemActions.MoveToFolder, Resource.String.move_to_folder);
            }

            menu.Add(Menu.None, MenuItemActions.Actions, MenuItemActions.Actions, Resource.String.actions);
            menu.Add(Menu.None, MenuItemActions.Links, MenuItemActions.Links, Resource.String.links);

            if (Folder?.InternalType == FolderInternalType.FilterView || Folder?.InternalType == FolderInternalType.Static || Folder?.InternalType == FolderInternalType.Worktray)
            {
                menu.Add(Menu.None, MenuItemActions.DeleteFromFolder, MenuItemActions.DeleteFromFolder, Resource.String.delete_from_folder);
            }

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator || ServerConfig.SystemSettings.ShortcodesModuleInfo.Permissions.DeleteAllowed)
            {
                menu.Add(Menu.None, MenuItemActions.Delete, MenuItemActions.Delete, Resource.String.delete);
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == MenuItemActions.CreateNewDocument)
            {
                if (!ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines.Any())
                {
                    Dialogs.ShowConfirmDialog(Activity, Resource.String.no_lines_error_title, Resource.String.no_lines_error_content);
                    return true;
                }

                StartActivity(ComposeDocumentActivity.CreateIntent(Context, DocumentCreationModeFlag.New, DocumentDirection.None, preconfiguredEmailToAddresses: Shortcode.Addresses.Where(a => a.Type == CommunicationAddressType.Email && a.AddressType == DocumentAddressType.To).Select(a => a.Address).ToList(), preconfiguredEmailCcAddresses: Shortcode.Addresses.Where(a => a.Type == CommunicationAddressType.Email && a.AddressType == DocumentAddressType.Cc).Select(a => a.Address).ToList(), preconfiguredEmailBccAddresses: Shortcode.Addresses.Where(a => a.Type == CommunicationAddressType.Email && a.AddressType == DocumentAddressType.Bcc).Select(a => a.Address).ToList()));
            }

            if (item.ItemId == MenuItemActions.CopyToWorktray)
            {
                CopyToWorktrayAction();
                return true;
            }

            if (item.ItemId == MenuItemActions.CopyToFolder)
            {
                var i = new Intent(Activity, typeof(CopyMoveToFolderListActivity));
                i.PutExtra(CopyMoveToFolderListActivity.ModeIntentKey, (int) CopyMoveToFolderListActivity.ModeType.Copy);
                i.PutExtra(CopyMoveToFolderListActivity.ModuleIntentKey, SerializationUtils.Serialize(ModuleType.Shortcodes));
                i.PutExtra(CopyMoveToFolderListActivity.BusinessEntitiesIntentKey, SerializationUtils.Serialize(new List<IBusinessEntity>
                {
                    ShortcodePreview
                }));
                StartActivity(i);

                return true;
            }

            if (item.ItemId == MenuItemActions.MoveToFolder)
            {
                var i = new Intent(Activity, typeof(CopyMoveToFolderListActivity));
                i.PutExtra(CopyMoveToFolderListActivity.ModeIntentKey, (int) CopyMoveToFolderListActivity.ModeType.Move);
                i.PutExtra(CopyMoveToFolderListActivity.ModuleIntentKey, SerializationUtils.Serialize(ModuleType.Shortcodes));
                i.PutExtra(CopyMoveToFolderListActivity.BusinessEntitiesIntentKey, SerializationUtils.Serialize(new List<IBusinessEntity>
                {
                    ShortcodePreview
                }));
                i.PutExtra(CopyMoveToFolderListActivity.FromFolderIntentKey, SerializationUtils.Serialize(Folder));
                StartActivity(i);

                return true;
            }

            if (item.ItemId == MenuItemActions.CopyToFolder)
            {
                var i = new Intent(Activity, typeof(CopyMoveToFolderListActivity));
                i.PutExtra(CopyMoveToFolderListActivity.ModeIntentKey, (int) CopyMoveToFolderListActivity.ModeType.Copy);
                i.PutExtra(CopyMoveToFolderListActivity.ModuleIntentKey, SerializationUtils.Serialize(ModuleType.Shortcodes));
                i.PutExtra(CopyMoveToFolderListActivity.BusinessEntitiesIntentKey, SerializationUtils.Serialize(new List<IBusinessEntity>
                {
                    ShortcodePreview
                }));
                StartActivity(i);

                return true;
            }

            if (item.ItemId == MenuItemActions.MoveToFolder)
            {
                var i = new Intent(Activity, typeof(CopyMoveToFolderListActivity));
                i.PutExtra(CopyMoveToFolderListActivity.ModeIntentKey, (int) CopyMoveToFolderListActivity.ModeType.Move);
                i.PutExtra(CopyMoveToFolderListActivity.ModuleIntentKey, SerializationUtils.Serialize(ModuleType.Shortcodes));
                i.PutExtra(CopyMoveToFolderListActivity.BusinessEntitiesIntentKey, SerializationUtils.Serialize(new List<IBusinessEntity>
                {
                    ShortcodePreview
                }));
                i.PutExtra(CopyMoveToFolderListActivity.FromFolderIntentKey, SerializationUtils.Serialize(Folder));
                StartActivity(i);

                return true;
            }

            if (item.ItemId == MenuItemActions.Actions)
            {
                var i = new Intent(Activity, typeof(ObjectActionsActivity));
                i.PutExtra(ObjectActionsActivity.BusinessEntityIntentKey, SerializationUtils.Serialize(ShortcodePreview as IBusinessEntity));
                StartActivity(i);

                return true;
            }

            if (item.ItemId == MenuItemActions.Links)
            {
                var i = new Intent(Activity, typeof(ObjectLinksActivity));
                i.PutExtra(ObjectLinksActivity.BusinessEntityIntentKey, SerializationUtils.Serialize(ShortcodePreview as IBusinessEntity));
                StartActivity(i);

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

        async void CopyToWorktrayAction()
        {
            var option = await Dialogs.ShowListDialog(Context, Resource.String.copy_to_worktray, Resource.Array.copy_to_worktray_options, true);

            if (option == 0)
            {
                CommonConfig.Logger.Info($"Attempting copy to worktray [shortcodePreview={ShortcodePreview}]...");

                var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.copying_to_worktray, Resource.String.please_wait);

                try
                {
                    await Managers.CommonActionsManager.CopyToWorktray(new List<IBusinessEntity>
                    {
                        ShortcodePreview
                    });

                    dismissAction();
                }
                catch (Exception ex)
                {
                    dismissAction();

                    CommonConfig.Logger.Error($"Copying to worktray failed [shortcodePreview={ShortcodePreview}]", ex);

                    await Dialogs.ShowErrorDialogAsync(Activity, ex);
                }
            }

            if (option == 1)
            {
                StartActivity(CopyToUserWorktrayActivity.CreateIntent(Activity, new List<IBusinessEntity>
                {
                    ShortcodePreview
                }));
            }
        }

        async void DeleteFromFolderAction()
        {
            var yesNo = await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.delete_from_folder, Resource.String.delete_from_folder_are_you_sure);
            if (!yesNo)
            {
                return;
            }

            CommonConfig.Logger.Info($"Attempting to delete from folder [shortcodePreview={ShortcodePreview}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.deleting_from_folder, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.RemoveFromFolder(new List<IBusinessEntity>
                {
                    ShortcodePreview
                }, Folder);

                PlatformConfig.MessengerHub.Publish(new EntityRemovedFromFolderMessage(this, ObjectType.Shortcode, Folder.Id, new List<int>
                {
                    ShortcodePreview.Id
                }));

                dismissAction();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Deleting from folder failed [shortcodePreview={ShortcodePreview}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        async void DeleteAction()
        {
            var yesNo = await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.delete, Resource.String.delete_are_you_sure);
            if (!yesNo)
            {
                return;
            }

            CommonConfig.Logger.Info($"Attempting to delete [shortcodePreview={ShortcodePreview}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.deleting, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.Delete(new List<IBusinessEntity>
                {
                    ShortcodePreview
                });

                PlatformConfig.MessengerHub.Publish(new EntityRemovedMessage(this, ObjectType.Shortcode, new List<int>
                {
                    ShortcodePreview.Id
                }));

                dismissAction();
                if (CloseRequest != null)
                    CloseRequest();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Deleting failed [shortcodePreview={ShortcodePreview}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        #endregion

        void AddressesView_DocumentAddressClicked(object sender, DocumentAddress e)
        {
            if (e.Type == CommunicationAddressType.Email)
            {
                if (!ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines.Any())
                {
                    Dialogs.ShowConfirmDialog(Activity, Resource.String.no_lines_error_title, Resource.String.no_lines_error_content);
                    return;
                }

                StartActivity(ComposeDocumentActivity.CreateIntent(Context, DocumentCreationModeFlag.New, DocumentDirection.None, preconfiguredEmailToAddresses: new List<string>
                {
                    e.Address
                }));
            }
        }

        async Task RefreshData()
        {
            try
            {
                if (NotificationGuid != default(Guid))
                {
                    await Managers.NotificationsManager.MarkAsRead(NotificationGuid);
                }

                if (ShortcodeId.HasValue && ShortcodePreview == null && Shortcode == null)
                {
                    var container = await Managers.ShortcodesManager.GetShortcodeWithPreviewAsync(FolderId ?? Folder?.Id, ShortcodeId.Value);
                    ShortcodePreview = container.ShortcodePreview;
                    Shortcode = container.Shortcode;
                }

                if (ShortcodePreview != null && Shortcode == null)
                {
                    Shortcode = await Managers.ShortcodesManager.GetShortcodeAsync(FolderId ?? Folder?.Id, ShortcodePreview.Id);
                }

                RefreshView();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Downloading shortcode failed [folder.name={Folder?.Name}, folder.id={FolderId ?? Folder?.Id}, shortcodeId={ShortcodeId ?? ShortcodePreview?.Id}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);

                if (CloseRequest != null)
                    CloseRequest();
            }
        }

        void RefreshView()
        {
            ((AppCompatActivity) Activity).SupportActionBar.Title = ShortcodePreview.Name;

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

            Activity.InvalidateOptionsMenu();
        }

        public override IRetainableState OnRetainInstanceState()
        {
            return new ShortcodeFragmentState
            {
                FolderId = FolderId,
                Folder = Folder,
                ShortcodeId = ShortcodeId,
                ShortcodePreview = ShortcodePreview,
                Shortcode = Shortcode
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var sfs = restoredState as ShortcodeFragmentState;
            if (sfs != null)
            {
                FolderId = sfs.FolderId;
                Folder = sfs.Folder;
                ShortcodeId = sfs.ShortcodeId;
                ShortcodePreview = sfs.ShortcodePreview;
                Shortcode = sfs.Shortcode;
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(ShortcodeFragment)} [ShortcodeId={ShortcodePreview?.Id ?? Shortcode?.Id ?? ShortcodeId}]";
        }

        class ShortcodeFragmentState : IRetainableState
        {
            public int? FolderId { get; set; }

            public Folder Folder { get; set; }

            public int? ShortcodeId { get; set; }

            public ShortcodePreview ShortcodePreview { get; set; }

            public Shortcode Shortcode { get; set; }
        }
    }
}