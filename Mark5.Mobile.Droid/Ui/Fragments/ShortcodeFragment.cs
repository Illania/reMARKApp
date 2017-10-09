using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
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
using Mark5.Mobile.Droid.Ui.Views.ShortcodeViews;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class ShortcodeFragment : RetainableStateFragment
    {
        static class RequestCodes
        {
            public const int EditRequest = 1;
        }

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

        bool forceRefresh;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(ShortcodeFragment)} [folder.name={Folder?.Name}, folder.id={FolderId ?? Folder?.Id}, shortcodeId={ShortcodeId ?? ShortcodePreview?.Id}...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_with_progress, container, false);
            rootView.SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.lightgray)));

            progress = rootView.FindViewById<ProgressBar>(Resource.Id.progress);
            scrollView = rootView.FindViewById<ScrollView>(Resource.Id.scroll_view);
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

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = null;
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;

            CommonConfig.Logger.Info($"Created {nameof(ShortcodeFragment)} [folder.name={Folder?.Name}, folder.id={FolderId ?? Folder?.Id}, shortcodeId={ShortcodeId ?? ShortcodePreview?.Id}]");
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
            if (ShortcodePreview == null)
                return;

            menu.Add(Menu.None, MenuItemActions.CreateNewDocument, MenuItemActions.CreateNewDocument, Resource.String.create_new_document);
            menu.Add(Menu.None, MenuItemActions.CopyToWorktray, MenuItemActions.CopyToWorktray, Resource.String.copy_to_worktray);
            menu.Add(Menu.None, MenuItemActions.CopyToFolder, MenuItemActions.CopyToFolder, Resource.String.copy_to_folder);

            if (Folder?.InternalType == FolderInternalType.FilterView || Folder?.InternalType == FolderInternalType.Static || Folder?.InternalType == FolderInternalType.Worktray)
                menu.Add(Menu.None, MenuItemActions.MoveToFolder, MenuItemActions.MoveToFolder, Resource.String.move_to_folder);

            menu.Add(Menu.None, MenuItemActions.Actions, MenuItemActions.Actions, Resource.String.actions);
            menu.Add(Menu.None, MenuItemActions.Links, MenuItemActions.Links, Resource.String.links);

            if (Folder?.InternalType == FolderInternalType.FilterView || Folder?.InternalType == FolderInternalType.Static || Folder?.InternalType == FolderInternalType.Worktray)
                menu.Add(Menu.None, MenuItemActions.DeleteFromFolder, MenuItemActions.DeleteFromFolder, Resource.String.delete_from_folder);

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator || ServerConfig.SystemSettings.ShortcodesModuleInfo.Permissions.DeleteAllowed)
                menu.Add(Menu.None, MenuItemActions.Delete, MenuItemActions.Delete, Resource.String.delete);

            if (ServerConfig.SystemSettings.ShortcodesModuleInfo.Permissions.EditAllowed)
                menu.Add(Menu.None, MenuItemActions.Edit, MenuItemActions.Edit, Resource.String.edit);
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
                StartActivity(ComposeDocumentActivity.CreateIntent(Context,
                                                                   DocumentCreationModeFlag.New,
                                                                   CopyToNewOption.None,
                                                                   preconfiguredEmailAddresses: new Dictionary<DocumentAddressType, string[]>
                {
                    { DocumentAddressType.To, Shortcode.Addresses.Where(a => a.Type == CommunicationAddressType.Email && a.AddressType == DocumentAddressType.To).Select(a => a.Address).ToArray() },
                    { DocumentAddressType.Cc, Shortcode.Addresses.Where(a => a.Type == CommunicationAddressType.Email && a.AddressType == DocumentAddressType.Cc).Select(a => a.Address).ToArray() },
                    { DocumentAddressType.Bcc, Shortcode.Addresses.Where(a => a.Type == CommunicationAddressType.Email && a.AddressType == DocumentAddressType.Bcc).Select(a => a.Address).ToArray() },
                }));
            }

            if (item.ItemId == MenuItemActions.CopyToWorktray)
            {
                CopyToWorktrayAction();
                return true;
            }

            if (item.ItemId == MenuItemActions.CopyToFolder)
            {
                var i = new Intent(Activity, typeof(CopyMoveToFolderListActivity));
                i.PutExtra(CopyMoveToFolderListActivity.ModeIntentKey, (int)CopyMoveToFolderListActivity.ModeType.Copy);
                i.PutExtra(CopyMoveToFolderListActivity.ModuleIntentKey, Serializer.Serialize(ModuleType.Shortcodes));
                i.PutExtra(CopyMoveToFolderListActivity.BusinessEntitiesIntentKey,
                    Serializer.Serialize(new List<IBusinessEntity>
                    {
                        ShortcodePreview
                    }));
                StartActivity(i);

                return true;
            }

            if (item.ItemId == MenuItemActions.MoveToFolder)
            {
                var i = new Intent(Activity, typeof(CopyMoveToFolderListActivity));
                i.PutExtra(CopyMoveToFolderListActivity.ModeIntentKey, (int)CopyMoveToFolderListActivity.ModeType.Move);
                i.PutExtra(CopyMoveToFolderListActivity.ModuleIntentKey, Serializer.Serialize(ModuleType.Shortcodes));
                i.PutExtra(CopyMoveToFolderListActivity.BusinessEntitiesIntentKey,
                    Serializer.Serialize(new List<IBusinessEntity>
                    {
                        ShortcodePreview
                    }));
                i.PutExtra(CopyMoveToFolderListActivity.FromFolderIntentKey, Serializer.Serialize(Folder));
                StartActivity(i);

                return true;
            }

            if (item.ItemId == MenuItemActions.CopyToFolder)
            {
                var i = new Intent(Activity, typeof(CopyMoveToFolderListActivity));
                i.PutExtra(CopyMoveToFolderListActivity.ModeIntentKey, (int)CopyMoveToFolderListActivity.ModeType.Copy);
                i.PutExtra(CopyMoveToFolderListActivity.ModuleIntentKey, Serializer.Serialize(ModuleType.Shortcodes));
                i.PutExtra(CopyMoveToFolderListActivity.BusinessEntitiesIntentKey,
                    Serializer.Serialize(new List<IBusinessEntity>
                    {
                        ShortcodePreview
                    }));
                StartActivity(i);

                return true;
            }

            if (item.ItemId == MenuItemActions.MoveToFolder)
            {
                var i = new Intent(Activity, typeof(CopyMoveToFolderListActivity));
                i.PutExtra(CopyMoveToFolderListActivity.ModeIntentKey, (int)CopyMoveToFolderListActivity.ModeType.Move);
                i.PutExtra(CopyMoveToFolderListActivity.ModuleIntentKey, Serializer.Serialize(ModuleType.Shortcodes));
                i.PutExtra(CopyMoveToFolderListActivity.BusinessEntitiesIntentKey,
                    Serializer.Serialize(new List<IBusinessEntity>
                    {
                        ShortcodePreview
                    }));
                i.PutExtra(CopyMoveToFolderListActivity.FromFolderIntentKey, Serializer.Serialize(Folder));
                StartActivity(i);

                return true;
            }

            if (item.ItemId == MenuItemActions.Edit)
            {
                var intent = new Intent(Context, typeof(AddEditShortcodeActivity));
                intent.PutExtra(AddEditShortcodeActivity.ShortcodeIntentKey, Serializer.Serialize(Shortcode));
                intent.PutExtra(AddEditShortcodeActivity.ShortcodePreviewIntentKey, Serializer.Serialize(ShortcodePreview));
                intent.PutExtra(AddEditShortcodeActivity.ShortcodeCreationModeFlagIntentKey, (int)ShortcodeCreationModeFlag.Edit);

                StartActivityForResult(intent, RequestCodes.EditRequest);
            }

            if (item.ItemId == MenuItemActions.Actions)
            {
                var i = new Intent(Activity, typeof(ObjectActionsActivity));
                i.PutExtra(ObjectActionsActivity.BusinessEntityIntentKey, Serializer.Serialize(ShortcodePreview as IBusinessEntity));
                StartActivity(i);

                return true;
            }

            if (item.ItemId == MenuItemActions.Links)
            {
                var i = new Intent(Activity, typeof(ObjectLinksActivity));
                i.PutExtra(ObjectLinksActivity.BusinessEntityIntentKey, Serializer.Serialize(ShortcodePreview as IBusinessEntity));
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

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            forceRefresh |= (resultCode == (int)Result.Ok && requestCode == RequestCodes.EditRequest);
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
                var i = new Intent(Activity, typeof(CopyToUserWorktrayActivity));
                i.PutExtra(CopyToUserWorktrayActivity.BusinessEntitiesIntentKey, Serializer.Serialize(new List<IBusinessEntity> { ShortcodePreview }));
                StartActivity(i);
            }
        }

        async void DeleteFromFolderAction()
        {
            var yesNo = await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.delete_from_folder, Resource.String.delete_from_folder_are_you_sure);
            if (!yesNo)
                return;

            CommonConfig.Logger.Info($"Attempting to delete from folder [shortcodePreview={ShortcodePreview}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.deleting_from_folder, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.RemoveFromFolder(new List<IBusinessEntity>
                    {
                        ShortcodePreview
                    },
                    Folder);

                CommonConfig.MessengerHub.Publish(new EntityRemovedFromFolderMessage(this,
                    ObjectType.Shortcode,
                    Folder.Id,
                    new List<int>
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
                return;

            CommonConfig.Logger.Info($"Attempting to delete [shortcodePreview={ShortcodePreview}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.deleting, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.Delete(new List<IBusinessEntity>
                {
                    ShortcodePreview
                });

                CommonConfig.MessengerHub.Publish(new EntityRemovedMessage(this,
                    ObjectType.Shortcode,
                    new List<int>
                    {
                        ShortcodePreview.Id
                    }));

                dismissAction();
                CloseRequest?.Invoke();
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

                StartActivity(ComposeDocumentActivity.CreateIntent(Context,
                                                                   DocumentCreationModeFlag.New,
                                                                   CopyToNewOption.None,
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

                if (force || (ShortcodeId.HasValue && ShortcodePreview == null && Shortcode == null))
                {
                    var container = await Managers.ShortcodesManager.GetShortcodeWithPreviewAsync(FolderId ?? Folder?.Id, ShortcodeId ?? ShortcodePreview.Id);
                    ShortcodePreview = container.ShortcodePreview;
                    Shortcode = container.Shortcode;
                }

                if (ShortcodePreview != null && Shortcode == null)
                    Shortcode = await Managers.ShortcodesManager.GetShortcodeAsync(FolderId ?? Folder?.Id, ShortcodePreview.Id);

                RefreshView();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Downloading shortcode failed [folder.name={Folder?.Name}, folder.id={FolderId ?? Folder?.Id}, shortcodeId={ShortcodeId ?? ShortcodePreview?.Id}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);

                CloseRequest?.Invoke();
            }
        }

        void RefreshView()
        {
            ((AppCompatActivity)Activity).SupportActionBar.Title = ShortcodePreview.Name;

            progress.Visibility = ViewStates.Gone;
            scrollView.Visibility = ViewStates.Visible;

            for (var i = 0; i < linearLayout.ChildCount; i++)
            {
                if (linearLayout.GetChildAt(i) is ShortcodeView dv)
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
            if (restoredState is ShortcodeFragmentState sfs)
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