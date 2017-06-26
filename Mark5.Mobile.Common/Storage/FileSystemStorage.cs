using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using PCLStorage;

namespace Mark5.Mobile.Common.Storage
{
    static class FileSystemStorage
    {
        static class Filenames
        {
            public const string ConnectionInfo = "connectionInfo.json";
            public const string InstallationId = "installationId.json";
            public const string SystemSettings = "systemSettings.json";
            public const string SystemUserDepartments = "systemUserDepartments.json";
            public const string FavoriteFolders = "favoriteFolders.json";
            public const string SavedOfflineFolderInfos = "savedOfflineFolderInfos.json";
            public const string NotificationSettings = "notificationSettings.json";
            public const string LastCacheCleanUp = "lastCacheCleanUp.json";
            public const string LastSearchDocumentCriteria = "lastSearchDocumentCriteria.json";
            public const string LastSearchContactsCriteria = "lastSearchContactsCriteria.json";
            public const string LastSearchShortcodesCriteria = "lastSearchShortcodesCriteria.json";

            public const string DocumentWorkingCopy = "documentWorkingCopy.json";
        }

        static readonly IDictionary<string, SemaphoreSlim> semaphores = new Dictionary<string, SemaphoreSlim>
        {
            [Filenames.ConnectionInfo] = new SemaphoreSlim(1),
            [Filenames.InstallationId] = new SemaphoreSlim(1),
            [Filenames.SystemSettings] = new SemaphoreSlim(1),
            [Filenames.SystemUserDepartments] = new SemaphoreSlim(1),
            [Filenames.FavoriteFolders] = new SemaphoreSlim(1),
            [Filenames.SavedOfflineFolderInfos] = new SemaphoreSlim(1),
            [Filenames.NotificationSettings] = new SemaphoreSlim(1),
            [Filenames.LastCacheCleanUp] = new SemaphoreSlim(1),
            [Filenames.LastSearchDocumentCriteria] = new SemaphoreSlim(1),
            [Filenames.LastSearchContactsCriteria] = new SemaphoreSlim(1),
            [Filenames.LastSearchShortcodesCriteria] = new SemaphoreSlim(1)
        };

        static readonly IDictionary<string, object> objectCache = new Dictionary<string, object>();

        #region ConnectionInfo

        public static async Task<ConnectionInfo> GetConnectionInfoAsync(CancellationToken ct = default(CancellationToken))
        {
            return await GetAsync<ConnectionInfo>(Filenames.ConnectionInfo, ct);
        }

        public static async Task SaveConnectionInfoAsync(ConnectionInfo connectionInfo, CancellationToken ct = default(CancellationToken))
        {
            await SaveAsync(connectionInfo, Filenames.ConnectionInfo, ct);
        }

        #endregion

        #region Installation ID

        public static async Task<string> GetInstallationIdAsync(CancellationToken ct = default(CancellationToken))
        {
            return await GetAsync<string>(Filenames.InstallationId, ct);
        }

        public static async Task SaveInstallationIdAsync(string installationId, CancellationToken ct = default(CancellationToken))
        {
            await SaveAsync(installationId, Filenames.InstallationId, ct);
        }

        #endregion

        #region System settings

        public static async Task<SystemSettings> GetSystemSettingsAsync(CancellationToken ct = default(CancellationToken))
        {
            return await GetAsync<SystemSettings>(Filenames.SystemSettings, ct);
        }

        public static async Task SaveSystemSettingsAsync(SystemSettings systemSettings, CancellationToken ct = default(CancellationToken))
        {
            await SaveAsync(systemSettings, Filenames.SystemSettings, ct);
        }

        #endregion

        #region System users departments

        public static async Task<SystemUsersDepartments> GetSystemUsersDepartmentsAsync(CancellationToken ct = default(CancellationToken))
        {
            return await GetAsync<SystemUsersDepartments>(Filenames.SystemUserDepartments, ct);
        }

        public static async Task SaveSystemUsersDepartmentsAsync(SystemUsersDepartments systemUsersDepartments, CancellationToken ct = default(CancellationToken))
        {
            await SaveAsync(systemUsersDepartments, Filenames.SystemUserDepartments, ct);
        }

        #endregion

        #region Favorite folders

        public static async Task<Dictionary<ModuleType, List<Folder>>> GetFavoriteFoldersAsync(CancellationToken ct = default(CancellationToken))
        {
            var favorites = await GetAsync<Dictionary<ModuleType, List<Folder>>>(Filenames.FavoriteFolders, ct);
            return favorites ?? new Dictionary<ModuleType, List<Folder>>();
        }

        public static async Task SaveFavoriteFoldersAsync(Dictionary<ModuleType, List<Folder>> favoriteFolders, CancellationToken ct = default(CancellationToken))
        {
            await SaveAsync(favoriteFolders, Filenames.FavoriteFolders, ct);
        }

        #endregion

        #region Last search criteria

        public static async Task<SearchDocumentsCriteria> GetLastSearchDocumentCrtiera(CancellationToken ct = default(CancellationToken))
        {
            var criteria = await GetAsync<SearchDocumentsCriteria>(Filenames.LastSearchDocumentCriteria, ct);
            return criteria ?? new SearchDocumentsCriteria();
        }

        public static async Task SaveLastSearchDocumentCrtiera(SearchDocumentsCriteria criteria, CancellationToken ct = default(CancellationToken))
        {
            await SaveAsync(criteria, Filenames.LastSearchDocumentCriteria, ct);
        }

        public static async Task<SearchContactsCriteria> GetLastSearchContactsCrtiera(CancellationToken ct = default(CancellationToken))
        {
            var criteria = await GetAsync<SearchContactsCriteria>(Filenames.LastSearchContactsCriteria, ct);
            return criteria ?? new SearchContactsCriteria();
        }

        public static async Task SaveLastSearchContactsCrtiera(SearchContactsCriteria criteria, CancellationToken ct = default(CancellationToken))
        {
            await SaveAsync(criteria, Filenames.LastSearchContactsCriteria, ct);
        }

        public static async Task<SearchShortcodesCriteria> GetLastSearchShortcodesCrtiera(CancellationToken ct = default(CancellationToken))
        {
            var criteria = await GetAsync<SearchShortcodesCriteria>(Filenames.LastSearchShortcodesCriteria, ct);
            return criteria ?? new SearchShortcodesCriteria();
        }

        public static async Task SaveLastSearchShortcodesCrtiera(SearchShortcodesCriteria criteria, CancellationToken ct = default(CancellationToken))
        {
            await SaveAsync(criteria, Filenames.LastSearchShortcodesCriteria, ct);
        }

        #endregion

        #region Offline folders

        public static async Task<List<SavedOfflineFolderInfo>> GetSavedOfflineFolderInfosAsync(CancellationToken ct = default(CancellationToken))
        {
            var infos = await GetAsync<List<SavedOfflineFolderInfo>>(Filenames.SavedOfflineFolderInfos, ct);
            return infos ?? new List<SavedOfflineFolderInfo>();
        }

        public static async Task SaveSavedOfflineFolderInfos(List<SavedOfflineFolderInfo> infos, CancellationToken ct = default(CancellationToken))
        {
            await SaveAsync(infos, Filenames.SavedOfflineFolderInfos, ct);
        }

        #endregion

        #region Notification settings

        public static async Task<NotificationSettings> GetNotificationSettingsAsync(CancellationToken ct = default(CancellationToken))
        {
            return await GetAsync<NotificationSettings>(Filenames.NotificationSettings, ct);
        }

        public static async Task SaveNotificationSettingsAsync(NotificationSettings notificationSettings, CancellationToken ct = default(CancellationToken))
        {
            await SaveAsync(notificationSettings, Filenames.NotificationSettings, ct);
        }

        #endregion

        #region Last cache clean up

        public static async Task<DateTime> GetLastCacheCleanUpAsync(CancellationToken ct = default(CancellationToken))
        {
            var lastCacheCleanUpString = await GetAsync<string>(Filenames.LastCacheCleanUp, ct);
            if (lastCacheCleanUpString == null)
                return DateTime.SpecifyKind(default(DateTime), DateTimeKind.Utc);

            return DateTime.SpecifyKind(Convert.ToDateTime(lastCacheCleanUpString), DateTimeKind.Utc);
        }

        public static async Task SaveLastCacheCleanUpAsync(DateTime lastCacheCleanUp, CancellationToken ct = default(CancellationToken))
        {
            await SaveAsync(lastCacheCleanUp.ToUniversalTime().ToString("s"), Filenames.LastCacheCleanUp, ct);
        }

        #endregion

        #region Documents upload / Document working copy interop

        public static async Task MoveDocumentWorkingCopyToUpload()
        {
            var documentWorkingCopy = await GetDocumentWorkingCopyAsync();
            var documentWorkingCopyAttachments = await GetDocumentWorkingCopyAttachments();

            await DeleteDocumentWorkingCopyAsync();
        }

        #endregion

        #region Documents upload

        public static async Task<Guid> GetDocumentToUploadGuid()
        {
            var folderName = (await CommonConfig.DocumentsToUploadFolder.GetFoldersAsync()).FirstOrDefault()?.Name;

            if (folderName == null)
                return Guid.Empty;

            return new Guid(folderName);
        }

        public static async Task<DocumentToUploadInfo> GetDocumentToUploadInfo(Guid guid)
        {
            var folder = (await CommonConfig.DocumentsToUploadFolder.GetFoldersAsync()).FirstOrDefault(f => f.Name == guid.ToString());
            if (folder == null)
                return null;

            var fileExists = await folder.CheckExistsAsync("documentToUploadInfo.json") == ExistenceCheckResult.FileExists;
            if (!fileExists)
                return null;

            var file = await folder.GetFileAsync("documentToUploadInfo.json");
            if (file == null)
                return null;

            var fileContent = await file.ReadAllTextAsync();

            return Serializer.Deserialize<DocumentToUploadInfo>(fileContent);
        }

        public static async Task<DocumentPreview> GetDocumentToUploadDocumentPreview(Guid guid)
        {
            var folder = (await CommonConfig.DocumentsToUploadFolder.GetFoldersAsync()).FirstOrDefault(f => f.Name == guid.ToString());
            if (folder == null)
                return null;

            var fileExists = await folder.CheckExistsAsync("documentPreview.json") == ExistenceCheckResult.FileExists;
            if (!fileExists)
                return null;

            var file = await folder.GetFileAsync("documentPreview.json");
            if (file == null)
                return null;

            var fileContent = await file.ReadAllTextAsync();

            return Serializer.Deserialize<DocumentPreview>(fileContent);
        }

        public static async Task<Document> GetDocumentToUploadDocument(Guid guid)
        {
            var folder = (await CommonConfig.DocumentsToUploadFolder.GetFoldersAsync()).FirstOrDefault(f => f.Name == guid.ToString());
            if (folder == null)
                return null;

            var fileExists = await folder.CheckExistsAsync("document.json") == ExistenceCheckResult.FileExists;
            if (!fileExists)
                return null;

            var file = await folder.GetFileAsync("document.json");
            if (file == null)
                return null;

            var fileContent = await file.ReadAllTextAsync();

            return Serializer.Deserialize<Document>(fileContent);
        }

        public static async Task<string[]> GetDocumentToUploadAttachmentNames(Guid guid)
        {
            var folder = (await CommonConfig.DocumentsToUploadFolder.GetFoldersAsync()).FirstOrDefault(f => f.Name == guid.ToString());
            if (folder == null)
                return null;

            var attFolderExists = await folder.CheckExistsAsync("att") == ExistenceCheckResult.FolderExists;
            if (!attFolderExists)
                return null;

            var attFolder = await folder.GetFolderAsync("att");
            if (attFolder == null)
                return null;

            return (await attFolder.GetFilesAsync()).Select(f => f.Name).ToArray();
        }

        public static async Task<Stream> GetDocumentToUploadAttachmentStream(Guid guid, string name)
        {
            var folder = (await CommonConfig.DocumentsToUploadFolder.GetFoldersAsync()).FirstOrDefault(f => f.Name == guid.ToString());
            if (folder == null)
                return null;

            var attFolderExists = await folder.CheckExistsAsync("att") == ExistenceCheckResult.FolderExists;
            if (!attFolderExists)
                return null;

            var attFolder = await folder.GetFolderAsync("att");
            if (attFolder == null)
                return null;

            var attExists = await attFolder.CheckExistsAsync(name) == ExistenceCheckResult.FileExists;
            if (!attExists)
                return null;

            var att = await attFolder.GetFileAsync(name);
            if (att == null)
                return null;

            return await att.OpenAsync(PCLStorage.FileAccess.Read);
        }

        public static async Task DeleteDocumentToUpload(Guid guid)
        {
            var folder = (await CommonConfig.DocumentsToUploadFolder.GetFoldersAsync()).FirstOrDefault(f => f.Name == guid.ToString());
            if (folder == null)
                return;

            await folder.DeleteAsync();
        }

        #endregion

        #region Document working copy

        public static async Task SaveDocumentWorkingCopyAsync(DocumentWorkingCopy documentWorkingCopy)
        {
            var documentWorkingCopyFile = await CommonConfig.DocumentWorkingCopyFolder.CreateFileAsync(Filenames.DocumentWorkingCopy, CreationCollisionOption.ReplaceExisting);
            await documentWorkingCopyFile.WriteAllTextAsync(Serializer.Serialize(documentWorkingCopy));
        }

        public static async Task SaveDocumentWorkingCopyAttachmentAsync(string filename, Stream stream)
        {
            var file = await CommonConfig.DocumentWorkingCopyFolder.CreateFileAsync(filename, CreationCollisionOption.GenerateUniqueName);
            using (var fileStream = await file.OpenAsync(PCLStorage.FileAccess.ReadAndWrite))
                try
                {
                    await stream.CopyToAsync(fileStream);
                }
                finally
                {
                    stream.Dispose();
                }
        }

        public static async Task<DocumentWorkingCopy> GetDocumentWorkingCopyAsync()
        {
            if (await CommonConfig.DocumentWorkingCopyFolder.CheckExistsAsync(Filenames.DocumentWorkingCopy) != ExistenceCheckResult.FileExists)
                return null;

            var documentWorkingCopyFile = await CommonConfig.DocumentWorkingCopyFolder.GetFileAsync(Filenames.DocumentWorkingCopy);
            return Serializer.Deserialize<DocumentWorkingCopy>(await documentWorkingCopyFile.ReadAllTextAsync());
        }

        public static async Task DeleteDocumentWorkingCopyAsync()
        {
            var files = await CommonConfig.DocumentWorkingCopyFolder.GetFilesAsync();
            foreach (var file in files)
                await file.DeleteAsync();
        }

        static async Task<string[]> GetDocumentWorkingCopyAttachments() => (await CommonConfig.DocumentWorkingCopyFolder.GetFilesAsync())
            .Where(f => f.Name != Filenames.DocumentWorkingCopy)
            .Select(f => f.Name)
            .ToArray();

        #endregion

        #region Attachments

        public static async Task<string> SaveAttachmentAsync(AttachmentDescription attachmentDescription, Stream attachmentStream, CancellationToken ct = default(CancellationToken))
        {
            var path = await CheckAttachmentsExistsAsync(attachmentDescription);
            if (!string.IsNullOrEmpty(path))
                return path;

            var folderExists = await CommonConfig.AttachmentsFolder.CheckExistsAsync(attachmentDescription.Id.ToString());
            if (folderExists != ExistenceCheckResult.FolderExists)
                await CommonConfig.AttachmentsFolder.CreateFolderAsync(attachmentDescription.Id.ToString(), CreationCollisionOption.OpenIfExists);

            var folder = await CommonConfig.AttachmentsFolder.GetFolderAsync(attachmentDescription.Id.ToString());

            var file = await folder.CreateFileAsync(CommonConfig.Utf8Normalizer(attachmentDescription.SafeName), CreationCollisionOption.ReplaceExisting, ct);
            using (var fileStream = await file.OpenAsync(PCLStorage.FileAccess.ReadAndWrite))
            {
                await attachmentStream.CopyToAsync(fileStream);
            }

            return file.Path;
        }

        public static async Task<string> CheckAttachmentsExistsAsync(AttachmentDescription attachmentDescription)
        {
            var folderExists = await CommonConfig.AttachmentsFolder.CheckExistsAsync(attachmentDescription.Id.ToString());
            if (folderExists != ExistenceCheckResult.FolderExists)
                return string.Empty;

            var folder = await CommonConfig.AttachmentsFolder.GetFolderAsync(attachmentDescription.Id.ToString());

            var fileExists = await folder.CheckExistsAsync(CommonConfig.Utf8Normalizer(attachmentDescription.SafeName));
            if (fileExists != ExistenceCheckResult.FileExists)
                return string.Empty;

            return CommonConfig.AttachmentsFolder.Path + CommonConfig.PathSeparator + attachmentDescription.Id + CommonConfig.PathSeparator + CommonConfig.Utf8Normalizer(attachmentDescription.SafeName);
        }

        #endregion

        #region Private methods

        static async Task<T> GetAsync<T>(string filename, CancellationToken ct = default(CancellationToken)) where T : class
        {
            try
            {
                await semaphores[filename].WaitAsync();

                if (objectCache.ContainsKey(filename))
                    return (T)objectCache[filename];

                var fileExists = await CommonConfig.DataFolder.CheckExistsAsync(filename, ct);
                if (fileExists == ExistenceCheckResult.FileExists)
                {
                    var file = await CommonConfig.DataFolder.GetFileAsync(filename, ct);
                    return await Serializer.DeserializeAsync<T>(await file.ReadAllTextAsync());
                }

                return null;
            }
            finally
            {
                semaphores[filename].Release();
            }
        }

        static async Task SaveAsync<T>(T obj, string filename, CancellationToken ct = default(CancellationToken)) where T : class
        {
            try
            {
                await semaphores[filename].WaitAsync();

                var fileExists = await CommonConfig.DataFolder.CheckExistsAsync(filename, ct);
                if (fileExists != ExistenceCheckResult.FileExists)
                    await CommonConfig.DataFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting, ct);
                var file = await CommonConfig.DataFolder.GetFileAsync(filename, ct);
                await file.WriteAllTextAsync(await Serializer.SerializeAsync(obj));

                objectCache[filename] = obj;
            }
            finally
            {
                semaphores[filename].Release();
            }
        }

        #endregion
    }
}