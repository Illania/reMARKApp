//
// Project: Mark5.Mobile.Common
// File: UserInfo.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Model
{

    public class UserInfo
    {

        public SystemUser User { get; set; }

        public bool IsSystemAdministrator { get; set; }
    }
}

