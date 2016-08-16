//
// Project: Mark5.Mobile.Common
// File: FileSystemStorage.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Managers;
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
            public const string NotificationSettings = "notificationSettings.json";

            public const string OutgoingDocument = "document.json";
            public const string OutgoingDocumentPreview = "documentPreview.json";
            public const string OutgoingInfo = "info.json";
            public const string OugoingLock = ".lock";
            public const string OutgoingFailed = ".failed";
        }

        static readonly IDictionary<string, SemaphoreSlim> semaphores = new Dictionary<string, SemaphoreSlim>
        {
            [Filenames.ConnectionInfo] = new SemaphoreSlim(1),
            [Filenames.InstallationId] = new SemaphoreSlim(1),
            [Filenames.SystemSettings] = new SemaphoreSlim(1),
            [Filenames.SystemUserDepartments] = new SemaphoreSlim(1),
            [Filenames.FavoriteFolders] = new SemaphoreSlim(1),
            [Filenames.NotificationSettings] = new SemaphoreSlim(1)
        }.ToImmutableDictionary();

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
            return await GetAsync<Dictionary<ModuleType, List<Folder>>>(Filenames.FavoriteFolders, ct);
        }

        public static async Task SaveFavoriteFoldersAsync(Dictionary<ModuleType, List<Folder>> favoriteFolders, CancellationToken ct = default(CancellationToken))
        {
            await SaveAsync(favoriteFolders, Filenames.FavoriteFolders, ct);
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

        #region OutgoingDocuments

        public static async Task SaveOutgoingDocumentAsync(OutgoingDocumentInfo outgoingDocumentInfo, Document document, DocumentPreview documentPreview)
        {
            var outgoingDocumentFolder = await GetOutgoingFolder(outgoingDocumentInfo.Identifier.ToString());

            var documentFile = await outgoingDocumentFolder.CreateFileAsync(Filenames.OutgoingDocument, CreationCollisionOption.ReplaceExisting);
            await documentFile.WriteAllTextAsync(await SerializationUtils.SerializeAsync(document));

            var documentPreviewFile = await outgoingDocumentFolder.CreateFileAsync(Filenames.OutgoingDocumentPreview, CreationCollisionOption.ReplaceExisting);
            await documentPreviewFile.WriteAllTextAsync(await SerializationUtils.SerializeAsync(documentPreview));
            //TODO need to put those names in constants
            var infoFile = await outgoingDocumentFolder.CreateFileAsync(Filenames.OutgoingInfo, CreationCollisionOption.ReplaceExisting);
            await infoFile.WriteAllTextAsync(await SerializationUtils.SerializeAsync(outgoingDocumentInfo));
        }

        public static async Task<IEnumerable<OutgoingDocumentContainer>> GetAvailableOutgoingDocumentContainers()
        {
            var folders = await CommonConfig.OutgoingFolder.GetFoldersAsync();
            var containers = new List<OutgoingDocumentContainer>();
            foreach (var folder in folders)
            {
                var isLocked = (await folder.CheckExistsAsync(Filenames.OugoingLock)) == ExistenceCheckResult.FileExists;
                var isFailed = (await folder.CheckExistsAsync(Filenames.OutgoingFailed)) == ExistenceCheckResult.FileExists;
                var isReady = (await folder.CheckExistsAsync(Filenames.OutgoingInfo)) == ExistenceCheckResult.FileExists;
                if (isReady && !isLocked && !isFailed)
                {
                    var documentFile = await folder.GetFileAsync(Filenames.OutgoingDocument);
                    var document = await SerializationUtils.DeserializeAsync<Document>(await documentFile.ReadAllTextAsync());

                    var documentPreviewFile = await folder.GetFileAsync(Filenames.OutgoingDocumentPreview);
                    var documentPreview = await SerializationUtils.DeserializeAsync<DocumentPreview>(await documentPreviewFile.ReadAllTextAsync());

                    var infoFile = await folder.GetFileAsync(Filenames.OutgoingInfo);
                    var info = await SerializationUtils.DeserializeAsync<OutgoingDocumentInfo>(await infoFile.ReadAllTextAsync());

                    var container = new OutgoingDocumentContainer
                    {
                        Document = document,
                        DocumentPreview = documentPreview,
                        Info = info,
                    };

                    containers.Add(container);
                }
            }

            return containers;
        }

        static async Task<IFolder> GetOutgoingFolder(string identifier)
        {
            return await CommonConfig.OutgoingFolder.CreateFolderAsync(identifier, CreationCollisionOption.OpenIfExists);
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
            using (var fileStream = await file.OpenAsync(FileAccess.ReadAndWrite))
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
                return PortablePath.Combine(CommonConfig.AttachmentsFolder.Path, filename);
            }

            return string.Empty;
        }

        static string GetAttachmentFilename(AttachmentDescription attachmentDescription)
        {
            return $"{attachmentDescription.Id}_{attachmentDescription.Name}";
        }

        #endregion

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

