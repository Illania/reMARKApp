using System;
using Foundation;
using Mark5.Mobile.IOS.Common.Exceptions;

namespace Mark5.Mobile.IOS.Common.CallId
{
    public static class CallIdDatabaseLock
    {
        const string databaseLockName = "sharedcontacts.lock";

        public static void LockDatabase()
        {
            var fm = NSFileManager.DefaultManager;
            using (var containerUrl = fm.GetContainerUrl(CallIdContainerUtilities.appGroupId))
            {
                var lockPath = containerUrl.Append(databaseLockName, false).Path;

                if (fm.FileExists(lockPath))
                    throw new DatabaseLockException("The database is locked, unable to get lock.");

                fm.CreateFile(lockPath, new NSData(), new NSFileAttributes());
            }
        }

        public static void UnlockDatabase()
        {
            var fm = NSFileManager.DefaultManager;
            using (var containerUrl = fm.GetContainerUrl(CallIdContainerUtilities.appGroupId))
            {
                var lockPath = containerUrl.Append(databaseLockName, false).Path;

                if (fm.FileExists(lockPath))
                {
                    NSError error = new NSError();
                    fm.Remove(lockPath, out error);
                    if (error != null)
                    {
                        throw new NSErrorException(error);
                    }
                }
                else
                    throw new DatabaseLockException("The database is locked, unable to get lock.");
            }
        }
    }
}
