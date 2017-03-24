//
// Project: Mark5.Mobile.Droid
// File: ActionAnimatorListenerAdapter.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Android.Animation;

namespace FastScrollRecycler
{
    
    class ActionAnimatorListenerAdapter : AnimatorListenerAdapter
    {

        readonly Action actionCancel;
        readonly Action actionEnd;

        public ActionAnimatorListenerAdapter(Action actionCancel = null, Action actionEnd = null)
        {
            this.actionCancel = actionCancel;
            this.actionEnd = actionEnd;
        }

        public override void OnAnimationCancel(Animator animation)
        {
            base.OnAnimationCancel(animation);

            if (actionCancel != null)
                actionCancel();
        }

        public override void OnAnimationEnd(Animator animation)
        {
            base.OnAnimationEnd(animation);

            if (actionEnd != null)
                actionEnd();
        }
    }
}
