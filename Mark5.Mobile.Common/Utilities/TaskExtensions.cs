//
// Project: Mark5.Mobile.Common
// File: TaskExtensions.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Threading.Tasks;

namespace Mark5.Mobile.Common.Utilities
{

    public static class TaskExtensions
    {

        public static async void FireAndForget(this Task task)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception e)
            {
                // TODO log errors
            }
        }
    }
}

