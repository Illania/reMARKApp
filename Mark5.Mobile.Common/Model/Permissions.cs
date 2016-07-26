//
// Project: Mark5.Mobile.Common
// File: Permissions.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

namespace Mark5.Mobile.Common.Model
{

    public class Permissions
    {

        public bool ManageCategories { get; set; }

        public bool CabinetSupervisor { get; set; }

        public bool CreateFolderAllowed { get; set; }

        public bool EditFolderAllowed { get; set; }

        public bool DeleteFolderAllowed { get; set; }

        public bool RemoveFromFolderAllowed { get; set; }

        public bool ManagePublicDynamicFolderAllowed { get; set; }

        public int MaxPublicPersonalFoldersAllowed { get; set; }

        public bool CreateAllowed { get; set; }

        public bool EditAllowed { get; set; }

        public bool DeleteAllowed { get; set; }

        public bool EditAccessRightsAllowed { get; set; }
    }

    public class DocumentsModulePermissions : Permissions
    {

        public bool IncomingSupervisor { get; set; }

        public bool OutgoingSupervisor { get; set; }

        public bool ManageFilterViewFoldersAllowed { get; set; }

        public bool SpamManager { get; set; }
    }
}

