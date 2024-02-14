using System;
namespace reMark.Mobile.Common.Manager
{
    public static class MicrosoftGraphConstants
    {
        public const string ClientId = "ca4a3013-2f7f-4733-aa6c-126c8d34216f";
        public const string IosRedirectUri = "msauth.com.nordic-it.mark5.mobile.ios://auth";
        public const string AndroidRedirectUri = "msauth://com.nordic_it.mark5.android/dUOzGWwhv%2BzH%2F6bxqKb4ZlnNC8M%3D";
        public const string EndpointInfoExtName = "com.remark-app.endpoint";
        public const string AppProxyExtName = "com.remark-app.proxy";
        public static string[] Scopes() => new string[] {
                "User.Read",
                "Calendars.Read",
                "Calendars.ReadWrite",
                "Calendars.Read.Shared",
        };
        
    }
}

