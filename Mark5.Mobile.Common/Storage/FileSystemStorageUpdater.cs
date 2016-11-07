//
// Project: Mark5.Mobile.Common
// File: FileSystemStorageUpdater.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System.Threading.Tasks;
using PCLStorage;

namespace Mark5.Mobile.Common.Storage
{

    public static class FileSystemStorageUpdater
    {

        const int RequiredFileSystemStorageVersion = 100;
        const string FileSystemStorageVersionFile = "filesystem_version";

        public static async Task<bool> UpdateStorage()
        {
            var currentStorageVersion = await ReadCurrentStorageVersion();

            if (RequiredFileSystemStorageVersion == currentStorageVersion)
            {
                return false;
            }

            // Here add update logic for next versions.
            // Remember to be as *low-level* in update logic as possible to minimize risk of failure.

            await WriteRequiredStorageVersion();

            return true;
        }

        public static async Task<int> ReadCurrentStorageVersion()
        {
            var versionFile = await CommonConfig.DataFolder.GetFileAsync(FileSystemStorageVersionFile);
            return int.Parse(await versionFile.ReadAllTextAsync());
        }

        public static async Task WriteRequiredStorageVersion()
        {
            var versionFile = await CommonConfig.DataFolder.CreateFileAsync(FileSystemStorageVersionFile, CreationCollisionOption.ReplaceExisting);
            await versionFile.WriteAllTextAsync(RequiredFileSystemStorageVersion.ToString());
        }
    }
}
