using System;
using System.Threading.Tasks;
using Foundation;
using SQLite;

using static Mark5.Mobile.IOS.Common.CallId.CallIdDatabaseLock;

namespace Mark5.Mobile.IOS.Common.CallId
{
    /// <summary>
    /// Contains utility methods for creating and cleaning the database in the shared container and also 
    /// for wiping the contents of the container.
    /// </summary>
    public static class CallIdContainerUtilities
    {
        public const string DatabaseName = "sharedcontacts.sqlite3";
        public const string AppGroupId = "group.com.nordic-it.mark5.mobile.ios";

        public static async Task CreateExtensionContactsTable()
        {
            await Task.Run(() =>
            {
                try
                {
                    LockDatabase();

                    using (var containerUrl = NSFileManager.DefaultManager.GetContainerUrl(AppGroupId))
                    {
                        var fullDatabaseUrl = containerUrl.Append(DatabaseName, false);

                        using (var connection = new SQLiteConnection(fullDatabaseUrl.Path, true))
                        {
                            connection.CreateTable<ExtensionContact>();
                        }
                    }
                }
                finally
                {
                    UnlockDatabase();
                }
            });
        }

        public static void WipeContainer()
        {
            var fm = NSFileManager.DefaultManager;
            try
            {
                using (var containerUrl = fm.GetContainerUrl(AppGroupId))
                {
                    LockDatabase();

                    if (containerUrl.Path != null)
                    {
                        var filesInDir = fm.GetDirectoryContent(containerUrl.Path, out NSError err);
                        if (err != null)
                            throw new NSErrorException(err);

                        foreach (string s in filesInDir)
                        {
                            var pathToRemove = containerUrl.Path + "/" + s;

                            if (fm.FileExists(pathToRemove))
                            {
                                if (s.Contains("log") || s == DatabaseName) //Only wipe log files and database.
                                {
                                    fm.Remove(pathToRemove, out err);
                                }
                                if (err != null)
                                {
                                    throw new Exception("Error wiping shared container: " + err.LocalizedFailureReason);
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                UnlockDatabase();
            }
        }
    }
}
