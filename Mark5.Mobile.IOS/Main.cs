//
// Project: Mark5.Mobile.IOS
// File: Application.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Threading;
using UIKit;

namespace Mark5.Mobile.IOS
{
    public class Application
    {
        
        static void Main(string[] args)
        {
            int workerThreads, completionPortThreads;
            ThreadPool.GetMinThreads(out workerThreads, out completionPortThreads);
            ThreadPool.SetMinThreads(workerThreads * 5, completionPortThreads * 5);

            UIApplication.Main(args, null, "AppDelegate");
        }
    }
}
