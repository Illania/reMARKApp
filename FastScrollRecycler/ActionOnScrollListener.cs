//
// Project: Mark5.Mobile.Droid
// File: ActionOnScrollListener.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Android.Support.V7.Widget;

namespace FastScrollRecycler
{
    
    class ActionOnScrollListener : RecyclerView.OnScrollListener
    {

        readonly Action<RecyclerView, int, int, object[]> action;
        readonly object[] objects;

        public ActionOnScrollListener(Action<RecyclerView, int, int, object[]> action, params object[] objects)
        {
            this.objects = objects;
            this.action = action;
        }

        public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
        {
            base.OnScrolled(recyclerView, dx, dy);
            action(recyclerView, dx, dy, objects);
        }
    }
}
