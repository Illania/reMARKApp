namespace Mark5.Mobile.Common.Job
{
    public static class Jobs
    {
        public static IRemindersUpdateJob RemindersUpdateJob { get; } = new RemindersUpdateJob();
        public static ISystemSettingsUpdateJob SystemSettingsUpdateJob { get; } = new SystemSettingsUpdateJob();
    }
}
