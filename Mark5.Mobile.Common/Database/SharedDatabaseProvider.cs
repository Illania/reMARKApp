using System;
using System.Threading;
using System.Threading.Tasks;
using SQLite;


namespace Mark5.Mobile.Common.Database
{
    public class SharedDatabaseProvider
    {
        static Mutex sharedLock = new Mutex();
        readonly SQLiteConnection connection;

        public SharedDatabaseProvider(string path, string dbName)
        {
            connection = new SQLiteConnection(path + $"/" + dbName, true);
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
