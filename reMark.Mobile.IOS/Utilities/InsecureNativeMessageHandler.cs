using ModernHttpClient;

namespace reMark.Mobile.IOS.Utilities
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