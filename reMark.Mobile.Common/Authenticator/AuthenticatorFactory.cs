namespace reMark.Mobile.Common.Authenticator
{
    public static class AuthenticatorFactory
    {
        public static IAuthenticator Create()
        {
            return new Authenticator();
        }
    }
}