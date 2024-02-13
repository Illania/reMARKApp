using System;
using Android.Views;
using View = Android.Views.View;

namespace reMark.Mobile.Droid.Ui.Common
{
    public class ActionOnLongClickListener : Java.Lang.Object, View.IOnLongClickListener
    {
        readonly Action action;

        public ActionOnLongClickListener(Action action)
        {
            this.action = action;
        }

        public bool OnLongClick(View v)
        {
            if (action != null)
            {
                action();
                return true;
            }

            return false;
        }
    }
}