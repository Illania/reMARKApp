//
// Project: Mark5.Mobile.Droid
// File: ActionOnFastScrollStateChangeListener.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;

namespace FastScrollRecycler
{
    
    public class ActionOnFastScrollStateChangeListener : IOnFastScrollStateChangeListener
    {

        readonly Action startAction;
        readonly Action stopAction;

        public ActionOnFastScrollStateChangeListener(Action startAction = null, Action stopAction = null)
        {
            this.startAction = startAction;
            this.stopAction = stopAction;
        }

        public void OnFastScrollStart()
        {
            if (startAction != null)
                startAction();
        }

        public void OnFastScrollStop()
        {
            if (stopAction != null)
                stopAction();
        }
    }
}
