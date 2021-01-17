namespace Mark5.Mobile.Droid.Service
{
    public static class PushNotificationsConstants
    {
        public static PushNotificationsProviderType PushNotificationsProviderType { get; set; }

    }

    public enum PushNotificationsProviderType
    {
        Firebase = 0,
        Pushy = 1
    }
}
