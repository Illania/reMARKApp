//
// Project: Mark5.Mobile.Common
// File: AuthenticatorFactory.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Authenticator
{

    public static class AuthenticatorFactory
    {

        public static IAuthenticator Create()
        {
            return new Authenticator();
        }
    }
}

