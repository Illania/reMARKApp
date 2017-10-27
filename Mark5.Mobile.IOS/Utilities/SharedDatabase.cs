using System;
using System.Threading.Tasks;
using Foundation;
using SQLite;


namespace Mark5.Mobile.IOS.Utilities
{
    public class SharedDatabase
    {
        const string databaseFileName = "sharedcontacts.sqlite3";
        const string appGroupId = "group.com.nordic-it.mark5.mobile.ios";
        readonly NSUrl sharedDatabaseUrl;

        public SharedDatabase()
        {
            sharedDatabaseUrl = NSFileManager.DefaultManager.GetContainerUrl(appGroupId);
        }

        public async Task RunInConnectionAsync(Action<SQLiteConnection> action)
        {

        }

        public void RunInConnectionSynchronous(Action<SQLiteConnection> action)
        {

        }
    }
}
