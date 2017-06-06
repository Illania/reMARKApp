using ModernHttpClient;

namespace Mark5.Mobile.IOS.Utilities
{
    public class InsecureNativeMessageHandler : NativeMessageHandler
    {
        public InsecureNativeMessageHandler()
            : base(false, true)
        {
            DisableCaching = true;
        }
    }
}