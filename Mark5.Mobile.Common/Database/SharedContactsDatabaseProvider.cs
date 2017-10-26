using System;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Database
{
    public class SharedContactsDatabaseProvider : SharedDatabaseProvider
    {
        public SharedContactsDatabaseProvider(string path) : base(path,"sharedcontacts.sqlite3") 
        {
            RunInConnectionSynchronous(c => {
                c.CreateTable<Contact>();
            });
        }
    }
}
