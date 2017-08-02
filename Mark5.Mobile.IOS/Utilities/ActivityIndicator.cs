using UIKit;
using Foundation;

namespace Mark5.Mobile.IOS.Utilities
{
    public static class ActivityIndicator
    {
        static readonly object lockObject = new object();
        static readonly NSObject nsobject = new NSObject();

        static int counter;

        public static void Show()
        {
            nsobject.InvokeOnMainThread(() =>
            {
                lock (lockObject)
                {
                    counter++;

                    if (!UIApplication.SharedApplication.NetworkActivityIndicatorVisible)
                        UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;
                }
            });
        }

        public static void Hide()
        {
            nsobject.InvokeOnMainThread(() =>
            {
                lock (lockObject)
                {
                    counter--;

                    if (counter < 0)
                        counter = 0;

                    if (counter < 1)
                        UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
                }
            });
        }
    }
}
