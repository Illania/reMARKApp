using System;
using Android.Views;
using View = Android.Views.View;

namespace reMark.Mobile.Droid.Ui.Common
{
    public class ActionOnClickListener : Java.Lang.Object, View.IOnClickListener
    {
        readonly Action action;

        public ActionOnClickListener(Action action)
        {
            this.action = action;
        }

        public void OnClick(View v)
        {
            if (action != null)
                action();
        }
    }
}