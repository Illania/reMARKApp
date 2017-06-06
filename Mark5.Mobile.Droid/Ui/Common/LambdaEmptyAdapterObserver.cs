//
// Project: Mark5.Mobile.Droid
// File: Formatters.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System;
using Android.Support.V7.Widget;

namespace Mark5.Mobile.Droid.Ui.Common
{
    public class LambdaEmptyAdapterObserver : RecyclerView.AdapterDataObserver
    {
        readonly Action action;

        public LambdaEmptyAdapterObserver(Action action)
        {
            this.action = action;
        }

        public override void OnChanged()
        {
            action();
        }

        public override void OnItemRangeInserted(int positionStart, int itemCount)
        {
            action();
        }

        public override void OnItemRangeRemoved(int positionStart, int itemCount)
        {
            action();
        }
    }
}