//
// Project: Mark5.Mobile.Common
// File: CalendarModuleInfo.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Model
{

    public class CalendarModuleInfo
    {

        List<CalendarCategory> calendarCategories;

        public List<CalendarCategory> CalendarCategories
        {
            get
            {
                if (calendarCategories == null)
                {
                    calendarCategories = new List<CalendarCategory>();
                }
                return calendarCategories;
            }

            set
            {
                calendarCategories = value;
            }
        }

        List<CalendarResource> calendarResources;

        public List<CalendarResource> CalendarResources
        {
            get
            {
                if (calendarResources == null)
                {
                    calendarResources = new List<CalendarResource>();
                }

                return calendarResources;
            }

            set
            {
                calendarResources = value;
            }
        }

        public Permissions Permissions { get; set; }
    }
}

