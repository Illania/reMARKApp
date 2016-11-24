//
// Project: Mark5.Mobile.Common
// File: SystemSettings.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Model
{

    public class SystemSettings
    {

        public SystemInfo SystemInfo { get; set; }

        public DocumentsModuleInfo DocumentsModuleInfo { get; set; }

        public ContactsModuleInfo ContactsModuleInfo { get; set; }

        public ShortcodesModuleInfo ShortcodesModuleInfo { get; set; }

        public CalendarModuleInfo CalendarModuleInfo { get; set; }

        public UserInfo UserInfo { get; set; }
    }
}

