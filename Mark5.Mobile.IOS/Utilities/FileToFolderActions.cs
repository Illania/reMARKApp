using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using UIKit;

namespace Mark5.Mobile.IOS.Utilities
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

        public static async Task CopyToFolderAsync(this UIViewController vc, IBusinessEntity businessEntity) =>
           await CopyToFolderAsync(vc, new List<IBusinessEntity> { businessEntity });

        public static async Task CopyToFolderAsync(this UIViewController vc, List<IBusinessEntity> businessEntities)
        {
            var copyVc = new CopyMoveToFolderListViewController(GetModuleType(businessEntities), businessEntities);
            vc.PresentViewController(new NavigationController(copyVc, UIModalPresentationStyle.PageSheet), true, null);
            await copyVc.Result;
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

        public static async Task CopyToUserWorktrayAsync(this UIViewController vc, IBusinessEntity businessEntity) =>
           await CopyToUserWorktrayAsync(vc, new List<IBusinessEntity> { businessEntity });

        public static async Task CopyToUserWorktrayAsync(this UIViewController vc, List<IBusinessEntity> businessEntities)
        {
            var copyVc = new CopyToUserWorktrayViewController
            {
                BusinessEntities = businessEntities,
                ModalPresentationStyle = UIModalPresentationStyle.PageSheet

            };

            vc.PresentViewController(new NavigationController(copyVc, UIModalPresentationStyle.PageSheet), true, null);
            await copyVc.Result;
        }

        public static async Task CopyToDepartmentWorktrayAsync(this UIViewController vc, IBusinessEntity businessEntity) =>
           await CopyToDepartmentWorktrayAsync(vc, new List<IBusinessEntity> { businessEntity });

        public static async Task CopyToDepartmentWorktrayAsync(this UIViewController vc, List<IBusinessEntity> businessEntities)
        {
            var copyVc = new CopyToDepartmentWorktrayViewController
            {
                BusinessEntities = businessEntities,
                ModalPresentationStyle = UIModalPresentationStyle.PageSheet

            };

            vc.PresentViewController(new NavigationController(copyVc, UIModalPresentationStyle.PageSheet), true, null);
            await copyVc.Result;
        }
        #endregion

        public static ModuleType GetModuleType(List<IBusinessEntity> businessEntities)
        {
            return businessEntities.FirstOrDefault().ModuleType;
        }
    }
}
