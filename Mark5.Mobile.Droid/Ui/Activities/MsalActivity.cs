
using Android.App;
using Android.Content;
using Microsoft.Identity.Client;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Activity]
    [IntentFilter(new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault },
        DataScheme = "msauth",
        DataHost = "com.nordic_it.mark5.android",
        DataPath = "/dUOzGWwhv+zH/6bxqKb4ZlnNC8M=")]
    public class MsalActivity : BrowserTabActivity
    {
    }
}
