using System;
namespace Mark5.Mobile.Common.Database
{
    public class SharedContactsDatabaseProvider : SharedDatabaseProvider
    {
        public SharedContactsDatabaseProvider(string path) : base(path,"sharedcontacts.sqlite3") { }
    }
}
