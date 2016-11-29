//
// Project: Mark5.Mobile.Common
// File: SystemInfo.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Model
{

    public class SystemInfo
    {

        public Version SystemVersion { get; set; }

        public Version ServiceVersion { get; set; }

        List<ModuleType> availableModules;

        public List<ModuleType> AvailableModules
        {
            get
            {
                if (availableModules == null)
                {
                    availableModules = new List<ModuleType>();
                }
                return availableModules;
            }
            set
            {
                availableModules = value;
            }
        }

        public TimeSpan ServerUtcOffset { get; set; }
    }
}

