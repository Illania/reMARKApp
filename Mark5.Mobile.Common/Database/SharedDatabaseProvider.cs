using Foundation;
using System.Threading;
using SQLite;

//"com.nordic-it.mark5.mobile.ios.extensions.callid"
namespace Mark5.Mobile.Common.Database
{
    public class SharedDatabaseProvider
    {
        readonly Mutex sharedLock = new Mutex();
        readonly SQLiteConnection connection;

        public SharedDatabaseProvider(string groupId)
        {
            var groupPath = NSFileManager.DefaultManager.GetContainerUrl(groupId);
            connection = new SQLiteConnection(CommonConfig.DatabaseFolder.Path + CommonConfig.PathSeparator , true);
        }
    }
}
