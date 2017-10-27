using System;
using System.Threading;
using System.Threading.Tasks;
using SQLite;


namespace Mark5.Mobile.IOS.Utilities
{
    public class SharedDatabaseProvider
    {
        const string databaseFileName = "sharedcontacts.sqlite3";
        const string appGroupId = "group.com.nordic-it.mark5.mobile.ios";
        readonly string sharedDatabaseUrl;

        public SharedDatabaseProvider(string path, string dbName)
        {
            sharedDatabaseUrl = NSFileManager.DefaultManager.GetContainerUrl(appGroupId);
        }

        public async Task RunInConnectionAsync(Action<SQLiteConnection> action)
        {
            await Task.Run(() =>
            {
                try
                {
                    //sharedLock.WaitOne();

                    connection.RunInTransaction(() => { action(connection); });
                }
                finally
                {
                    //sharedLock.ReleaseMutex();
                }
            });
        }

        public void RunInConnectionSynchronous(Action<SQLiteConnection> action)
        {
            //sharedLock.WaitOne();
            try
            {
                connection.RunInTransaction(() =>
                {
                    try
                    {
                        action(connection);
                    }
                    catch (Exception ex)
                    {

                    }
                });
            }
            catch (Exception ex)
            {

            }
            //sharedLock.ReleaseMutex();
        }
    }
}
