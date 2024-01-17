using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using reMark.Mobile.Common.Model;
using reMark.Mobile.IOS.Ui.Common;
using reMark.Mobile.IOS.Ui.ViewControllers;
using reMark.Mobile.IOS.Ui.ViewControllers.FoldersList;
using UIKit;

namespace reMark.Mobile.IOS.Utilities
{
    public static class FileToFolderActions
    {
        #region CopyToFolder
        public static void CopyToFolder(this UIViewController vc, IBusinessEntity businessEntity) =>
           CopyToFolder(vc, new List<IBusinessEntity> { businessEntity });

        public static void CopyToFolder(this UIViewController vc, List<IBusinessEntity> businessEntities)
        {
            var copyVc = new CopyMoveToFolderListViewController(GetModuleType(businessEntities), businessEntities);
            vc.PresentViewController(new NavigationController(copyVc, UIModalPresentationStyle.PageSheet), true, null);
        }

        public static async Task<int?> CopyToFolderAsync(this UIViewController vc, IBusinessEntity businessEntity, bool delayedCopy = false)
        {
            var folderId = await CopyToFolderAsync(vc, new List<IBusinessEntity> { businessEntity }, delayedCopy);
            return folderId;
        }

        public static async Task<int?> CopyToFolderAsync(this UIViewController vc, List<IBusinessEntity> businessEntities, bool delayedCopy = false)
        {
            var copyVc = new CopyMoveToFolderListViewController(GetModuleType(businessEntities), businessEntities, null, delayedCopy);
            vc.PresentViewController(new NavigationController(copyVc, UIModalPresentationStyle.PageSheet), true, null);
            var folderId = await copyVc.Result;
            return folderId;
        }
        #endregion

        #region MoveToFolder
        public static void MoveToFolder(this UIViewController vc, IBusinessEntity businessEntity, Folder folder) =>
            MoveToFolder(vc, new List<IBusinessEntity> { businessEntity }, folder);

        public static void MoveToFolder(this UIViewController vc, List<IBusinessEntity> businessEntities, Folder folder)
        {
            var copyVc = new CopyMoveToFolderListViewController(GetModuleType(businessEntities), businessEntities, folder);
            vc.PresentViewController(new NavigationController(copyVc, UIModalPresentationStyle.PageSheet), true, null);
        }

        public static async Task MoveToFolderAsync(this UIViewController vc, IBusinessEntity businessEntity, Folder folder) =>
           await MoveToFolderAsync(vc, new List<IBusinessEntity> { businessEntity }, folder);

        public static async Task MoveToFolderAsync(this UIViewController vc, List<IBusinessEntity> businessEntities, Folder folder)
        {
            var copyVc = new CopyMoveToFolderListViewController(GetModuleType(businessEntities), businessEntities, folder);
            vc.PresentViewController(new NavigationController(copyVc, UIModalPresentationStyle.PageSheet), true, null);
            await copyVc.Result;
        }
        #endregion

        #region Copy to worktray
        public static void CopyToWorktray(this UIViewController vc, IBusinessEntity businessEntity) =>
            CopyToWorktray(vc, new List<IBusinessEntity> {businessEntity});

        public static void CopyToWorktray(this UIViewController vc, List<IBusinessEntity> businessEntities)
        {
            var copyVc = new CopyToWorktrayViewController
            {
                BusinessEntities = businessEntities,
                ModalPresentationStyle = UIModalPresentationStyle.PageSheet
                
            };
   
            vc.PresentViewController(new NavigationController(copyVc, UIModalPresentationStyle.PageSheet), true, null);
        }

        public static async Task<List<int>> CopyToUserWorktrayAsync(this UIViewController vc, IBusinessEntity businessEntity, bool delayedCopy = false) =>
           await CopyToUserWorktrayAsync(vc, new List<IBusinessEntity> { businessEntity }, delayedCopy);

        public static async Task<List<int>> CopyToUserWorktrayAsync(this UIViewController vc, List<IBusinessEntity> businessEntities, bool delayedCopy = false)
        {
            var copyVc = new CopyToUserWorktrayViewController
            {
                BusinessEntities = businessEntities,
                ModalPresentationStyle = UIModalPresentationStyle.PageSheet,
                DelayedCopy = delayedCopy
            };

            vc.PresentViewController(new NavigationController(copyVc, UIModalPresentationStyle.PageSheet), true, null);
            var selectedUsers = await copyVc.Result;
            return selectedUsers;
        }

        public static async Task<List<int>> CopyToDepartmentWorktrayAsync(this UIViewController vc, IBusinessEntity businessEntity, bool delayedCopy = false) =>
           await CopyToDepartmentWorktrayAsync(vc, new List<IBusinessEntity> { businessEntity }, delayedCopy);

        public static async Task<List<int>> CopyToDepartmentWorktrayAsync(this UIViewController vc, List<IBusinessEntity> businessEntities, bool delayedCopy = false)
        {
            var copyVc = new CopyToDepartmentWorktrayViewController
            {
                BusinessEntities = businessEntities,
                ModalPresentationStyle = UIModalPresentationStyle.PageSheet,
                DelayedCopy = delayedCopy
            };

            vc.PresentViewController(new NavigationController(copyVc, UIModalPresentationStyle.PageSheet), true, null);
            var selectedUsers = await copyVc.Result;
            return selectedUsers;
        }
        #endregion

        public static ModuleType GetModuleType(List<IBusinessEntity> businessEntities)
        {
            return businessEntities.FirstOrDefault().ModuleType;
        }
    }
}
