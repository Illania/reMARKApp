//
// Project: 
// File: ActionOnCreateContextMenuListener.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Common
{
    public class ActionOnCreateContextMenuListener : Java.Lang.Object, View.IOnCreateContextMenuListener
    {
        readonly Action<IContextMenu, View, IContextMenuContextMenuInfo> action;

        public ActionOnCreateContextMenuListener(Action<IContextMenu, View, IContextMenuContextMenuInfo> action)
        {
            this.action = action;
        }

        public void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
            if (action != null)
            {
                action(menu, v, menuInfo);
            }
        }
    }
}
