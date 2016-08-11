using System.Collections.Generic;
using SQLite;

namespace Mark5.Mobile.Common
{
    public static class SQLiteConnectionExtensions
    {
        public static void InsertOrReplaceAll<T>(this SQLiteConnection c, IEnumerable<T> list)
        {
            foreach (var item in list)
            {
                c.InsertOrReplace(item);
            }
        }
    }
}

