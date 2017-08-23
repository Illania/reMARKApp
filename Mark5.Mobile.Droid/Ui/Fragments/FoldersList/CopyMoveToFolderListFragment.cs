using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Model.HubMessages;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class CopyMoveToFolderListFragment : FoldersListFragment
    {
        public const string BusinessEntitiesBundleKey = "BusinessEntities_cac3f9a0-dba6-486f-ab6f-2b691ae9f243";
        public const string FromFolderBundleKey = "FromFolder_8dc816f4-08b8-428d-9cce-1c481c460df5";
        public const string ActionTypeBundleKey = "ActionType_ed011f46-e180-462c-9d49-5dc047f3c324";

        protected override bool LoadRemoteFromCache => true;

        List<IBusinessEntity> businessEntities;
        Folder fromFolder;
        ActionType actionType;

        public static (CopyMoveToFolderListFragment fragment, string tag) NewInstance(Folder remoteFolder, List<IBusinessEntity> businessEntities, Folder fromFolder = null, ActionType? actionType = null)
        {
            var tag = $"{nameof(FoldersListFragment)} [FolderId={remoteFolder.Id}, ModuleType={remoteFolder.Module}]" + $" / {nameof(CopyMoveToFolderListFragment)} [businessEntities.Count={businessEntities.Count}, businessEntity.Type={businessEntities.First().ObjectType}, fromFolder.Id={fromFolder?.Id ?? -1}]";

            var args = new Bundle();
            args.PutString(RemoteFolderBundleKey, Serializer.Serialize(remoteFolder));
            args.PutString(BusinessEntitiesBundleKey, Serializer.Serialize(businessEntities));

            if (fromFolder != null)
                args.PutString(FromFolderBundleKey, Serializer.Serialize(fromFolder));

            if (actionType != null)
                args.PutString(ActionTypeBundleKey, Serializer.Serialize(actionType.Value));

            var fragment = new CopyMoveToFolderListFragment();
            fragment.Arguments = args;

            return (fragment,tag);
        }

        #region Fragment overrides

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (Arguments.ContainsKey(RemoteFolderBundleKey))
                RemoteFolder = Serializer.Deserialize<Folder>(Arguments.GetString(RemoteFolderBundleKey));

            if (Arguments.ContainsKey(BusinessEntitiesBundleKey))
                businessEntities = Serializer.Deserialize<List<IBusinessEntity>>(Arguments.GetString(BusinessEntitiesBundleKey));

            if (Arguments.ContainsKey(FromFolderBundleKey))
                fromFolder = Serializer.Deserialize<Folder>(Arguments.GetString(FromFolderBundleKey));

            if (Arguments.ContainsKey(ActionTypeBundleKey))
                actionType = Serializer.Deserialize<ActionType>(Arguments.GetString(ActionTypeBundleKey));

            return base.OnCreateView(inflater, container, savedInstanceState);
        }

        #endregion


        protected override void SetSections()
        {
            CommonConfig.Logger.Info("Setting sections according to the folder");

            if (RemoteFolder.Root)
                AvailableSections = new List<Section>
                {
                    Section.Favourites,
                    Section.Remote
                };
            else
                AvailableSections = new List<Section>
                {
                    Section.Remote
                };

            Adapter.SetSections(AvailableSections);
        }

        protected override (RetainableStateFragment fragment, string tag) GetFolderFragment(Folder folder)
        {
            return NewInstance(folder, businessEntities, fromFolder, actionType);

        }

        protected override async void Adapter_ItemClicked(object sender, int position)
        {
            var toFolder = CurrentAdapter.GetItemAtPosition(position);
            if (actionType == ActionType.Copy)
                await CopyBusinessEntityToFolder(toFolder);
            else
                await MoveBusinessEntityToFolder(toFolder);
        }

        protected override void Adapter_ItemLongClicked(object sender, int position)
        {
        }

        async Task MoveBusinessEntityToFolder(Folder toFolder)
        {
            var title = GetString(Resource.String.confirm_move_to_folder);

            var resourceId = 0;
            switch (businessEntities.First().ObjectType)
            {
                case ObjectType.Document:
                    resourceId = Resource.Plurals.confirm_move_to_folder_documents;
                    break;
                case ObjectType.Contact:
                    resourceId = Resource.Plurals.confirm_move_to_folder_contacts;
                    break;
                case ObjectType.Shortcode:
                    resourceId = Resource.Plurals.confirm_move_to_folder_shortcodes;
                    break;
                default:
                    throw new ArgumentException("Object type not supported!");
            }


            var content = Resources.GetQuantityString(resourceId, businessEntities.Count, toFolder.Name);
            var confirmed = await Dialogs.ShowYesNoDialogAsync(Context, title, content);
            if (!confirmed)
                return;

            CommonConfig.Logger.Info($"Moving business entity to folder [businessEntities.Count={businessEntities?.Count}, businessEntity.Type={businessEntities.First().ObjectType}, toFolder.Id={toFolder?.Id}, fromFolder.Id={fromFolder?.Id}]");
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.moving_to_folder, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.MoveToFolder(businessEntities, fromFolder, toFolder);
                CommonConfig.MessengerHub.Publish(new EntityMovedFromFolderMessage(this, businessEntities.First().ObjectType, fromFolder.Id, businessEntities.Select(b => b.Id).ToList()));
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while moving business entity to folder [businessEntities.Count={businessEntities?.Count}, businessEntity.Type={businessEntities?.First().ObjectType}, toFolder.Id={toFolder?.Id}, fromFolder.Id={fromFolder?.Id}]", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
            finally
            {
                dismissAction();
                Activity?.Finish();
            }
        }

        async Task CopyBusinessEntityToFolder(Folder folder)
        {
            var title = Resources.GetString(Resource.String.confirm_copy_to_folder);

            var resourceId = 0;
            switch (businessEntities.First().ObjectType)
            {
                case ObjectType.Document:
                    resourceId = Resource.Plurals.confirm_copy_to_folder_documents;
                    break;
                case ObjectType.Contact:
                    resourceId = Resource.Plurals.confirm_copy_to_folder_contacts;
                    break;
                case ObjectType.Shortcode:
                    resourceId = Resource.Plurals.confirm_copy_to_folder_shortcodes;
                    break;
                default:
                    throw new ArgumentException("Object type not supported!");
            }

            var content = Resources.GetQuantityString(resourceId, businessEntities.Count, folder.Name);
            var confirmed = await Dialogs.ShowYesNoDialogAsync(Context, title, content);

            if (!confirmed)
                return;

            CommonConfig.Logger.Info($"Copying business entities to folder [businessEntities.Count={businessEntities?.Count}, businessEntities.Type={businessEntities?.First().ObjectType}, folder.Id={folder?.Id}]");
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.copying_to_folder, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.CopyToFolder(businessEntities, folder);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while copying business entities to folder [businessEntities.Count={businessEntities?.Count}, businessEntities.Type={businessEntities?.First().ObjectType}, folder.Id={folder?.Id}]", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
            finally
            {
                dismissAction();
                Activity?.Finish();
            }
        }

        #region Retained Fragment methods

        public override string GenerateTag()
        {
            return base.GenerateTag() + $" / {nameof(CopyMoveToFolderListFragment)} [businessEntities.Count={businessEntities.Count}, businessEntity.Type={businessEntities.First().ObjectType}, fromFolder.Id={fromFolder?.Id ?? -1}]";
        }

        public override IRetainableState OnRetainInstanceState()
        {
            var baseState = base.OnRetainInstanceState() as FolderListFragmentState;

            CommonConfig.Logger.Info($"Retaining state: [businessEntities.Count={businessEntities?.Count}, businessEntity.Type={businessEntities.First().ObjectType}, fromFolder.Id={fromFolder?.Id}]");

            return new MoveToFolderListFragmentState
            {
                Folder = baseState.Folder,
                SelectedItemPositions = baseState.SelectedItemPositions,
                BusinessEntities = businessEntities,
                FromFolder = fromFolder,
                Type = actionType
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            base.OnRetainedInstanceStateRestored(restoredState as FolderListFragmentState);

            if (restoredState is MoveToFolderListFragmentState flfs)
            {
                businessEntities = flfs.BusinessEntities;
                fromFolder = flfs.FromFolder;
                actionType = flfs.Type;
                CommonConfig.Logger.Info($"Restored state state: [businessEntities.Count={businessEntities?.Count}, businessEntity.Type={businessEntities?.First().ObjectType}, fromFolder.Id={fromFolder?.Id}]");
            }
        }

        protected class MoveToFolderListFragmentState : FolderListFragmentState
        {
            public List<IBusinessEntity> BusinessEntities { get; set; }
            public Folder FromFolder { get; set; }
            public ActionType Type { get; set; }
        }

        #endregion

        public enum ActionType
        {
            Copy,
            Move,
        };
    }
}