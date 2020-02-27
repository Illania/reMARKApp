using System;
namespace Mark5.Mobile.Common.Synchronizer
{
    public static class Synchronizers
    {
        public static ILocalRemindersSynchronizer LocalRemindersSynchronizer { get; set; } = new LocalRemindersSynchronizer();
    }
}
