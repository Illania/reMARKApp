//
// Project: Mark5.Mobile.Common
// File: FileSystemStorage.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using PCLStorage;

#pragma warning disable CS1701
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
            public const string OfflineFolders = "offlineFolders.json";
            public const string NotificationSettings = "notificationSettings.json";
            public const string LastCacheCleanUp = "lastCacheCleanUp.json";

            public const string OutgoingDocument = "document.json";
            public const string OutgoingDocumentPreview = "documentPreview.json";
            public const string OutgoingInfo = "info.json";
            public const string OutgoingLock = ".lock";
            public const string OutgoingFailed = ".failed";
            public const string OutgoingAttachmentFolder = "attachment";
        }

        static readonly IDictionary<string, SemaphoreSlim> semaphores = new Dictionary<string, SemaphoreSlim>
        {
            [Filenames.ConnectionInfo] = new SemaphoreSlim(1),
            [Filenames.InstallationId] = new SemaphoreSlim(1),
            [Filenames.SystemSettings] = new SemaphoreSlim(1),
            [Filenames.SystemUserDepartments] = new SemaphoreSlim(1),
            [Filenames.FavoriteFolders] = new SemaphoreSlim(1),
            [Filenames.OfflineFolders] = new SemaphoreSlim(1),
            [Filenames.NotificationSettings] = new SemaphoreSlim(1),
            [Filenames.LastCacheCleanUp] = new SemaphoreSlim(1)
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

        #region Offline folders

        public static async Task<Dictionary<ModuleType, List<Folder>>> GetOfflineFoldersAsync(CancellationToken ct = default(CancellationToken))
        {
            var offlines = await GetAsync<Dictionary<ModuleType, List<Folder>>>(Filenames.OfflineFolders, ct);
            return offlines ?? new Dictionary<ModuleType, List<Folder>>();
        }

        public static async Task SaveOfflineFoldersAsync(Dictionary<ModuleType, List<Folder>> favoriteFolders, CancellationToken ct = default(CancellationToken))
        {
            await SaveAsync(favoriteFolders, Filenames.OfflineFolders, ct);
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

        #region LastCacheCleanUp

        public static async Task<DateTime> GetLastCacheCleanUpAsync(CancellationToken ct = default(CancellationToken))
        {
            var lastCacheCleanUpString = await GetAsync<string>(Filenames.LastCacheCleanUp, ct);
            if (lastCacheCleanUpString == null)
            {
                return DateTime.SpecifyKind(default(DateTime), DateTimeKind.Utc);
            }
            return DateTime.SpecifyKind(Convert.ToDateTime(lastCacheCleanUpString), DateTimeKind.Utc);
        }

        public static async Task SaveLastCacheCleanUpAsync(DateTime lastCacheCleanUp, CancellationToken ct = default(CancellationToken))
        {
            await SaveAsync(lastCacheCleanUp.ToUniversalTime().ToString("s"), Filenames.LastCacheCleanUp, ct);
        }

        #endregion

        #region OutgoingDocuments

        public static async Task SaveOutgoingDocumentAsync(OutgoingDocumentInfo outgoingDocumentInfo, Document document, DocumentPreview documentPreview, bool cleanFailedState = false)
        {
            var outgoingDocumentFolder = await GetOutgoingFolderAsync(outgoingDocumentInfo.Identifier);

            if (cleanFailedState)
            {
                var wasFailed = (await outgoingDocumentFolder.CheckExistsAsync(Filenames.OutgoingFailed)) == ExistenceCheckResult.FileExists;
                if (wasFailed)
                {
                    var isFailedFile = await outgoingDocumentFolder.GetFileAsync(Filenames.OutgoingFailed);
                    await isFailedFile.DeleteAsync();
                }
            }

            var wasLocked = (await outgoingDocumentFolder.CheckExistsAsync(Filenames.OutgoingLock)) == ExistenceCheckResult.FileExists;
            if (wasLocked)
            {
                var isLockedFile = await outgoingDocumentFolder.GetFileAsync(Filenames.OutgoingLock);
                await isLockedFile.DeleteAsync();
            }

            var documentFile = await outgoingDocumentFolder.CreateFileAsync(Filenames.OutgoingDocument, CreationCollisionOption.ReplaceExisting);
            await documentFile.WriteAllTextAsync(await SerializationUtils.SerializeAsync(document));

            var documentPreviewFile = await outgoingDocumentFolder.CreateFileAsync(Filenames.OutgoingDocumentPreview, CreationCollisionOption.ReplaceExisting);
            await documentPreviewFile.WriteAllTextAsync(await SerializationUtils.SerializeAsync(documentPreview));

            outgoingDocumentInfo.DateLastSavedTimestamp = DateTime.Now.ToUniversalTime().ConvertDateTimeToTimestampMilliseconds();
            var infoFile = await outgoingDocumentFolder.CreateFileAsync(Filenames.OutgoingInfo, CreationCollisionOption.ReplaceExisting);
            await infoFile.WriteAllTextAsync(await SerializationUtils.SerializeAsync(outgoingDocumentInfo));
        }

        public static async Task<OutgoingDocumentContainer> GetOutgoingDocumentContainerAsync(Guid id, bool lockDocument, LoadMode loadMode)
        {
            if (!await OutgoingFolderExistsAsync(id))
            {
                return null;
            }

            var outgoingDocumentFolder = await GetOutgoingFolderAsync(id);

            var documentPreviewFile = await outgoingDocumentFolder.GetFileAsync(Filenames.OutgoingDocumentPreview);
            var documentPreview = await SerializationUtils.DeserializeAsync<DocumentPreview>(await documentPreviewFile.ReadAllTextAsync());

            var infoFile = await outgoingDocumentFolder.GetFileAsync(Filenames.OutgoingInfo);
            var info = await SerializationUtils.DeserializeAsync<OutgoingDocumentInfo>(await infoFile.ReadAllTextAsync());
            var isFailed = (await outgoingDocumentFolder.CheckExistsAsync(Filenames.OutgoingFailed)) == ExistenceCheckResult.FileExists;

            if (isFailed)
            {
                info.State = OutgoingDocumentState.Failed;
            }
            else
            {
                info.State = OutgoingDocumentState.Waiting;
            }

            if (lockDocument)
            {
                await LockOutgoingDocumentAsync(id);
                info.Locked = true;
            }
            else
            {
                var isLocked = (await outgoingDocumentFolder.CheckExistsAsync(Filenames.OutgoingLock)) == ExistenceCheckResult.FileExists;

                if (isLocked)
                {
                    info.Locked = true;
                }
            }

            Document document = null;
            List<OutgoingDocumentAttachmentDescription> attachments = null;

            if (loadMode == LoadMode.Complete)
            {
                var documentFile = await outgoingDocumentFolder.GetFileAsync(Filenames.OutgoingDocument);
                document = await SerializationUtils.DeserializeAsync<Document>(await documentFile.ReadAllTextAsync());

                var attachmentsFolder = await GetOutgoingAttachmentsFolderAsync(id);
                attachments = new List<OutgoingDocumentAttachmentDescription>();
                foreach (var item in await attachmentsFolder.GetFilesAsync())
                {
                    var attachment = new OutgoingDocumentAttachmentDescription();
                    var stream = await item.OpenAsync(PCLStorage.FileAccess.Read);
                    attachment.SizeInBytes = (int)stream.Length;
                    stream.Dispose();
                    attachment.Path = item.Path;
                    attachment.Name = item.Name;
                    attachments.Add(attachment);
                }
            }

            return new OutgoingDocumentContainer
            {
                Document = document,
                DocumentPreview = documentPreview,
                Info = info,
                LocalAttachments = attachments,
                LoadMode = loadMode,
            };
        }

        //TODO also the time should be in the info

        public static async Task<List<OutgoingDocumentContainer>> GetOutgoingDocumentContainersAsync()
        {
            var identifiers = await GetOutgoingDocumentIdentifiersAsync();
            var outgoingDocumentContainers = new List<OutgoingDocumentContainer>();

            foreach (var id in identifiers)
            {
                outgoingDocumentContainers.Add(await GetOutgoingDocumentContainerAsync(id, false, LoadMode.Preview));
            }

            return outgoingDocumentContainers;
        }

        public static async Task<IEnumerable<Guid>> GetOutgoingDocumentIdentifiersAsync()
        {
            var identifiers = new List<Guid>();
            foreach (var folder in await CommonConfig.OutgoingFolder.GetFoldersAsync())
            {
                identifiers.Add(new Guid(folder.Name));
            }
            return identifiers;
        }

        static async Task<IFolder> GetOutgoingAttachmentsFolderAsync(Guid id)
        {
            return await (await GetOutgoingFolderAsync(id)).CreateFolderAsync(Filenames.OutgoingAttachmentFolder, CreationCollisionOption.OpenIfExists);
        }

        static async Task<bool> OutgoingFolderExistsAsync(Guid id)
        {
            return await CommonConfig.OutgoingFolder.CheckExistsAsync(id.ToString()) == ExistenceCheckResult.FolderExists;
        }

        public static async Task DeleteOutgoingDocumentFolderAsync(Guid id)
        {
            var folderExists = await CommonConfig.OutgoingFolder.CheckExistsAsync(id.ToString()) == ExistenceCheckResult.FolderExists;
            if (folderExists)
            {
                var outgoingDocumentFolder = await CommonConfig.OutgoingFolder.GetFolderAsync(id.ToString());
                await outgoingDocumentFolder.DeleteAsync();
            }
        }

        public static async Task<IEnumerable<Attachment>> GetOutgoingDocumentAttachmentsAsync(Guid id)
        {
            var attachmentsFolder = await GetOutgoingAttachmentsFolderAsync(id);
            var attachments = new List<Attachment>();
            foreach (var item in await attachmentsFolder.GetFilesAsync())
            {
                var attachment = new Attachment();
                attachment.Stream = await item.OpenAsync(PCLStorage.FileAccess.Read);
                attachment.Size = (int)attachment.Stream.Length; //TODO check this
                attachment.Extension = Path.GetExtension(item.Name);
                attachment.Filename = Path.GetFileNameWithoutExtension(item.Name);
                attachments.Add(attachment);
            }

            return attachments;
        }

        public static async Task<string> SaveOutgoingDocumentAttachmentAsync(Guid id, string filename, Stream attachmentStream, CancellationToken ct = default(CancellationToken))
        {
            var attachmentsFolder = await GetOutgoingAttachmentsFolderAsync(id);
            var fileExists = await attachmentsFolder.CheckExistsAsync(filename, ct);
            if (fileExists == ExistenceCheckResult.FileExists)
            {
                return Path.Combine(attachmentsFolder.Path, filename);
            }

            var file = await attachmentsFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting, ct);
            using (var fileStream = await file.OpenAsync(PCLStorage.FileAccess.ReadAndWrite))
            {
                await attachmentStream.CopyToAsync(fileStream);
            }

            return file.Path;
        }

        public static async Task RemoveOutgoingDocumentAttachmentAsync(Guid id, string filename, CancellationToken ct = default(CancellationToken))
        {
            var attachmentsFolder = await GetOutgoingAttachmentsFolderAsync(id);

            var fileExists = await attachmentsFolder.CheckExistsAsync(filename, ct);
            if (fileExists != ExistenceCheckResult.FileExists)
            {
                return;
            }

            var file = await attachmentsFolder.GetFileAsync(filename, ct);
            await file.DeleteAsync(ct);

            return;
        }

        public static async Task SetOutgoingDocumentToFailedAsync(Guid id, Exception ex)
        {
            var outgoingDocumentFolder = await GetOutgoingFolderAsync(id);
            await outgoingDocumentFolder.CreateFileAsync(Filenames.OutgoingFailed, CreationCollisionOption.ReplaceExisting);
        }

        public static async Task LockOutgoingDocumentAsync(Guid id)
        {
            var outgoingDocumentFolder = await GetOutgoingFolderAsync(id);
            await outgoingDocumentFolder.CreateFileAsync(Filenames.OutgoingLock, CreationCollisionOption.ReplaceExisting);
        }

        public static async Task UnlockOutgoingDocumentAsync(Guid id)
        {
            var outgoingDocumentFolder = await GetOutgoingFolderAsync(id);
            var lockFile = await outgoingDocumentFolder.CreateFileAsync(Filenames.OutgoingLock, CreationCollisionOption.OpenIfExists);
            await lockFile?.DeleteAsync();
        }

        static async Task<IFolder> GetOutgoingFolderAsync(Guid id)
        {
            return await CommonConfig.OutgoingFolder.CreateFolderAsync(id.ToString(), CreationCollisionOption.OpenIfExists);
        }

        #endregion

        #region Attachments

        public static async Task<string> SaveAttachmentAsync(AttachmentDescription attachmentDescription, Stream attachmentStream, CancellationToken ct = default(CancellationToken))
        {
            var path = await CheckAttachmentsExistsAsync(attachmentDescription);
            if (!string.IsNullOrEmpty(path))
            {
                return path;
            }

            var file = await CommonConfig.AttachmentsFolder.CreateFileAsync(GetAttachmentFilename(attachmentDescription), CreationCollisionOption.ReplaceExisting, ct);
            using (var fileStream = await file.OpenAsync(PCLStorage.FileAccess.ReadAndWrite))
            {
                await attachmentStream.CopyToAsync(fileStream);
            }

            return file.Path;
        }

        public static async Task<string> CheckAttachmentsExistsAsync(AttachmentDescription attachmentDescription)
        {
            var filename = GetAttachmentFilename(attachmentDescription);

            var fileExists = await CommonConfig.AttachmentsFolder.CheckExistsAsync(filename);
            if (fileExists == ExistenceCheckResult.FileExists)
            {
                return CommonConfig.AttachmentsFolder.Path + CommonConfig.PathSeparator + filename;
            }

            return string.Empty;
        }

        static string GetAttachmentFilename(AttachmentDescription attachmentDescription)
        {
            return $"{attachmentDescription.Id}_{attachmentDescription.Name}";
        }

        #endregion

        #region Private methods

        static async Task<T> GetAsync<T>(string filename, CancellationToken ct = default(CancellationToken)) where T : class
        {
            try
            {
                await semaphores[filename].WaitAsync();

                if (objectCache.ContainsKey(filename))
                {
                    return (T)objectCache[filename];
                }

                var fileExists = await CommonConfig.DataFolder.CheckExistsAsync(filename, ct);
                if (fileExists == ExistenceCheckResult.FileExists)
                {
                    var file = await CommonConfig.DataFolder.GetFileAsync(filename, ct);
                    return await SerializationUtils.DeserializeAsync<T>(await file.ReadAllTextAsync());
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
                {
                    await CommonConfig.DataFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting, ct);
                }

                var file = await CommonConfig.DataFolder.GetFileAsync(filename, ct);
                await file.WriteAllTextAsync(await SerializationUtils.SerializeAsync(obj));

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

