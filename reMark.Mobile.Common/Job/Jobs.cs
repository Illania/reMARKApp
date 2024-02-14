namespace reMark.Mobile.Common.Job
{
    public static class Jobs
    {
        public static ISystemSettingsUpdateJob SystemSettingsUpdateJob { get; } = new SystemSettingsUpdateJob();
    }
}
