
using Android.App;
using Android.Content;
using Android.OS;
using Microsoft.Identity.Client;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Manager;
using reMark.Mobile.Common.Utilities;

namespace reMark.Mobile.Droid.Ui.Activities
{
    [Activity(Exported = true)]
    [IntentFilter(new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault },
        DataScheme = "msauth",
        DataHost = "com.nordic_it.mark5.android",
        DataPath = "/dUOzGWwhv+zH/6bxqKb4ZlnNC8M=")]
    public class MsalActivity : BrowserTabActivity
    {

    }
}
