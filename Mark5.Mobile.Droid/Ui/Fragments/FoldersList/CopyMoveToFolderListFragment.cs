using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class CopyMoveToFolderListFragment : FoldersListFragment
    {
        const string IdsIntentKey = "IdsIntentKey";
        const string ObjectTypeIntentKey = "ObjectTypeIntentKey";
        const string FromFolderBundleKey = "FromFolder_8dc816f4-08b8-428d-9cce-1c481c460df5";
        const string ActionTypeBundleKey = "ActionType_ed011f46-e180-462c-9d49-5dc047f3c324";
        const string DelayedCopyBundleKey = "DelayedCopy_ed011f46-e180-462c-9d49-5dc047f3c325";

        List<int> _businessEntitiesIds;
        ObjectType _objectType;
        Folder _fromFolder;
        ActionType _actionType;
        bool _delayedCopy;
        Action _dismissAction;

        public static (CopyMoveToFolderListFragment fragment, string tag) NewInstance(Folder remoteFolder, List<int> ids, ObjectType ot,
            Folder fromFolder = null, ActionType? actionType = null, bool? loadRemoteFromCache = null, bool? delayedCopy = false)
        {
            var args = new Bundle();

            if (remoteFolder != null)
                args.PutString(RemoteFolderBundleKey, Serializer.Serialize(remoteFolder));

            if (ids != null)
                args.PutString(IdsIntentKey, Serializer.Serialize(ids));

            if (fromFolder != null)
                args.PutString(FromFolderBundleKey, Serializer.Serialize(fromFolder));

            if (actionType != null)
                args.PutInt(ActionTypeBundleKey, (int)actionType);

            if (loadRemoteFromCache != null)
                args.PutBoolean(LoadRemoteFromCacheBundleKey, loadRemoteFromCache.Value);

            if (delayedCopy != null)
                args.PutBoolean(DelayedCopyBundleKey, delayedCopy.Value);

            args.PutInt(ObjectTypeIntentKey, (int)ot);

            var fragment = new CopyMoveToFolderListFragment();
            fragment.Arguments = args;

            var tag = $"{nameof(FoldersListFragment)} [FolderId={remoteFolder?.Id}, ModuleType={remoteFolder?.Module}]" + $" / {nameof(CopyMoveToFolderListFragment)} [businessEntities.Count={ids?.Count}, businessEntity.Type={ot}, fromFolder.Id={fromFolder?.Id ?? -1}]";

            return (fragment, tag);
        }

        #region Fragment overrides

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (Arguments == null)
                return base.OnCreateView(inflater, container, savedInstanceState);

            if (Arguments.ContainsKey(IdsIntentKey))
                _businessEntitiesIds = Serializer.Deserialize<List<int>>(Arguments.GetString(IdsIntentKey));

            _objectType = (ObjectType)Arguments.GetInt(ObjectTypeIntentKey);

            if (Arguments.ContainsKey(FromFolderBundleKey))
                _fromFolder = Serializer.Deserialize<Folder>(Arguments.GetString(FromFolderBundleKey));

            if (Arguments.ContainsKey(ActionTypeBundleKey))
                _actionType = (ActionType)Arguments.GetInt(ActionTypeBundleKey);

            if (Arguments.ContainsKey(DelayedCopyBundleKey))
                _delayedCopy = Arguments.GetBoolean(DelayedCopyBundleKey);

            return base.OnCreateView(inflater, container, savedInstanceState);
        }

        public override void OnDestroyView()
        {
            _dismissAction?.Invoke();
            base.OnDestroyView();
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

        protected override (BaseFragment fragment, string tag) GetFolderFragment(Folder folder) =>
            NewInstance(folder, _businessEntitiesIds, _objectType, _fromFolder, _actionType);

        protected override async void Adapter_ItemClicked(object sender, int position)
        {
            if (position < 0)
                return;

            var toFolder = CurrentAdapter.GetItemAtPosition(position).Folder;

            if (toFolder.InternalType != FolderInternalType.FilterView
                && toFolder.InternalType != FolderInternalType.Static
                && toFolder.InternalType != FolderInternalType.Worktray)
            {
                await Dialogs.ShowConfirmDialogAsync(Context,
                    Resource.String.failed,
                    Resource.String.cannot_copy_or_move_to_dynamic_folder);
                return;
            }

            if (_delayedCopy == false)
            {
                if (_actionType == ActionType.Copy)
                    await CopyBusinessEntityToFolder(toFolder);
                else
                    await MoveBusinessEntityToFolder(toFolder);
            }
            else
            {
                _dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.copying_to_folder,
                    Resource.String.please_wait);
                var data = new Intent();
                data.PutExtra(CopyMoveToFolderListActivity.SelectedFolderIdResultKey, Serializer.Serialize(toFolder.Id));
                Activity?.SetResult(Android.App.Result.Ok, data);
                _dismissAction();
                Activity?.Finish();
            }
        }

        protected override void Adapter_ItemLongClicked(object sender, int position)
        {
        }

        async Task MoveBusinessEntityToFolder(Folder toFolder)
        {
            var title = GetString(Resource.String.confirm_move_to_folder);

            int resourceId;
            switch (_objectType)
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

            var content = Resources.GetQuantityString(resourceId, _businessEntitiesIds.Count, toFolder.Name);
            var confirmed = await Dialogs.ShowYesNoDialogAsync(Context, title, content);
            if (!confirmed)
                return;

            CommonConfig.Logger.Info($"Moving business entity to folder [businessEntities.Count={_businessEntitiesIds?.Count}, businessEntity.Type={_objectType}, toFolder.Id={toFolder.Id}, fromFolder.Id={_fromFolder?.Id}]");
            _dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity,
                Resource.String.moving_to_folder, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.MoveToFolder(_businessEntitiesIds, _objectType, _fromFolder, toFolder);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while moving business entity to folder [businessEntities.Count={_businessEntitiesIds?.Count}, businessEntity.Type={_objectType}, toFolder.Id={toFolder.Id}, fromFolder.Id={_fromFolder?.Id}]", ex);

                _dismissAction();
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
            finally
            {
                _dismissAction();
                Activity?.Finish();
            }
        }

        async Task CopyBusinessEntityToFolder(Folder folder)
        {
            var title = Resources.GetString(Resource.String.confirm_copy_to_folder);

            int resourceId;
            switch (_objectType)
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

            var content = Resources.GetQuantityString(resourceId, _businessEntitiesIds.Count, folder.Name);
            var confirmed = await Dialogs.ShowYesNoDialogAsync(Context, title, content);

            if (!confirmed)
                return;

            CommonConfig.Logger.Info($"Copying business entities to folder [businessEntities.Count={_businessEntitiesIds?.Count}, businessEntities.Type={_objectType}, folder.Id={folder.Id}]");
            _dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity,
                Resource.String.copying_to_folder, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.CopyToFolder(_businessEntitiesIds, _objectType, folder.Id);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while copying business entities to folder [businessEntities.Count={_businessEntitiesIds?.Count}, businessEntities.Type={_objectType}, folder.Id={folder.Id}]", ex);

                _dismissAction();
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
            finally
            {
                _dismissAction();
                Activity?.Finish();
            }
        }

        public enum ActionType
        {
            Copy = 0,
            Move = 1,
        };
    }
}
