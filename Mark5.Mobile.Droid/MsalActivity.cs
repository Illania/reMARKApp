using Android.App;
using Android.Content;
using Microsoft.Identity.Client;

namespace Mark5.Mobile.Droid
{
    [Activity]
    [IntentFilter(new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault },
        DataScheme = "msauth",
        DataHost = "com.companyname.mfatest",
        DataPath = "/ga0RGNYHvNM5d0SLGQfpQWAPGJ8=")]
    public class MsalActivity : BrowserTabActivity
    {
    }
}
