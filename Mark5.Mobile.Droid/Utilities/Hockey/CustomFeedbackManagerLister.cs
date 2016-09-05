//
// Project: Mark5.Mobile.Droid
// File: CustomFeedbackManagerLister.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using HockeyApp.Android;
using HockeyApp.Android.Objects;

namespace Mark5.Mobile.Droid.Utilities.Hockey
{

    public class CustomFeedbackManagerLister : FeedbackManagerListener
    {

        public override Java.Lang.Class FeedbackActivityClass
        {
            get
            {
                return Java.Lang.Class.FromType(typeof(CustomFeedbackActivity));
            }
        }

        public override bool FeedbackAnswered(FeedbackMessage p0)
        {
            return false;
        }
    }
}

